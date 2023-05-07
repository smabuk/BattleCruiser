using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using MQTTnet;
using MQTTnet.Client;

namespace CaptainCoder.BattleCruiser.Client;

public class ClientConnection
{
    private ConcurrentQueue<string> _messages = new ();
    private MqttFactory _mqttFactory = new ();
    public ClientConnection(string host, int port) => (Host, Port) = (host, port);

    public string Host { get; }
    public int Port { get; }
    public bool IsConnected { get; private set; } = false;

    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action OnConnecting;
    public event Action<IServerMessage> OnMessageReceived;
    

    public async Task Connect()
    {
        /*
         * This sample creates a simple MQTT client and connects to a public broker.
         *
         * Always dispose the client when it is no longer used.
         * The default version of MQTT is 3.1.1.
         */
        

        using var mqttClient = _mqttFactory.CreateMqttClient();
        
        // Use builder classes where possible in this project.
        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(Host, Port).Build();
        mqttClient.ConnectedAsync += ForwardConnectedAsync;
        mqttClient.DisconnectedAsync += ForwardDisconnectedAsync;
        mqttClient.ConnectingAsync += ForwardConnectingAsync;
        mqttClient.ApplicationMessageReceivedAsync += ForwardMessageReceivedAsync;

        // This will throw an exception if the server is not available.
        // The result from this message returns additional data which was sent 
        // from the server. Please refer to the MQTT protocol specification for details.
        
        var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        Console.WriteLine("The MQTT client is connected.");

        var mqttSubscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("chat");
                    })
                .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
        

        while (true)
        {
            string action = await NextAction();
            if (action == "END")
            {
                break;
            }
            await SendStringMessage(action, mqttClient);
        }
        // response.DumpToConsole();

        // Send a clean disconnect to the server by calling _DisconnectAsync_. Without this the TCP connection
        // gets dropped and the server will handle this as a non clean disconnect (see MQTT spec for details).
        var mqttClientDisconnectOptions = _mqttFactory.CreateClientDisconnectOptionsBuilder().Build();
        await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
        
    }

    private Task ForwardDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        IsConnected = false;
        return Task.Run(() => OnDisconnected?.Invoke());
    }

    public async Task<string> NextAction()
    {
        string action = null!;
        Console.WriteLine("Waiting for next action");
        while(!_messages.TryDequeue(out action))
        {
            await Task.Delay(1000);
        }
        Console.WriteLine($"Next Action was: {action}");
        return action;

    }

    public void EnqueueMessage(string message)
    {
        Console.WriteLine($"Enqueuing: {message}");
        _messages.Enqueue(message);
    }


    private Task ForwardMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        // Task.Run(() => MessageReceived.Invoke(args))
        return Task.Run(() =>
        {
            // ServerMessage message = new (args.ApplicationMessage);
            // OnMessageReceived?.Invoke(message);
        });
    } 

    private Task ForwardConnectingAsync(MqttClientConnectingEventArgs args)  => Task.Run(() => OnConnecting?.Invoke());
        // if (ConnectingAsync == null) { return null } else { return ConnectingAsync.Invoke(args); }
        
    private Task ForwardConnectedAsync(MqttClientConnectedEventArgs args)
    {
        IsConnected = true;     
        _ = Task.Run(() => OnConnected?.Invoke());
        return Task.CompletedTask;
    } 
    
    private async Task SendStringMessage(string toSend, IMqttClient  client)
    {
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic("chat")
                .WithPayload(toSend)
                .Build();
        await client.PublishAsync(message);
    }

}