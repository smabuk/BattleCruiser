using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser;

public static class GridConfigValidator
{
    public static int ExpectedRows { get; set; } = 7;
    public static int ExpectedCols { get; set; } = 7;
    public static ValidationResult Validate(this PlayerConfig config)
    {
        if (config.Ships.Count() != 3) { return new MissingShipResult(); }
        HashSet<Position> occupied = new();
        HashSet<ShipType> types = new ();
        foreach (Ship ship in config.Ships)
        {
            if (!types.Add(ship.ShipType)) { return new DuplicateShipResult(ship.ShipType); }
            foreach (Position position in ship.Positions())
            {
                if (!position.Validate()) { return new PositionOutOfBoundsResult(position); }
                if (!occupied.Add(position)) { return new OverlappingResult(position); }
            }
        }
        return ValidationResult.Valid;
    }

    public static bool Validate(this Position toValidate) =>
        toValidate.Row >= 0 && toValidate.Row < ExpectedRows && toValidate.Col >= 0 && toValidate.Col < ExpectedCols;
    
}

public abstract record ValidationResult(bool IsValid, string Message)
{
    public static readonly ValidGrid Valid = new ();
}
public record MissingShipResult() : 
    ValidationResult(false, $"Grid did not have exactly 3 ships.");
public record DuplicateShipResult(ShipType Type) : 
    ValidationResult(false, $"Grid should contain 1 boat of each type but found multiple {Type}.");
public record PositionOutOfBoundsResult(Position Position) : 
    ValidationResult(false, $"Position out of bounds {Position}.");
public record OverlappingResult(Position Position) :
    ValidationResult(false, $"Overlapping ships detected at {Position}");
public record ValidGrid() : ValidationResult(true, "Valid Grid");
