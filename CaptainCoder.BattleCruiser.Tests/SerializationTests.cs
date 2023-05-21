using FsCheck.Xunit;
using FsCheck;
using Shouldly;
using CaptainCoder.Core.Collections;

namespace CaptainCoder.BattleCruiser.Tests;

public class GridConfigTest
{
    [Fact]
    public void TestSerializeDeserializeGameStartingAt()
    {
        DateTime time = DateTime.UtcNow;
        GameStartingAt original = new (time);
        byte[] serialized = NetworkSerializer.Serialize(original);
        INetworkPayload deserialized = NetworkSerializer.Deserialize<INetworkPayload>(serialized);
        deserialized.ShouldNotBeNull();
        deserialized.ShouldBeOfType<GameStartingAt>();
        deserialized.ShouldBe(original);
    }
    [Fact]
    public void TestSerializeDeserializeGridConfigMessage()
    {
        Ship[] ships = new Ship[]
        {
            new Ship((0, 0), ShipType.Battleship, Orientation.Horizontal),
            new Ship((1, 0), ShipType.Destroyer, Orientation.Vertical),
            new Ship((2, 1), ShipType.Submarine, Orientation.Horizontal),
        };
        PlayerConfig config = new (ships);
        GridConfigMessage original = new (config);

        byte[] serialized = NetworkSerializer.Serialize(original);
        INetworkPayload deserialized = NetworkSerializer.Deserialize<INetworkPayload>(serialized);
        Assert.NotNull(deserialized);
        Assert.True(deserialized is GridConfigMessage);
        GridConfigMessage actual = (GridConfigMessage)deserialized;
        // Assert.Equal(original.Config.Ships, actual.Config.Ships);
        Assert.Equivalent(original, deserialized, strict: true);
    }

    [Fact]
    public void TestSerializeDeserializeGridConfigMessage2()
    {
        Ship[] ships = new Ship[]
        {
            new Ship((2, 2), ShipType.Destroyer, Orientation.Horizontal),
        };
        PlayerConfig config = new (ships);
        GridConfigMessage original = new (config);

        byte[] serialized = NetworkSerializer.Serialize(original);
        INetworkPayload deserialized = NetworkSerializer.Deserialize<INetworkPayload>(serialized);
        Assert.NotNull(deserialized);
        Assert.True(deserialized is GridConfigMessage);
        GridConfigMessage actual = (GridConfigMessage)deserialized;
        Assert.True(original.Config.Ships.SequenceEqual(actual.Config.Ships));
        Assert.Equivalent(original, deserialized, strict: true);
    } 

    [Property]
    public Property TestSerializeDeSerializeConfigAcceptedMessage() => 
        ConstructorBasedProperty<string>((id) => new ConfigAcceptedMessage(id));
    
    [Property]
    public Property TestSerializeDeSerializeInvalidConfigMessage() => 
        ConstructorBasedProperty<string>((id) => new InvalidConfigMessage(id));

    [Property]
    public Property TestSerializeDeSerializePlayerJoinedMessage() => 
        ConstructorBasedProperty<string>((id) => new PlayerJoinedMessage(id));

    [Property]
    public Property TestSerializeDeSerializePlayerLeftMessage() => 
        ConstructorBasedProperty<string>((id) => new PlayerLeftMessage(id));

    [Property]
    public Property TestSerializeDeSerializeGameStartingMessage()
    { 
        bool Checker(INetworkPayload original, INetworkPayload deserialized)
        {
            if (original is not GameStartingMessage expected) { return false; }
            if (deserialized is not GameStartingMessage actual) { return false; }
            return expected.PlayerIdentifiers.SequenceEqual(actual.PlayerIdentifiers);
        };
        return ConstructorBasedProperty<string[]>((string[] ids) => new GameStartingMessage(ids), Checker);
    }

    [Fact]
    public void TestSerializeDeserializeFireMessage()
    {
        // TODO: Explore Property generator for range of ints rather than any int
        FireMessage payload = new ("PlayerId", (0, 0));
        byte[] serialized = NetworkSerializer.Serialize(payload!);
        INetworkPayload deserialized = NetworkSerializer.Deserialize<INetworkPayload>(serialized);
        Assert.Equal(payload, deserialized);        

        payload = new ("PlayerId2", (5, 7));
        serialized = NetworkSerializer.Serialize(payload!);
        deserialized = NetworkSerializer.Deserialize<INetworkPayload>(serialized);
        Assert.Equal(payload, deserialized);        
    }

    [Fact]
    public void TestFireAcceptedMessage()
    {   
        FireAcceptedMessage payload = new ();
        byte[] serialized = NetworkSerializer.Serialize(payload!);
        INetworkPayload deserialized = NetworkSerializer.Deserialize<INetworkPayload>(serialized);
        Assert.Equal(payload, deserialized);     
    }

