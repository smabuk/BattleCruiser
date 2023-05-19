using CaptainCoder.Core;
using CaptainCoder.Core.Collections;
namespace CaptainCoder.BattleCruiser;

public class PlayerGrids
{
    private readonly Dictionary<string, InfoGrid> _playerGrids = new();
    public PlayerGrids(HashSet<string> boardIdentifiers)
    {
        Identifiers = new ReadOnlySet<string>(boardIdentifiers);
        foreach (string identifier in boardIdentifiers)
        {
            _playerGrids[identifier] = new InfoGrid(7, 7);
        }
    }

    public ReadOnlySet<string> Identifiers { get; }
    public InfoGrid GetGrid(string identifier)
    {
        if(!_playerGrids.TryGetValue(identifier, out InfoGrid grid))
        {
            throw new KeyNotFoundException($"Invalid identifer {identifier}.");
        }
        return grid;
    }

    public void ApplyResults(IEnumerable<FireResult> results) => results.ForEach(ApplyResult);
    public void ApplyResult(FireResult result) => _playerGrids[result.PlayerId][result.Position] = result.Mark;
}

