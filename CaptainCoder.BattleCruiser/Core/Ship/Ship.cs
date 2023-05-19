using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser;
public record struct Ship(Position Position, ShipType ShipType, Orientation Orientation);
public static class ShipExtensions
{
    public static int Size(this Ship ship) => ship.ShipType.Size();
    public static IReadOnlyCollection<Position> Positions(this Ship ship)
    {
        int size = ship.ShipType.Size();
        Position[] positions = new Position[size];
        Position increment = ship.Orientation.DirectionOffset();
        for (int offset = 0; offset < size; offset++)
        {
            positions[offset] = ship.Position + (increment * offset);
        }
        return positions;
    }

    public static bool HasPosition(this Ship ship, Position position) => ship.Positions().Contains(position);
}