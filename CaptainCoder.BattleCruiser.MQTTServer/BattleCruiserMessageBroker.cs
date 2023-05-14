using MQTTnet;
using MQTTnet.Server;
using System.Collections.Concurrent;

namespace CaptainCoder.BattleCruiser.Server;

public class BattleCruiserMessageBroker
{
    private ConcurrentDictionary<string, bool> _usernames = new();
    private ConcurrentDictionary<string, string> _clientIdToUserName = new();
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
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"Subscription request: {args.TopicFilter.Topic}");
        if (!ValidateSubscription(args))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Subscription failed: {args.ReasonString}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Subscription successful!");
        }
        Console.ResetColor();
        return Task.CompletedTask;
    }

    private bool ValidateSubscription(InterceptingSubscriptionEventArgs args)
    {
        string[] topics = args.TopicFilter.Topic.Split("/");
        if (topics.Length != 2)
        {
            args.ProcessSubscription = false;
            args.ReasonString = "Invalid topic. Expected public/private followed by a Username. E.g. public/UserName or private/UserName";
            return false;
        }

        if (topics[0] == "public") { return true; }
        if (topics[0] != "private")
        {
            args.ProcessSubscription = false;
            args.ReasonString = "Invalid topic. Expected public/private followed by a Username. E.g. public/UserName or private/UserName";
            return false;
        }
        // You can only subscribe to your own private channel *IF* you are that user
        if (!_clientIdToUserName.TryGetValue(args.ClientId, out string? username))
        {
            args.ProcessSubscription = false;
            args.ReasonString = "Could not validate username. Please reconnect to server.";
            return false;
        }

        if (topics[1] != username)
        {
            args.ProcessSubscription = false;
            args.ReasonString = "You may only subscribe to your own private channel.";
            return false;
        }

        return true;
    }

    private Task OnPublish(InterceptingPublishEventArgs args)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"Publish request: {args.ApplicationMessage.Topic}");
        if (!ValidatePublish(args, out string? username))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            args.Response.ReasonCode = MQTTnet.Protocol.MqttPubAckReasonCode.TopicNameInvalid;
            Console.WriteLine($"Publish failed: {args.Response.ReasonString}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Subscription successful!");
            args.ApplicationMessage.ResponseTopic = $"private/{username}";
        }
        Console.ResetColor();
        return Task.CompletedTask;
    }

    private bool ValidatePublish(InterceptingPublishEventArgs args, out string? username)
    {
        string[] topics = args.ApplicationMessage.Topic.Split("/");

        if (!_clientIdToUserName.TryGetValue(args.ClientId, out username))
        {
            args.ProcessPublish = false;
            args.Response.ReasonString = "Could not validate username. Please reconnect to server.";
            return false;
        }

        if (topics.Length != 2)
        {
            args.ProcessPublish = false;

            args.Response.ReasonString = "Invalid topic. Expected public/private followed by a Username. E.g. public/UserName or private/UserName";
            return false;
        }

        if (topics[0] == "private") { return true; }
        if (topics[0] != "public")
        {
            args.ProcessPublish = false;
            args.Response.ReasonString = "Invalid topic. Expected public/private followed by a Username. E.g. public/UserName or private/UserName";
            return false;
        }
        // You can only subscribe to your own private channel *IF* you are that user


        if (topics[1] != username)
        {
            args.ProcessPublish = false;
            args.Response.ReasonString = "You may only publish to your own public channel.";
            return false;
        }

        return true;
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
        if (_clientIdToUserName.Remove(args.ClientId, out string? username))
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