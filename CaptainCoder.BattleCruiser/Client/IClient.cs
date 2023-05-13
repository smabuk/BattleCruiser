namespace CaptainCoder.BattleCruiser.Client;

public interface IClient
{
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action OnConnecting;
    public event Action<NetworkMessage> OnMessageReceived;
    public Task Connect();
    public void RequestDisconnect();
    public void EnqueueMessage(INetworkPayload toSend, string topic);
}
