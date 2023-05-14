namespace CaptainCoder.BattleCruiser.Client;
public interface IMessageHandler
{
    public void Handle(NetworkMessage message);
}

public class AcceptingConfigMessageHandler : IMessageHandler
{
    private readonly GameHostClient _host;
    public AcceptingConfigMessageHandler(GameHostClient host) => _host = host;
    public void Handle(NetworkMessage message)
    {
        INetworkPayload payload = message.Payload;
        Action action = payload switch
        {
            GridConfigMessage(GridConfig config) => () => HandleGridConfig(config, message.From),
            _ => () => ReplyWithInvalidConfig(message.From),
        };
        action.Invoke();
    }

    private void HandleGridConfig(GridConfig config, string username)
    {
        // TODO: Check that config is valid
        INetworkPayload configAcceptedPayload = new ConfigAcceptedMessage();
        _host.EnqueueMessage(configAcceptedPayload, $"private/{username}");
        // TODO: Generate nickname for client and send that rather than clientId
        INetworkPayload playerJoinedPayload = new PlayerJoinedMessage(username);
        _host.EnqueueMessage(playerJoinedPayload, $"public/{_host.UserName}");
    }

    private void ReplyWithInvalidConfig(string clientId)
    {
        INetworkPayload payload = new InvalidConfigMessage("Only accepting Config messages at this time.");
        _host.EnqueueMessage(payload, $"private/{clientId}");
    }
}