using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

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
    private string? _clientId;

    private ILogger? _logger;
    private ILogger Logger 
    { 
        get
        {
            if (_logger == null)
            {
                _logger = LoggerFactory.Create(
                    config => config.AddConsole()
                ).CreateLogger("Client");
            }
            return _logger;
        }
    }

    public AbstractClient(string host, int port, string username) => (Host, Port, UserName) = (host, port, username);

    public bool IsLogging { get; set; } = false;
    public string Host { get; }
    public int Port { get; }
    public string UserName { get; }
    
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action OnConnecting;
    public event Action<NetworkMessage> OnMessageReceived;

    public async Task Connect()
    {
        using IMqttClient mqttClient = _mqttFactory.CreateMqttClient();
        ConnectEvents(mqttClient);
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(Host, Port)
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
            .WithCredentials(UserName, "")
            .Build();
        var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        await ConnectSubscriptions(mqttClient);
        // TODO: FISHY SMELL (stinky code) -- connect shouldn't also disconnect?
        await ProcessMessages(mqttClient);

        var mqttClientDisconnectOptions = _mqttFactory.CreateClientDisconnectOptionsBuilder().Build();
        await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
    }

    protected virtual async Task ConnectSubscriptions(IMqttClient client)
    {
        MqttClientSubscribeResult result = await SubscribeToTopic($"private/{UserName}", client);
        if (result.ReasonString != null) { Console.WriteLine(result.ReasonString); }
    }

    protected async Task<MqttClientSubscribeResult> SubscribeToTopic(string topic, IMqttClient client)
    {
        var subscribeInfo = _mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(topic);
                    })
                .Build();
        return await client.SubscribeAsync(subscribeInfo, CancellationToken.None);
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
        Log($"EnqueuingMessage: {toSend}");
        _outbox.Enqueue(new OutboxMessage(toSend, topic));
    }

    public void RequestDisconnect() => _requestDisconnect = true;
    private async Task<IResult<OutboxMessage>> DequeueOutbox()
    {
        OutboxMessage? message = null;
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
        return new Message<OutboxMessage>(message);
    }

    private Task ForwardMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        return Task.Run(() =>
        {
            MqttApplicationMessage message =  args.ApplicationMessage;
            byte[] payload = message.PayloadSegment.ToArray();
            INetworkPayload networkPayload = NetworkSerializer.Deserialize<INetworkPayload>(payload);
            NetworkMessage networkMessage = new NetworkMessage(args.ClientId, networkPayload);
            Log($"Received Message: {networkMessage}");
            OnMessageReceived?.Invoke(networkMessage);
        });
    }

    private Task ForwardDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        Log("Disconnected");
        return Task.Run(() => OnDisconnected?.Invoke());
    }

    private Task ForwardConnectingAsync(MqttClientConnectingEventArgs args)
    {
        Log("Connecting");
        OnConnecting?.Invoke();
        return Task.CompletedTask;
    }

    private Task ForwardConnectedAsync(MqttClientConnectedEventArgs args)
    {
        Log($"Connected!");
        OnConnected?.Invoke();
        return Task.CompletedTask;
    }

    private async Task SendMessage(OutboxMessage toSend, IMqttClient client)
    {
        byte[] payload = NetworkSerializer.Serialize(toSend.Message);
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic(toSend.Topic)
                .WithPayload(payload)
                .Build();
        MqttClientPublishResult result = await client.PublishAsync(message);
        Log($"Sent message: {toSend}");
        Log($"Success: {result.IsSuccess}");
    }

    private void Log(string message)
    {
        if (!IsLogging) { return; }
        Logger.LogInformation(message);
    }
}