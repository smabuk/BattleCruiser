using System.Collections.Concurrent;
using MQTTnet;
using MQTTnet.Client;

namespace CaptainCoder.BattleCruiser.Client;

internal record OutboxMessage(INetworkPayload Message, string Topic);
public abstract class AbstractClient : IClient
{
    private bool _requestDisconnect = false;
    private MqttFactory _mqttFactory = new();
    private ConcurrentQueue<OutboxMessage> _outbox = new();

    public AbstractClient(string host, int port) => (Host, Port) = (host, port);
    public string Host { get; }
    public int Port { get; }
    
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action OnConnecting;
    public event Action<NetworkMessage> OnMessageReceived;

    public async Task Connect()
    {
        using IMqttClient mqttClient = _mqttFactory.CreateMqttClient();
        ConnectEvents(mqttClient);
        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(Host, Port).Build();
        var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        Console.WriteLine("The MQTT client is connected.");
        await ConnectSubscriptions(mqttClient);
        // TODO: FISHY SMELL (stinky code) -- connect shouldn't also disconnect?
        await ProcessMessages(mqttClient);

        var mqttClientDisconnectOptions = _mqttFactory.CreateClientDisconnectOptionsBuilder().Build();
        await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
    }

    protected abstract Task ConnectSubscriptions(IMqttClient client);

    protected async Task SubscribeToTopic(string topic, IMqttClient client)
    {
        var subscribeInfo = _mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(topic);
                    })
                .Build();
        await client.SubscribeAsync(subscribeInfo, CancellationToken.None);
    }

    private void ConnectEvents(IMqttClient client)
    {
        client.ConnectedAsync += ForwardConnectedAsync;
        client.DisconnectedAsync += ForwardDisconnectedAsync;
        client.ConnectingAsync += ForwardConnectingAsync;
        client.ApplicationMessageReceivedAsync += ForwardMessageReceivedAsync;
    }

    private async Task ProcessMessages(IMqttClient client)
    {
        while (true)
        {
            IResult<OutboxMessage> action = await DequeueOutbox();
            if (action is Disconnect<OutboxMessage>) { break; }
            if (action is Message<OutboxMessage> success)
            {
                await SendMessage(success.Value, client);
            }
            else
            {
                throw new InvalidCastException($"Could not cast {action} to message.");
            }
        }
    }

    public void EnqueueMessage(INetworkPayload toSend, string topic)
    {
        Console.WriteLine($"Enqueuing: {toSend}");
        _outbox.Enqueue(new OutboxMessage(toSend, topic));
    }

    public void RequestDisconnect() => _requestDisconnect = true;
    private async Task<IResult<OutboxMessage>> DequeueOutbox()
    {
        OutboxMessage? message = null;
        Console.WriteLine("Waiting for next action");
        while (!_outbox.TryDequeue(out message))
        {
            // TODO: Examine ManualResetEvent instead: 
            // https://learn.microsoft.com/en-us/dotnet/api/system.threading.manualresetevent?view=netstandard-2.1
            await Task.Delay(100);
            if (_requestDisconnect)
            {
                return new Disconnect<OutboxMessage>();
            }
        }
        Console.WriteLine($"Next Action was: {message}");
        return new Message<OutboxMessage>(message);
    }

    private Task ForwardMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        return Task.Run(() =>
        {
            MqttApplicationMessage message =  args.ApplicationMessage;
            byte[] payload = message.PayloadSegment.ToArray();
            INetworkPayload networkPayload = NetworkSerializer.Deserialize<INetworkPayload>(payload);
            OnMessageReceived?.Invoke(new NetworkMessage(args.ClientId, networkPayload));
        });
    }

    private Task ForwardDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        return Task.Run(() => OnDisconnected?.Invoke());
    }

    private Task ForwardConnectingAsync(MqttClientConnectingEventArgs args) => Task.Run(() => OnConnecting?.Invoke());
    // if (ConnectingAsync == null) { return null } else { return ConnectingAsync.Invoke(args); }

    private Task ForwardConnectedAsync(MqttClientConnectedEventArgs args)
    {
        _ = Task.Run(() => OnConnected?.Invoke());
        return Task.CompletedTask;
    }

    private async Task SendMessage(OutboxMessage toSend, IMqttClient client)
    {
        byte[] payload = NetworkSerializer.Serialize(toSend.Message);
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic(toSend.Topic)
                .WithPayload(payload)
                .Build();
        await client.PublishAsync(message);
    }
}