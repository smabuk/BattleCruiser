using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using MQTTnet;
using MQTTnet.Client;

namespace CaptainCoder.BattleCruiser.Client;

public class PlayerClient : AbstractClient
{
    public PlayerClient(string host, int port, string username, string hostname) : base(host, port, username)
    {
        OnConnected += () => Console.WriteLine($"Connected as Player");
        OnConnecting += () => Console.WriteLine("Connecting...");
        OnDisconnected += () => Console.WriteLine("Disconnected!");
        HostName = hostname;
    }

    public string HostName { get; }

    protected override async Task ConnectSubscriptions(IMqttClient client)
    {
        await base.ConnectSubscriptions(client);
        await SubscribeToTopic($"public/{HostName}", client);
    }
}