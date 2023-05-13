using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using MQTTnet;
using MQTTnet.Client;

namespace CaptainCoder.BattleCruiser.Client;

public class PlayerClient : AbstractClient
{
    public PlayerClient(string host, int port) : base(host, port)
    {
        OnConnected += () => Console.WriteLine("Connected!");
        OnConnecting += () => Console.WriteLine("Connecting...");
        OnDisconnected += () => Console.WriteLine("Disconnected!");
        OnMessageReceived += PrintNetworkMessage;
    }

    protected override async Task ConnectSubscriptions(IMqttClient client)
    {
        await SubscribeToTopic("private/clientId", client);
        await SubscribeToTopic("public/hostId", client);
    }
    void PrintNetworkMessage(NetworkMessage message)
    {
        Console.WriteLine($"Received Message From: {message.ClientId}");
        Console.WriteLine($"Message was: {message.Payload}");
    }
}