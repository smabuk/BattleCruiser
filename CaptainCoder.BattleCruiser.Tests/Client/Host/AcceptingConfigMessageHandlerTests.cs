using Moq;
using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser.Client.Tests;

public class AcceptingConfigMessageHandlerTests
{
    static GridConfigMessage VALID_GRID;
    static GridConfigMessage INVALID_GRID;
    static AcceptingConfigMessageHandlerTests()
    {
        Ship[] ships = new Ship[]
        {
            new Ship((0, 0), ShipType.Battleship, Orientation.EastWest),
            new Ship((1, 0), ShipType.Destroyer, Orientation.NorthSouth),
            new Ship((2, 1), ShipType.Submarine, Orientation.EastWest),
        };
        GridConfig config = new(7, 7, ships);
        VALID_GRID = new GridConfigMessage(config);

        ships = Array.Empty<Ship>();
        config = new(7, 7, ships);
        INVALID_GRID = new GridConfigMessage(config);
    }
    AcceptingConfigMessageHandler _handler;
    public AcceptingConfigMessageHandlerTests()
    {
        _handler = new();
    }

    [Fact]
    public void TestInitialState()
    {
        Assert.Empty(_handler.Manifest.NickNames);
        Assert.Empty(_handler.Manifest.UserNames);
    }

    [Theory]
    [InlineData("Bob")]
    [InlineData("Susan")]
    public void TestNewValidGridConfig(string username)
    {
        INetworkPayload[] responses = _handler.HandleMessage(new NetworkMessage(username, VALID_GRID)).ToArray();
        Assert.Equal(2, responses.Length);
        Assert.Single(_handler.Manifest.NickNames);
        Assert.Single(_handler.Manifest.UserNames);
        Assert.Contains(username, _handler.Manifest.UserNames);
        Assert.Contains(typeof(ConfigAcceptedMessage), responses.Select(r => r.GetType()));
        Assert.Contains(typeof(PlayerJoinedMessage), responses.Select(r => r.GetType()));
    }

    [Theory]
    [InlineData("Bob", "Susan", "Sally")]
    [InlineData("Luke", "Leia", "Obi")]
    public void TestMultipleUsersValidGridConfig(params string[] usernames)
    {
        int count = 0;
        foreach (string username in usernames)
        {
            count++;
            INetworkPayload[] responses = _handler.HandleMessage(new NetworkMessage(username, VALID_GRID)).ToArray();
            Assert.Equal(2, responses.Length);
            Assert.Equal(count, _handler.Manifest.NickNames.Count());
            Assert.Equal(count, _handler.Manifest.UserNames.Count());
            Assert.Contains(username, _handler.Manifest.UserNames);
            Assert.Contains(typeof(ConfigAcceptedMessage), responses.Select(r => r.GetType()));
            Assert.Contains(typeof(PlayerJoinedMessage), responses.Select(r => r.GetType()));
        }
    }

    [Theory]
    [InlineData("Bob")]
    [InlineData("Sally")]
    public void TestAddInvalidValidGridConfig(string username)
    {
        var responses = _handler.HandleMessage(new NetworkMessage(username, INVALID_GRID)).ToArray();
        Assert.Single(responses);
        Assert.Empty(_handler.Manifest.NickNames);
        Assert.Empty(_handler.Manifest.UserNames);
        Assert.Contains(typeof(InvalidConfigMessage), responses.Select(r => r.GetType()));
    }

    [Theory]
    [InlineData("Bob")]
    [InlineData("Sally")]
    public void TestUpdateInvalidGridConfig(string username)
    {
        _handler.HandleMessage(new NetworkMessage(username, VALID_GRID));
        INetworkPayload[] responses = _handler.HandleMessage(new NetworkMessage(username, INVALID_GRID)).ToArray();
        Assert.Single(responses);
        Assert.Single(_handler.Manifest.NickNames);
        Assert.Single(_handler.Manifest.UserNames);
        Assert.Contains(username, _handler.Manifest.UserNames);
        Assert.Contains(typeof(InvalidConfigMessage), responses.Select(r => r.GetType()));
    }

    [Fact]
    public void TestDoNotAcceptFireMessages()
    {
        var responses = _handler.HandleMessage(new NetworkMessage("Bob", new FireMessage("Susan", (0,0))));
        Assert.Single(responses);
        Assert.Contains(typeof(InvalidConfigMessage), responses.Select(r => r.GetType()));
    }

}