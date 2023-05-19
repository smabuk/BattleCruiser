using System.Runtime.CompilerServices;

public enum ShipType
{
    /// <summary>
    /// A Ship of length 4
    /// </summary>
    Battleship,
    /// <summary>
    /// A ship of length 3
    /// </summary>
    Submarine,
    /// <summary>
    /// A ship of length 2
    /// </summary>
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