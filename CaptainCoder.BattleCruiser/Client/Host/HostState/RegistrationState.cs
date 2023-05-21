namespace CaptainCoder.BattleCruiser.Client;

/// <summary>
/// Player's may register their grids during this state.
/// </summary>
internal class RegistrationState : IHostState
{
    private readonly AcceptingConfigMessageHandler _messageHandler;
    private readonly int _roundDuration;
    public RegistrationState(int duration, int roundDuration)
    {
        Duration = duration;   
        _messageHandler = new AcceptingConfigMessageHandler();
        _roundDuration = roundDuration;
    }
    public IMessageHandler MessageHandler => _messageHandler;
    public int Duration { get; }
    public IEnumerable<INetworkPayload> Messages => new []{new GameStartingMessage(_messageHandler.NickNames.NickNames.ToArray())};
    public IHostState NextState() => new GameRunningState(_messageHandler.Configs, _messageHandler.NickNames, _roundDuration);
    public bool ShouldHalt => false;
}