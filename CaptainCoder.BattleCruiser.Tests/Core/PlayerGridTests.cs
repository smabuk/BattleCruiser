using CaptainCoder.Core;
using CaptainCoder.Core.Collections;

namespace CaptainCoder.BattleCruiser.Client.Tests;
public class PlayerGridTests
{
    [Fact]
    public void TestAttack()
    {
        /*
            B.DD...
            B......
            B......
            B......
            ......S
            ......S
            ......S
        */
        Ship[] ships = { 
            new Ship((0, 0), ShipType.Battleship, Orientation.Vertical),
            new Ship((0, 2), ShipType.Destroyer, Orientation.Horizontal),
            new Ship((4, 6), ShipType.Submarine, Orientation.Vertical),
        };
        PlayerConfig Config = new (ships);
        IPlayerGrid grid = new PlayerGrid("Bob", Config);
        Assert.True(grid.IsAlive);
        Assert.Equal("Bob", grid.NickName);

        #region  Attack and Sink BattleShip
        AttackResult result = grid.Attack((0, 0));
        AttackResult expected = new (IGridMark.Hit(ShipType.Battleship));
        Assert.Equal(expected, result);
        Assert.True(grid.IsAlive);

        result = grid.Attack((1, 0));
        expected = new (IGridMark.Hit(ShipType.Battleship));
        Assert.Equal(expected, result);
        Assert.True(grid.IsAlive);

        result = grid.Attack((2, 0));
        expected = new (IGridMark.Hit(ShipType.Battleship));
        Assert.Equal(expected, result);
        Assert.True(grid.IsAlive);

        result = grid.Attack((3, 0));
        expected = new SunkResult(ShipType.Battleship);
        Assert.Equal(expected, result);
        Assert.True(grid.IsAlive);
        #endregion

        #region Misses
        result = grid.Attack((0, 1));
        expected = new AttackResult(IGridMark.Miss);
        Assert.Equal(expected, result);
        Assert.True(grid.IsAlive);

        result = grid.Attack((1, 1));
        expected = new AttackResult(IGridMark.Miss);
        Assert.Equal(expected, result);
        Assert.True(grid.IsAlive);
        #endregion

        #region Validate IGridInfo
        Dictionary<Position, IGridMark> expectedMarks = new ()
        {
            {(0, 0), IGridMark.Hit(ShipType.Battleship)},
            {(1, 0), IGridMark.Hit(ShipType.Battleship)},
            {(2, 0), IGridMark.Hit(ShipType.Battleship)},
            {(3, 0), IGridMark.Hit(ShipType.Battleship)},
            {(0, 1), IGridMark.Miss},
            {(1, 1), IGridMark.Miss},
        };
        var actualMarks = grid.Grid.Marks;
        Assert.Equal(expectedMarks.Count, actualMarks.Count);
        Assert.True(expectedMarks.KeyValuePairEquals(actualMarks));
        #endregion

        #region Attack and Sink Destroyer
        result = grid.Attack((0, 2));
        expected = new (IGridMark.Hit(ShipType.Destroyer));
        Assert.Equal(expected, result);
        Assert.True(grid.IsAlive);

        result = grid.Attack((0, 3));
        expected = new SunkResult(ShipType.Destroyer);
        Assert.Equal(expected, result);
        Assert.True(grid.IsAlive);
        #endregion
    
        #region Attack and Sink Submarine
        result = grid.Attack((4, 6));
        expected = new (IGridMark.Hit(ShipType.Submarine));
        Assert.Equal(expected, result);
        Assert.True(grid.IsAlive);

        result = grid.Attack((5, 6));
        expected = new (IGridMark.Hit(ShipType.Submarine));
        Assert.Equal(expected, result);
        Assert.True(grid.IsAlive);

        result = grid.Attack((6, 6));
        expected = new SunkResult(ShipType.Submarine);
        Assert.Equal(expected, result);
        Assert.False(grid.IsAlive);
        #endregion
    }
}