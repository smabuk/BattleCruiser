using System.Runtime.CompilerServices;

public enum ShipType
{
    Battleship,
    Submarine,
    Destroyer,
}

public static class ShipExtensions
{
    public static int Size(this ShipType ship)
    {
        return ship switch
        {
            ShipType.Battleship => 4,
            ShipType.Submarine => 3,
            ShipType.Destroyer => 2,
            _ => throw new SwitchExpressionException($"Invalid ShipType {ship}"),
        };
    }
}