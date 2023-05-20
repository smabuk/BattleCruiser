using CaptainCoder.BattleCruiser.Client;
using System.Collections.ObjectModel;
using CaptainCoder.Core;
using System.Collections.Generic;
using System.Linq;
namespace CaptainCoder.BattleCruiser;

public interface IPlayerGrid
{
    public string NickName { get; }
    public bool IsAlive { get; }
    public IInfoGrid Grid { get; }
    public AttackResult Attack(Position position);
}

public record AttackResult(IGridMark Mark);
public record SunkResult(ShipType Ship) : AttackResult (IGridMark.Hit(Ship));

public static class IPlayerGridExtensions
{
    public static IReadOnlyDictionary<string, IPlayerGrid> ToPlayerGrids(this IReadOnlyDictionary<string, PlayerConfig> configs)
    {
        Dictionary<string, IPlayerGrid> grids = new();
        foreach ((string nickname, PlayerConfig config) in configs)
        {
            grids[nickname] = new PlayerGrid(nickname, config);
        }
        return new ReadOnlyDictionary<string, IPlayerGrid>(grids);
    }
}

internal class PlayerGrid : IPlayerGrid
{
    private readonly InfoGrid _grid = new();
    private readonly ShipHitInfo[] _ships;
    public PlayerGrid(string nickName, PlayerConfig config)
    {
        NickName = nickName;
        _ships = config.Ships.Select(ship => new ShipHitInfo(ship)).ToArray();
    }

    public string NickName { get; }
    public bool IsAlive => _ships.Any(s => s.IsAlive);
    public IInfoGrid Grid => _grid;

    public AttackResult Attack(Position position)
    {
        if (_grid.Marks.TryGetValue(position, out IGridMark mark)) { return new AttackResult(mark); }
        foreach (ShipHitInfo ship in _ships)
        {
            if (ship.HasPosition(position))
            {
                return MarkShip(ship, position);
            }
        }
        _grid[position] = IGridMark.Miss;
        return new AttackResult(IGridMark.Miss);
    }

    private AttackResult MarkShip(ShipHitInfo shipInfo, Position position)
    {
        shipInfo.MarkHit(position);
        _grid[position] = IGridMark.Hit(shipInfo.Ship.ShipType);
        if (shipInfo.IsAlive) { return new AttackResult(IGridMark.Hit(shipInfo.Ship.ShipType)); }
        return new SunkResult(shipInfo.Ship.ShipType);
    }
}

internal class ShipHitInfo
{
    private readonly HashSet<Position> _hits = new();
    public ShipHitInfo(Ship ship) => Ship = ship;
    public Ship Ship { get; }
    public bool IsAlive => _hits.Count < Ship.Size();
    public void MarkHit(Position position)
    {
        if (!Ship.HasPosition(position)) { throw new ArgumentOutOfRangeException(); }
        _hits.Add(position);
    }
    public bool HasPosition(Position position) => Ship.HasPosition(position);
}