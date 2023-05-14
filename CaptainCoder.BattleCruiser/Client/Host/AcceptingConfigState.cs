namespace CaptainCoder.BattleCruiser.Client;

public class AcceptingConfigMessageHandler : IGameState
{
    private readonly GameHostClient _host;
    private readonly Dictionary<string, GridConfig> _grids = new();
    private readonly Dictionary<string, string> _identifierLookup = new();
    public AcceptingConfigMessageHandler(GameHostClient host) => _host = host;
    public void HandleMessage(NetworkMessage message)
    {
        INetworkPayload payload = message.Payload;
        Action action = payload switch
        {
            GridConfigMessage(GridConfig config) => () => HandleGridConfig(config, message.From),
            _ => () => ReplyWithInvalidConfig(message.From),
        };
        action.Invoke();
    }

    public IGameState ProcessState()
    {
        // TODO: Generate board, notify players, then start game
        // _host.EnqueueMessage(new GameStartingMessage(_identifierLookup.Keys.ToArray()), 
        _host.BroadcastMessage(new GameStartingMessage(_identifierLookup.Keys.ToArray()));
        return new RunningGameState(_host);
    }

    private void HandleGridConfig(GridConfig config, string username)
    {
        string identifier = UserIdentifier(username);
        // TODO: Check that config is valid
        INetworkPayload configAcceptedPayload = new ConfigAcceptedMessage(identifier);
        _host.PrivateMessage(configAcceptedPayload, username);

        // TODO: Generate nickname for client and send that rather than clientId
        if (!_grids.ContainsKey(username))
        {
            INetworkPayload playerJoinedPayload = new PlayerJoinedMessage(identifier);
            _host.BroadcastMessage(playerJoinedPayload);
        }
        _grids[username] = config;
    }

    private void ReplyWithInvalidConfig(string clientId)
    {
        INetworkPayload payload = new InvalidConfigMessage("Only accepting Config messages at this time.");
        _host.EnqueueMessage(payload, $"private/{clientId}");
    }

    private string UserIdentifier(string username)
    {
        if (_identifierLookup.TryGetValue(username, out string? identifier))
        {
            return identifier;
        }
        // TODO: Implement name generator
        identifier = $"Player_{_identifierLookup.Count}";
        _identifierLookup[username] = identifier;
        return identifier;
    }
}