    [Property]
    public Property TestSerializeDeSerializeFireRejectedMessage() => 
        ConstructorBasedProperty<string>((id) => new FireRejectedMessage(id));

    [Fact]
    public void TestSerializeDeserializeRoundResultMessage()
    {
        FireResult[] results = 
        {
            new FireResult("Player1", (0, 0), new AttackResult(IGridMark.Hit(ShipType.Battleship)), new []{"Player2"}),
            new FireResult("Player2", (5, 5), new AttackResult(IGridMark.Miss), new[]{"Player1"}),
            new FireResult("Player2", (3, 5), new SunkResult(ShipType.Destroyer), new []{"Player4", "Player3"}),
        };
        // TODO: Explore Property generator for range of ints rather than any int
        RoundResultMessage payload = new (1, results);
        byte[] serialized = NetworkSerializer.Serialize(payload!);
        INetworkPayload deserialized = NetworkSerializer.Deserialize<INetworkPayload>(serialized);
        Assert.True(deserialized is RoundResultMessage);
        RoundResultMessage actual = (RoundResultMessage)deserialized;
        Assert.Equal(payload.RoundNumber, actual.RoundNumber);
        // Assert.True(payload.Results.SequenceEqual(actual.Results));
        Assert.Equivalent(payload, actual);
    }

    // private static bool CheckFireResultEquality(FireResult[] arr0, FireResult[] arr1)

    [Fact]
    public void TestSerializeDeserializeRoundResultMessage2()
    {
        FireResult[] results = 
        {
            new FireResult("Stephen", (5, 5), new AttackResult(IGridMark.Miss), new []{"Player2"}),
            new FireResult("Player1", (0, 0), new AttackResult(IGridMark.Miss), new []{"Player2", "Stephen"}),            
            new FireResult("Player2", (3, 5), new AttackResult(IGridMark.Hit(ShipType.Submarine)), new []{"Player1"}),
        };
        // TODO: Explore Property generator for range of ints rather than any int
        RoundResultMessage payload = new (1, results);
        byte[] serialized = NetworkSerializer.Serialize(payload!);
        INetworkPayload deserialized = NetworkSerializer.Deserialize<INetworkPayload>(serialized);
        Assert.True(deserialized is RoundResultMessage);
        RoundResultMessage actual = (RoundResultMessage)deserialized;
        Assert.Equal(payload.RoundNumber, actual.RoundNumber);
        Assert.Equivalent(payload, actual);
    }

    [Fact]
    public void TestSerializeDeSerializeGameResultMessage()
    { 
        string[] results = 
        {
            "Bob",
            "Sue"
        };
        // TODO: Explore Property generator for range of ints rather than any int
        GameResultMessage payload = new (1, results);
        byte[] serialized = NetworkSerializer.Serialize(payload!);
        INetworkPayload deserialized = NetworkSerializer.Deserialize<INetworkPayload>(serialized);
        Assert.True(deserialized is GameResultMessage);
        GameResultMessage actual = (GameResultMessage)deserialized;
        Assert.Equal(payload.TotalRounds, actual.TotalRounds);
        Assert.True(payload.WinnerIds.SequenceEqual(actual.WinnerIds));
    }

    [Fact]
    public void TestSerializeDeSerializeGameResultMessag2()
    { 
        string[] results = 
        {
            "Bobby"
        };
        // TODO: Explore Property generator for range of ints rather than any int
        GameResultMessage payload = new (17, results);
        byte[] serialized = NetworkSerializer.Serialize(payload!);
        INetworkPayload deserialized = NetworkSerializer.Deserialize<INetworkPayload>(serialized);
        Assert.True(deserialized is GameResultMessage);
        GameResultMessage actual = (GameResultMessage)deserialized;
        Assert.Equal(payload.TotalRounds, actual.TotalRounds);
        Assert.True(payload.WinnerIds.SequenceEqual(actual.WinnerIds));
    }

    private Property ConstructorBasedProperty<T>(Func<T, INetworkPayload> constructor)
        => ConstructorBasedProperty<T>(constructor, (a, b) => a != null && a.Equals(b));

    private Property ConstructorBasedProperty<T>(Func<T, INetworkPayload> constructor, Func<INetworkPayload, INetworkPayload, bool> check)
    {
        Func<T, bool> canSerializeDeserialize = (identifier) =>
        {
            INetworkPayload payload = constructor(identifier);
            byte[] serialized = NetworkSerializer.Serialize(payload!);
            INetworkPayload deserialized = NetworkSerializer.Deserialize<INetworkPayload>(serialized);
            return check(payload, deserialized);
        };
        return Prop.ForAll(canSerializeDeserialize);
    }

}

delegate bool CheckSerialize<T>(T payload, INetworkPayload deserialized);