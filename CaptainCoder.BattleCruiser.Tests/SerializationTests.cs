namespace CaptainCoder.BattleCruiser.Tests;

public class GridConfigTest
{
    [Fact]
    public void TestSerializeDeserialize()
    {
        ShipConfig[] ships = new ShipConfig[]
        {
            new ShipConfig((0, 0), ShipType.Battleship, Orientation.EastWest),
            new ShipConfig((1, 0), ShipType.Destroyer, Orientation.NorthSouth),
            new ShipConfig((2, 1), ShipType.Submarine, Orientation.EastWest),
        };
        GridConfig config = new (7, 7, ships);

        string json = config.ToJson();
        GridConfig deserialized = GridConfig.FromJson(json)!;
        Assert.NotNull(deserialized);
        Assert.Equal(config.Rows, deserialized.Rows);
        Assert.Equal(config.Cols, deserialized.Cols);
        Assert.Collection(deserialized.Ships, 
            (firstElem) => Assert.Equal(ships[0], firstElem),
            (secondElem) => Assert.Equal(ships[1], secondElem),
            (thirdElem) => Assert.Equal(ships[2], thirdElem)
        );
    }

    [Fact]
    public void TestSerializeDeserialize2()
    {
        ShipConfig[] ships = new ShipConfig[]
        {
            new ShipConfig((2, 2), ShipType.Destroyer, Orientation.EastWest),
        };
        GridConfig config = new (4, 4, ships);

        string json = config.ToJson();
        GridConfig deserialized = GridConfig.FromJson(json)!;
        Assert.NotNull(deserialized);
        Assert.Equal(config.Rows, deserialized.Rows);
        Assert.Equal(config.Cols, deserialized.Cols);
        Assert.Collection(deserialized.Ships, 
            (firstElem) => Assert.Equal(ships[0], firstElem)
        );
    }
}