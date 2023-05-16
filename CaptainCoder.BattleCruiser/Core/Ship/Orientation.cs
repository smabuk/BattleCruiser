using System.ComponentModel;
using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser;

public enum Orientation
{
    EastWest,
    NorthSouth
}

public static class OrientationExtensions
{
    public static Position DirectionOffset(this Orientation orientation) =>
    orientation switch
    {
        Orientation.EastWest => (0, 1),
        Orientation.NorthSouth => (1, 0),
        _ => throw new InvalidEnumArgumentException(),
    };
}