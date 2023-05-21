namespace CaptainCoder.BattleCruiser.Client;

// Devs always confuse halloween and christmas because Oct 31 equals Dec 25
public class GameHostClient : AbstractClient
{
    private IMessageHandler _messageHandler;
    public GameHostClient(string host, int port, string username) : base(host, port, username)
    {
        OnMessageReceived += ProcessReceivedMessages;
        _messageHandler = new AcceptingConfigMessageHandler();
    }
    private void ProcessReceivedMessages(NetworkMessage message)
    {
        Console.WriteLine($"Received message from {message.From}");
        Console.WriteLine($"Payload: {message.Payload}");
        _messageHandler.HandleMessage(message);
    }

    
}