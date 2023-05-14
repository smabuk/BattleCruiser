namespace CaptainCoder.BattleCruiser.Client;

public class RunningGameState : IGameState
{
    private readonly GameHostClient _host;
    private readonly Dictionary<string, GridConfig> _grids = new();
    public RunningGameState(GameHostClient host) => _host = host;

    public void HandleMessage(NetworkMessage message)
    {
       throw new NotImplementedException();
    }

    public IGameState ProcessState()
    {
        // TODO: Execute all moves 
        // Broadcast result to players

        // TODO: Check if game is finished
        // if so, return GameResultState

        // Otherwise we continue as RunningGameState
        return this;
    }
}