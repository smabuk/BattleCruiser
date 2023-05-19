namespace CaptainCoder.BattleCruiser.Client;

public class GameRunningMessageHandler : IMessageHandler
{
    private readonly Dictionary<string, PlayerConfig> _grids = new();
    // public GameRunningMessageHandler(NameManifest manifest, Dictionary<string, GridConfig>) => _host = host;

    public IEnumerable<INetworkPayload> HandleMessage(NetworkMessage message)
    {
       throw new NotImplementedException();
    }

    public IMessageHandler ProcessState()
    {
        // TODO: Execute all moves 
        // Broadcast result to players

        // TODO: Check if game is finished
        // if so, return GameResultState

        // Otherwise we continue as RunningGameState
        return this;
    }
}