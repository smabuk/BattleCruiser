using CaptainCoder.Core;
using CaptainCoder.Core.Collections;
namespace CaptainCoder.BattleCruiser;

public class PlayerGrids
{
    private readonly Dictionary<string, Grid> _playerGrids = new();
    public PlayerGrids(HashSet<string> boardIdentifiers)
    {
        Identifiers = new ReadOnlySet<string>(boardIdentifiers);
        foreach (string identifier in boardIdentifiers)
        {
            _playerGrids[identifier] = new Grid(7, 7);
        }
    }

    public ReadOnlySet<string> Identifiers { get; }
    public Grid GetGrid(string identifier)
    {
        if(!_playerGrids.TryGetValue(identifier, out Grid grid))
        {
            throw new KeyNotFoundException($"Invalid identifer {identifier}.");
        }
        return grid;
    }

    public void ApplyResults(IEnumerable<FireResult> results) => results.ForEach(ApplyResult);
    public void ApplyResult(FireResult result) => _playerGrids[result.PlayerId].Mark(result.Position, result.Mark);
}

