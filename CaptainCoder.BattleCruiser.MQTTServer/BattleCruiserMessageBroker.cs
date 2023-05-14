using MQTTnet;
using MQTTnet.Server;
using System.Collections.Concurrent;

namespace CaptainCoder.BattleCruiser.Server;

public class BattleCruiserMessageBroker
{
    private ConcurrentDictionary<string, bool> _usernames = new ();
    private ConcurrentDictionary<string, string> _clientIdToUserName = new ();
    public BattleCruiserMessageBroker(int port, bool logging = false) => (Port, Logging) = (port, logging);
    public int Port { get; }
    public bool Logging { get; }
    private MqttServer? _server;

    public async Task Start()
    {
         /*
         * This sample starts a simple MQTT server and prints the logs to the output.
         *
         * IMPORTANT! Do not enable logging in live environment. It will decrease performance.
         *
         * See sample "Run_Minimal_Server" for more details.
         */

        var mqttFactory = Logging ? new MqttFactory(new ConsoleLogger()) : new MqttFactory();

        var mqttServerOptions = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(Port)
            .Build();

        using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
        {
            _server = mqttServer;
            mqttServer.ClientConnectedAsync += OnClientConnect;
            mqttServer.ValidatingConnectionAsync += OnValidateConnection;
            mqttServer.ClientDisconnectedAsync += OnDisconnect;
            mqttServer.InterceptingPublishAsync += OnPublish;
            mqttServer.InterceptingSubscriptionAsync += OnSubscribe;
            await mqttServer.StartAsync();

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            // Stop and dispose the MQTT server if it is no longer needed!
            await mqttServer.StopAsync();
            _server = null;
        }
    }

    private Task OnSubscribe(InterceptingSubscriptionEventArgs args)
    {
        
        string[] topics = args.TopicFilter.Topic.Split("/");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"On Subscribe: {args.TopicFilter.Topic} ... {string.Join(", ", topics)}");
        Console.ResetColor();
        if (topics.Length != 2)
        {
            args.ProcessSubscription = false;
            args.ReasonString = "Invalid topic. Expected public/private followed by a Username. E.g. public/UserName or private/UserName";
            return Task.CompletedTask;
        }

        if (topics[0] == "public")
        {
            return Task.CompletedTask;
        }
        if (topics[0] != "private")
        {
            args.ProcessSubscription = false;
            args.ReasonString = "Invalid topic. Expected public/private followed by a Username. E.g. public/UserName or private/UserName";
            return Task.CompletedTask;
        }
        // You can only subscribe to your own private channel *IF* you are that user
        if(!_clientIdToUserName.TryGetValue(args.ClientId, out string? username))
        {
            args.ProcessSubscription = false;
            args.ReasonString = "Could not validate username. Please reconnect to server.";
            return Task.CompletedTask;
        }

        if (topics[1] != username)
        {
            args.ProcessSubscription = false;
            args.ReasonString = "You may only subscribe to your own private channel.";
            Console.WriteLine($"Sub failed");
            Console.ResetColor();
            return Task.CompletedTask;
        }

        Console.WriteLine($"Successfully subscribed to {args.TopicFilter.Topic}");
        return Task.CompletedTask;
    }

    private Task OnPublish(InterceptingPublishEventArgs args)
    {
        throw new NotImplementedException();
    }

    private Task OnValidateConnection(ValidatingConnectionEventArgs args)
    {
        if (!_usernames.TryAdd(args.UserName, true))
        {
            args.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
            args.ReasonString = $"The specified username {args.UserName} is already in use.";
            return Task.CompletedTask;
        }
        
        if (!_clientIdToUserName.TryAdd(args.ClientId, args.UserName))
        {
            args.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.ClientIdentifierNotValid;
            args.ReasonString = $"The specified clientID is not valid.";
            _usernames.Remove(args.UserName, out _);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    private Task OnDisconnect(ClientDisconnectedEventArgs args)
    {
        if(_clientIdToUserName.Remove(args.ClientId, out string? username))
        {
            _usernames.Remove(username, out _);
        }
        return Task.CompletedTask;
    }

    private Task OnClientConnect(ClientConnectedEventArgs args)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("CLIENT CONNECTED");
        Console.ResetColor();
        return Task.CompletedTask;
    }
}