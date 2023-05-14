namespace CaptainCoder.BattleCruiser.Client;
public interface IGameState
{
    public void HandleMessage(NetworkMessage message);
    public IGameState ProcessState();
}