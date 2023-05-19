using CaptainCoder.Core;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace CaptainCoder.BattleCruiser;

public interface IInfoGrid
{
    public IReadOnlyDictionary<Position, IGridMark> Marks { get; }
    public int Rows { get; }
    public int Columns { get; }
}

/// <summary>
/// An <see cref="InfoGrid"/> represents a grid containing information about
/// positions that have been attacked.
/// </summary>
internal class InfoGrid : IInfoGrid
{
    private readonly Dictionary<Position, IGridMark> _marks = new();
    public IReadOnlyDictionary<Position, IGridMark> Marks => new ReadOnlyDictionary<Position, IGridMark>(_marks);
    public int Rows { get; } = GridConfigValidator.ExpectedRows;
    public int Columns { get; } = GridConfigValidator.ExpectedCols;

    public IGridMark this[Position position]
    {
        get 
        {
            if (!ValidatePosition(position)) { throw new IndexOutOfRangeException(); }
            return _marks[position];
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

[JsonDerivedType(typeof(MissMark), typeDiscriminator: "miss")]
[JsonDerivedType(typeof(HitMark), typeDiscriminator: "hit")]
public interface IGridMark
{
    public static readonly MissMark Miss = new ();
    public static HitMark Hit(ShipType type) => new (type);

}
public record MissMark : IGridMark;
public record HitMark(ShipType Type) : IGridMark;