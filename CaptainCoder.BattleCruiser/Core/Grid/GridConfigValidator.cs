using System.ComponentModel;
using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser;

public class GridConfigValidator
{
    public bool Validate(GridConfig config)
    {
        HashSet<Position> occupied = new();
        bool ValidPosition(Position p) =>
            p.Row >= 0 && p.Row < config.Rows && p.Col >= 0 && p.Col < config.Cols;
        foreach (Ship ship in config.Ships)
        {
            foreach (Position p in ship.Positions())
            {
                if (!ValidPosition(p)) { return false; }
                if (!occupied.Add(p)) { return false; }
            }
        }        
        return true;
    }
}