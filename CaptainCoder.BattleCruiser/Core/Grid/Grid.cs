using CaptainCoder.Core;
using System.Linq;
namespace CaptainCoder.BattleCruiser;
public class Grid
{
    private readonly Dictionary<Position, GridMark> _marks = new();
    public Grid(int rows, int columns) => (Rows, Columns) = (rows, columns);
    public int Rows { get; }
    public int Columns { get; }

    public GridMark this[Position position]
    {
        get 
        {
            if (!ValidatePosition(position)) { throw new IndexOutOfRangeException(); }
            return _marks.GetValueOrDefault(position, GridMark.Unknown);
        }
        set
        {
            if (!ValidatePosition(position)) { throw new IndexOutOfRangeException(); }
            _marks[position] = value;
        }
    }

    public void Clear(Position position) 
    {
        if (!ValidatePosition(position)) { throw new IndexOutOfRangeException(); }
        _marks.Remove(position);
    }

    private bool ValidatePosition(Position position) => !(position.Row < 0 || position.Row >= Rows || position.Col < 0 || position.Col >= Columns);

}

public enum GridMark
{
    Unknown,
    Miss,
    Hit
}