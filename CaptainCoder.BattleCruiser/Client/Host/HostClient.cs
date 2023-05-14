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
    public GameHostClient(string host, int port, string username) : base(host, port, username)
    {
        OnMessageReceived += ProcessReceivedMessages;
    }

    public HostState State { get; private set; } = HostState.AcceptingConfigs;
    private void ProcessReceivedMessages(NetworkMessage message)
    {
        Console.WriteLine($"Received message from {message.ClientId}");
        Console.WriteLine($"Payload: {message.Payload}");
    }
}
public enum HostState
{
    AcceptingConfigs,
    Starting,
    RunningGame,
    GameEnded,
}