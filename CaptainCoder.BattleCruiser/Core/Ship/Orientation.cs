using System.ComponentModel;
using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser;

public enum Orientation
{
    Horizontal,
    Vertical
}

public static class OrientationExtensions
{
    public static Position DirectionOffset(this Orientation orientation) =>
    orientation switch
    {
        Orientation.Horizontal => (0, 1),
        Orientation.Vertical => (1, 0),
        _ => throw new InvalidEnumArgumentException(),
    };
}