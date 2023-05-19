using CaptainCoder.Core;
using System.Linq;
namespace CaptainCoder.BattleCruiser;

public interface IPlayerGrid
{
    public string NickName { get; }
    public bool IsAlive { get; }
    public IInfoGrid Grid { get; }
    public PlayerConfig Config { get; }
    public AttackResult Attack(Position position);
}

public record AttackResult(GridMark Mark);
public record SunkResult(ShipType Ship) : AttackResult (GridMark.Hit);

internal class PlayerGrid : IPlayerGrid
{
    private readonly InfoGrid _grid = new();
    private readonly ShipHitInfo[] _ships;
    public PlayerGrid(string nickName, PlayerConfig config)
    {
        NickName = nickName;
        Config = config;
        _ships = config.Ships.Select(ship => new ShipHitInfo(ship)).ToArray();
    }

    public string NickName { get; }
    public bool IsAlive => _ships.Any(s => s.IsAlive);
    public IInfoGrid Grid => _grid;
    public PlayerConfig Config { get; }

    public AttackResult Attack(Position position)
    {
        GridMark mark = _grid[position];
        if (mark != GridMark.Unknown) { return new AttackResult(mark); }
        foreach (ShipHitInfo ship in _ships)
        {
            if (ship.HasPosition(position))
            {
                return MarkShip(ship, position);
            }
        }
        _grid[position] = GridMark.Miss;
        return new AttackResult(GridMark.Miss);
    }

    private AttackResult MarkShip(ShipHitInfo ship, Position position)
    {
        ship.MarkHit(position);
        _grid[position] = GridMark.Hit;
        if (ship.IsAlive) { return new AttackResult(GridMark.Hit); }
        return new SunkResult(ship.Ship.ShipType);
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