namespace CaptainCoder.BattleCruiser.Client;
public interface IMessageHandler
{
    public IEnumerable<INetworkPayload> HandleMessage(NetworkMessage message);
}