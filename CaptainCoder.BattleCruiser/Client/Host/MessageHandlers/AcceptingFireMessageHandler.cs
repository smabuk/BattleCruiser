using System.Collections.ObjectModel;
using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser.Client;

/// <summary>
/// 
/// </summary>
public sealed class AcceptingFireMessageHandler : IMessageHandler
{
    private readonly static INetworkPayload[] s_OnlyAcceptingFireMessages = { new FireRejectedMessage("Only accepting FireMessages at this time.") };
    private readonly static INetworkPayload[] s_UserNameNotRecognized = { new FireRejectedMessage("Username not recognized as participant in this game.") };
    private readonly static INetworkPayload[] s_PlayerNotAlive = { new FireRejectedMessage("User board has been destroyed and may not make any moves.") };
    private readonly static INetworkPayload[] s_Accepted = { new FireAcceptedMessage() };
    private readonly Dictionary<string, FireMessage> _playerTargets = new();
    public AcceptingFireMessageHandler(IReadOnlyDictionary<string, IPlayerGrid> playerGrids, INameManifest nickNames)
    {
        PlayerGrids = playerGrids;
        NickNames = nickNames;
        PlayerTargets = new ReadOnlyDictionary<string, FireMessage>(_playerTargets);
    }
    public INameManifest NickNames { get; }
    public IReadOnlyDictionary<string, IPlayerGrid> PlayerGrids { get; }
    public IReadOnlyDictionary<string, FireMessage> PlayerTargets { get; }

    public IEnumerable<INetworkPayload> HandleMessage(NetworkMessage message)
    {
        if (message.Payload is not FireMessage) { return s_OnlyAcceptingFireMessages; }
        if (!NickNames.TryGetNickName(message.From, out string? nickName)) { return s_UserNameNotRecognized; }
        IPlayerGrid playerGrid = PlayerGrids[nickName];
        if (!playerGrid.IsAlive) { return s_PlayerNotAlive; }
        FireMessage fireMessage = (FireMessage)message.Payload;
        if (!fireMessage.Target.Validate()) { return RejectResponse($"{fireMessage.Target} is out of bounds."); }
        if (!PlayerGrids.TryGetValue(fireMessage.PlayerId, out IPlayerGrid targetGrid)) { return RejectResponse($"{fireMessage.PlayerId} is not a valid target."); }
        if (!targetGrid.IsAlive) { return RejectResponse($"{fireMessage.PlayerId} has already been destroyed."); }
        bool isMarked = targetGrid.Grid.Marks.ContainsKey(fireMessage.Target);
        if (isMarked) { return RejectResponse($"{fireMessage.PlayerId} @ {fireMessage.Target} has already been fired upon."); }
        _playerTargets[nickName] = fireMessage;
        return s_Accepted;
    }

    public void ClearTargets() => _playerTargets.Clear();

    private static IEnumerable<INetworkPayload> RejectResponse(string message) => new []{ new FireRejectedMessage(message) };
}