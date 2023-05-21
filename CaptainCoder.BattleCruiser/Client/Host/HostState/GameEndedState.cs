using System.Diagnostics;
using CaptainCoder.Core;

namespace CaptainCoder.BattleCruiser.Client; 

internal class GameEndedState : IHostState
{
    private readonly int _totalRounds;
    private readonly string[] _winnerIds;
    public GameEndedState(int totalRounds, string[] winnerIds) => (_totalRounds, _winnerIds) = (totalRounds, winnerIds);
    public IMessageHandler MessageHandler => throw new NotImplementedException();
    public int Duration => 1;
    public IEnumerable<INetworkPayload> Messages => new []{ new GameResultMessage(_totalRounds, _winnerIds) };
    public bool ShouldHalt => true;

    public IHostState NextState() => this;
}