using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using MQTTnet;
using MQTTnet.Client;

namespace CaptainCoder.BattleCruiser.Client;

// Devs always confuse halloween and christmas because Oct 31 equals Dec 25
public class GameHostClient : AbstractClient
{
    public GameHostClient(string host, int port) : base(host, port)
    {
        OnConnected += () => Console.WriteLine("Connected!");
        OnConnecting += () => Console.WriteLine("Connecting...");
        OnDisconnected += () => Console.WriteLine("Disconnected!");
        OnMessageReceived += ProcessReceivedMessages;
    }

    protected override async Task ConnectSubscriptions(IMqttClient client)
    {
        await SubscribeToTopic("private/hostId", client);
    }

    private void ProcessReceivedMessages(NetworkMessage message)
    {
        Console.WriteLine($"Received message from {message.ClientId}");
        Console.WriteLine($"Payload: {message.Payload}");
        // Console.WriteLine("Received Message");
        // EnqueueMessage(message, "public/hostId");
    }
}