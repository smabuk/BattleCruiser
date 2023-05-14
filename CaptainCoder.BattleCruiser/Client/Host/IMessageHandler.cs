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
            GridConfigMessage(GridConfig config) => () => HandleGridConfig(config, message.ClientId),
            _ => () => ReplyWithInvalidConfig(message.ClientId),
        };
        action.Invoke();
    }

    private void HandleGridConfig(GridConfig config, string clientId)
    {
        // TODO: Check that config is valid
        INetworkPayload configAcceptedPayload = new ConfigAcceptedMessage();
        _host.EnqueueMessage(configAcceptedPayload, $"private/{clientId}");
        // TODO: Generate nickname for client and send that rather than clientId
        INetworkPayload playerJoinedPayload = new PlayerJoinedMessage(clientId);
        _host.EnqueueMessage(playerJoinedPayload, $"public/hostId");
    }

    private void ReplyWithInvalidConfig(string clientId)
    {
        INetworkPayload payload = new InvalidConfigMessage("Only accepting Config messages at this time.");
        _host.EnqueueMessage(payload, $"private/{clientId}");
    }
}