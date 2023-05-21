using System.Diagnostics;
using CaptainCoder.Core;

namespace CaptainCoder.BattleCruiser.Client;

internal class GameRunningState : IHostState
{
    public const int SECONDS = 1000;
    private readonly AcceptingFireMessageHandler _messageHandler;
    private IEnumerable<INetworkPayload>? _roundResult;
    private readonly int _roundNumber;
    public GameRunningState(IReadOnlyDictionary<string, CaptainCoder.BattleCruiser.PlayerConfig> configs, INameManifest nickNames, int roundDuration = 10*SECONDS, int roundNumber = 0)
    {
        _messageHandler = new AcceptingFireMessageHandler(configs.ToPlayerGrids(), nickNames);
        Duration = roundDuration;
        _roundNumber = roundNumber;
    }
    private GameRunningState(GameRunningState lastRound)
    {
        _messageHandler = lastRound._messageHandler;
        _messageHandler.ClearTargets();
        Duration = lastRound.Duration;
        _roundNumber = lastRound._roundNumber + 1;
    }
    public IMessageHandler MessageHandler => _messageHandler;
    public int Duration { get; }
    public bool ShouldHalt => false;
    public IEnumerable<INetworkPayload> Messages => _roundResult ??= new []{new RoundResultMessage(_roundNumber, ApplyFireMessages(_messageHandler.PlayerGrids, _messageHandler.PlayerTargets))};

    public IHostState NextState()
    {
        IEnumerable<IPlayerGrid> livingPlayers = _messageHandler.PlayerGrids.Values.Where(grid => grid.IsAlive);
        if (livingPlayers.Count() > 1)
        {
            return new GameRunningState(this);
        }
        return new GameEndedState(_roundNumber, livingPlayers.Select(grid => grid.NickName).ToArray());
    }

    public static FireResult[] ApplyFireMessages(IReadOnlyDictionary<string, IPlayerGrid> PlayerGrids, IReadOnlyDictionary<string, FireMessage> PlayerTargets)
    {
        IEnumerable<IPlayerGrid> livingPlayers = PlayerGrids.Values.Where(grid => grid.IsAlive);
        IEnumerable<string> playersWhoFired = PlayerTargets.Keys;
        IEnumerable<string> playersWhoDidNotFire = livingPlayers.Select(grid => grid.NickName).Except(playersWhoFired);
        Dictionary<string, FireMessage> generatedFireMessages = GenerateRandomFireMessages(playersWhoDidNotFire, livingPlayers.Shuffle());
        Dictionary<FireMessage, List<string>> fireMessages = GenerateFireMessages(PlayerTargets.Union(generatedFireMessages));
        return ProcessFireMessages(fireMessages, PlayerGrids);
    }

    private static Dictionary<string, FireMessage> GenerateRandomFireMessages(IEnumerable<string> playersToFire, IEnumerable<IPlayerGrid> possibleTargets)
    {
        Dictionary<string, FireMessage> result = new ();
        IGenerator<IPlayerGrid> gridGenerator = new BagGenerator<IPlayerGrid>(possibleTargets);
        foreach (string player in playersToFire)
        {
            result[player] = GenerateRandomFireMessage(gridGenerator);
        }
        return result;
    }

    private static FireMessage GenerateRandomFireMessage(IGenerator<IPlayerGrid> gridGenerator)
    {
        IPlayerGrid playerGrid = gridGenerator.Next();
        Debug.Assert(playerGrid.IsAlive);
        return new FireMessage(playerGrid.NickName, playerGrid.Grid.Unknown.First());
    }

    private static Dictionary<FireMessage, List<string>> GenerateFireMessages(IEnumerable<KeyValuePair<string, FireMessage>> messages)
    {
        Dictionary<FireMessage, List<string>> result = new ();
        foreach ((string attacker, FireMessage message) in messages)
        {
            if(!result.TryGetValue(message, out List<string> attackers))
            {
                attackers = new();
                result[message] = attackers;
            }
            attackers.Add(attacker);
        }
        return result;
    }

    private static FireResult[] ProcessFireMessages(Dictionary<FireMessage, List<string>> messages, IReadOnlyDictionary<string, IPlayerGrid> grids)
    {
        // We apply each attack twice.
        // This allows us to determine on the second set of attacks if the hits
        // happen to be Sunk results rather than hit results.
        foreach ((FireMessage fireMessage, List<string> attackerIds) in messages)
        {
            grids[fireMessage.PlayerId].Attack(fireMessage.Target);
        }

        FireResult[] results = new FireResult[messages.Count];
        int ix = 0;
        foreach ((FireMessage fireMessage, List<string> attackerIds) in messages)
        {
            AttackResult result = grids[fireMessage.PlayerId].Attack(fireMessage.Target);
            results[ix] = new FireResult(fireMessage.PlayerId, fireMessage.Target, result, attackerIds.ToArray());
            ix++;
        }
        return results;
    }
}