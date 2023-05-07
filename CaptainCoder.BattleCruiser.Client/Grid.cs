namespace CaptainCoder.BattleCruiser;
using CaptainCoder.Core;
using System.Text.Json;

public record GridConfig(int Rows, int Cols, ShipConfig[] Ships)
{
    // TODO: Investigate encoding schema
    public string ToJson() => JsonSerializer.Serialize(this);
    public static GridConfig? FromJson(string json) => JsonSerializer.Deserialize<GridConfig>(json);
}

public record struct ShipConfig(Position Position, ShipType ShipType, Orientation Orientation);

public enum Orientation
{
    EastWest,
    NorthSouth
}