using CaptainCoder.Core;
using System.Linq;
namespace CaptainCoder.BattleCruiser;

public interface IInfoGrid
{
    GridMark this[Position position] { get; }
    public int Rows { get; }
    public int Columns { get; }
}

/// <summary>
/// An <see cref="InfoGrid"/> represents a grid containing information about
/// positions that have been attacked.
/// </summary>
internal class InfoGrid : IInfoGrid
{
    private readonly Dictionary<Position, GridMark> _marks = new();
    public int Rows { get; } = GridConfigValidator.ExpectedRows;
    public int Columns { get; } = GridConfigValidator.ExpectedCols;

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