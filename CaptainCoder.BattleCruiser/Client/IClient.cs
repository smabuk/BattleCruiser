namespace CaptainCoder.BattleCruiser.Client;

public interface IClient
{
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action OnConnecting;
    public event Action<INetworkMessage> OnMessageReceived;
    public Task Connect();
    public void RequestDisconnect();
    public void EnqueueMessage(INetworkMessage toSend, string topic);
}
