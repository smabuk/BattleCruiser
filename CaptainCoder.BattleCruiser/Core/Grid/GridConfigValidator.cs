using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser;

public static class GridConfigValidator
{
    public static int ExpectedRows { get; set; } = 7;
    public static int ExpectedCols { get; set; } = 7;
    public static ValidationResult Validate(this GridConfig config)
    {
        if (config.Rows != ExpectedRows && config.Cols != ExpectedCols) { return new InvalidDimensions(ExpectedRows, ExpectedCols); }
        if (config.Ships.Length != 3) { return new MissingShipResult(); }
        HashSet<Position> occupied = new();
        HashSet<ShipType> types = new ();
        bool ValidPosition(Position p) =>
            p.Row >= 0 && p.Row < config.Rows && p.Col >= 0 && p.Col < config.Cols;
        foreach (Ship ship in config.Ships)
        {
            if (!types.Add(ship.ShipType)) { return new DuplicateShipResult(ship.ShipType); }
            foreach (Position position in ship.Positions())
            {
                if (!ValidPosition(position)) { return new PositionOutOfBoundsResult(position); }
                if (!occupied.Add(position)) { return new OverlappingResult(position); }
            }
        }
        return ValidationResult.Valid;
    }
}

public abstract record ValidationResult(bool IsValid, string Message)
{
    public static readonly ValidGrid Valid = new ();
}
public record InvalidDimensions(int rows, int cols) : 
    ValidationResult(false, $"Grid must have {rows} rows and {cols} columns.");
public record MissingShipResult() : 
    ValidationResult(false, $"Grid did not have exactly 3 ships.");
public record DuplicateShipResult(ShipType Type) : 
    ValidationResult(false, $"Grid should contain 1 boat of each type but found multiple {Type}.");
public record PositionOutOfBoundsResult(Position Position) : 
    ValidationResult(false, $"Position out of bounds {Position}.");
public record OverlappingResult(Position Position) :
    ValidationResult(false, $"Overlapping ships detected at {Position}");
public record ValidGrid() : ValidationResult(true, "Valid Grid");
