using CaptainCoder.Core.Collections;

namespace CaptainCoder.BattleCruiser.Client;

/// <summary>
/// 
/// </summary>
public class AcceptingConfigMessageHandler : IMessageHandler
{
    private static INetworkPayload[] s_NotAcceptingConfigs = { new InvalidConfigMessage("Only accepting Config messages at this time.") };
    public NameManifest Manifest { get; } = new();
    public IEnumerable<INetworkPayload> HandleMessage(NetworkMessage message)
    {
        if (message.Payload is not GridConfigMessage)
        {
            return s_NotAcceptingConfigs;
        }

        GridConfig config = ((GridConfigMessage)message.Payload).Config;
        ValidationResult validation = config.Validate();
        if (!validation.IsValid)
        {
            return new[]{new InvalidConfigMessage(validation.Message)};
        }

        INetworkPayload[]? responses = null;
        if (Manifest.GetNickName(message.From, out string nickname))
        {
            responses = new INetworkPayload[2];
            // If this is the first time we have seen this username, broadcast playerjoined
            responses[1] = new PlayerJoinedMessage(nickname);
        }
        responses ??= new INetworkPayload[1];
        responses[0] = new ConfigAcceptedMessage(nickname);
        return responses;
    }
}