using System.Collections.ObjectModel;
using CaptainCoder.Core.Collections;

namespace CaptainCoder.BattleCruiser.Client;

/// <summary>
/// 
/// </summary>
public sealed class AcceptingConfigMessageHandler : IMessageHandler
{
    private static INetworkPayload[] s_NotAcceptingConfigs = { new InvalidConfigMessage("Only accepting Config messages at this time.") };
    private readonly Dictionary<string, PlayerConfig> _configs = new();
    private readonly NameManifest _nickNames = new();
    public AcceptingConfigMessageHandler()
    {
        Configs = new ReadOnlyDictionary<string, PlayerConfig>(_configs);
    }

    public INameManifest NickNames => _nickNames;
    public IReadOnlyDictionary<string, PlayerConfig> Configs { get; private set; }
    public IEnumerable<INetworkPayload> HandleMessage(NetworkMessage message)
    {
        if (message.Payload is not GridConfigMessage)
        {
            return s_NotAcceptingConfigs;
        }

        PlayerConfig config = ((GridConfigMessage)message.Payload).Config;
        ValidationResult validation = config.Validate();
        if (!validation.IsValid)
        {
            return new[]{new InvalidConfigMessage(validation.Message)};
        }

        INetworkPayload[]? responses = null;
        if (_nickNames.GetNickName(message.From, out string nickname))
        {
            responses = new INetworkPayload[2];
            // If this is the first time we have seen this username, broadcast playerjoined
            responses[1] = new PlayerJoinedMessage(nickname);
        }
        responses ??= new INetworkPayload[1];
        responses[0] = new ConfigAcceptedMessage(nickname);
        _configs[nickname] = config;
        return responses;
    }
}