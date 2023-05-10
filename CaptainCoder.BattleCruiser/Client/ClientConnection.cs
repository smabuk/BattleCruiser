using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using MQTTnet;
using MQTTnet.Client;

namespace CaptainCoder.BattleCruiser.Client;

public class ClientConnection
{
    private bool _requestDisconnect = false;
    private ConcurrentQueue<INetworkMessage> _outbox = new();
    private ConcurrentQueue<INetworkMessage> _inbox = new();
    private MqttFactory _mqttFactory = new();
    public ClientConnection(string host, int port) => (Host, Port) = (host, port);
    public string Host { get; }
    public int Port { get; }
    public bool IsConnected { get; private set; } = false;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action OnConnecting;
    public event Action<INetworkMessage> OnMessageReceived;

    public async Task ConnectAndProcessMessages()
    {
        using var mqttClient = _mqttFactory.CreateMqttClient();
        ConnectEvents(mqttClient);
        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(Host, Port).Build();
        var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        Console.WriteLine("The MQTT client is connected.");
        await SubscribeToTopic("private/clientId", mqttClient);
        await SubscribeToTopic("public/server", mqttClient);
        // TODO: FISHY SMELL (stinky code) -- connect shouldn't also disconnect?
        await ProcessMessages(mqttClient);

        var mqttClientDisconnectOptions = _mqttFactory.CreateClientDisconnectOptionsBuilder().Build();
        await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
    }

    private async Task ProcessMessages(IMqttClient client)
    {
        while (true)
        {
            IResult<INetworkMessage> action = await DequeueOutbox();
            if (action is Disconnect<INetworkMessage>) { break; }
            if (action is Message<INetworkMessage> success)
            {
                await SendMessage(success.Value, client);
            }
            else
            {
                throw new InvalidCastException($"Could not cast {action} to message.");
            }
        }
    }

    private void ConnectEvents(IMqttClient client)
    {
        client.ConnectedAsync += ForwardConnectedAsync;
        client.DisconnectedAsync += ForwardDisconnectedAsync;
        client.ConnectingAsync += ForwardConnectingAsync;
        client.ApplicationMessageReceivedAsync += ForwardMessageReceivedAsync;
    }

    public void RequestDisconnect() => _requestDisconnect = true;

    private async Task SubscribeToTopic(string topic, IMqttClient client)
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

    private Task ForwardDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        IsConnected = false;
        return Task.Run(() => OnDisconnected?.Invoke());
    }

    public async Task<IResult<INetworkMessage>> DequeueOutbox()
    {
        INetworkMessage? action = null;
        Console.WriteLine("Waiting for next action");
        while (!_outbox.TryDequeue(out action))
        {
            // TODO: Examine ManualResetEvent instead: 
            // https://learn.microsoft.com/en-us/dotnet/api/system.threading.manualresetevent?view=netstandard-2.1
            await Task.Delay(100);
            if (_requestDisconnect)
            {
                return new Disconnect<INetworkMessage>();
            }
        }
        Console.WriteLine($"Next Action was: {action}");
        return new Message<INetworkMessage>(action);
    }

    public void EnqueueMessage(INetworkMessage toSend)
    {
        Console.WriteLine($"Enqueuing: {toSend}");
        _outbox.Enqueue(toSend);
    }

    private Task ForwardMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        return Task.Run(() =>
        {
            byte[] payload = args.ApplicationMessage.PayloadSegment.ToArray();
            INetworkMessage incommingMessage = NetworkSerializer.Deserialize<INetworkMessage>(payload);
            _inbox.Enqueue(incommingMessage);
        });
    }

    private Task ForwardConnectingAsync(MqttClientConnectingEventArgs args) => Task.Run(() => OnConnecting?.Invoke());
    // if (ConnectingAsync == null) { return null } else { return ConnectingAsync.Invoke(args); }

    private Task ForwardConnectedAsync(MqttClientConnectedEventArgs args)
    {
        IsConnected = true;
        _ = Task.Run(() => OnConnected?.Invoke());
        return Task.CompletedTask;
    }

    private async Task SendMessage(INetworkMessage toSend, IMqttClient client)
    {
        byte[] payload = NetworkSerializer.Serialize(toSend);
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic("private/server")
                .WithPayload(payload)
                .Build();
        await client.PublishAsync(message);
    }

}