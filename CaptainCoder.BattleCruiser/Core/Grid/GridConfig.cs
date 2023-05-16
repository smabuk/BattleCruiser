namespace CaptainCoder.BattleCruiser;
using System.Text.Json;

public record GridConfig(int Rows, int Cols, Ship[] Ships);