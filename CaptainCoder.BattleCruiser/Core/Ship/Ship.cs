using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser;
public record struct Ship(Position Position, ShipType ShipType, Orientation Orientation)
{
    public int Size => ShipType.Size();
    public IEnumerable<Position> Positions()
    {
        int size = Size;
        Position[] positions = new Position[size];
        Position increment = Orientation.DirectionOffset();
        for (int offset = 0; offset < size; offset++)
        {
            yield return Position + (increment * offset);
        }        
    }
}