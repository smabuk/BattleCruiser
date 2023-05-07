namespace CaptainCoder.BattleCruiser;
using CaptainCoder.Core;
using System.Text.Json;

public record GridConfig(int Rows, int Cols, ShipConfig[] Ships)
{
    public string ToJson()
    {
        // TODO: Investigate encoding schema
        return JsonSerializer.Serialize(this);
    }

    public static GridConfig? FromJson(string json) // "null"
    {
        return JsonSerializer.Deserialize<GridConfig>(json);
    }
}

public record struct ShipConfig(Position Position, ShipType ShipType, Orientation Orientation);

public enum Orientation
{
    EastWest,
    NorthSouth
}