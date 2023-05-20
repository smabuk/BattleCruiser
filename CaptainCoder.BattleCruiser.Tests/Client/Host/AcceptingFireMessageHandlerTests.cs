using Moq;
using Shouldly;
using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser.Client.Tests;

public class AcceptingFireMessageHandlerTests
{

    private AcceptingFireMessageHandler InitMockedPlayerGrid(params string[] usernames)
    {

        Dictionary<string, IPlayerGrid> playerGrids = new();
        NameManifest nickNames = new ();
        foreach (string username in usernames)
        {
            nickNames.GetNickName(username, out string nickName);
            Mock<IPlayerGrid> mocked = new ();
            playerGrids[nickName] = mocked.Object;
        }
        return new AcceptingFireMessageHandler(playerGrids, nickNames);
    }

    [Fact]
    public void TestAttackingPlayerIsNotInGame()
    {
        AcceptingFireMessageHandler handler = InitMockedPlayerGrid("Bob", "Sally");
        IEnumerable<INetworkPayload> responses = handler.HandleMessage(new NetworkMessage("Steve", new FireMessage("Bob", (0, 0))));
        responses.ShouldHaveSingleItem();
        INetworkPayload actual = responses.First();
        actual.ShouldBeOfType<FireRejectedMessage>();
    }

    [Fact]
    public void TestAttackingPlayerIsNotAlive()
    {
        Mock<IPlayerGrid> alivePlayerMock = new ();
        alivePlayerMock.Setup((x) => x.IsAlive).Returns(true);
        Mock<IPlayerGrid> deadPlayerMock = new ();
        alivePlayerMock.Setup((x) => x.IsAlive).Returns(false);
        Dictionary<string, IPlayerGrid> playerGrids = new ()
        {
            {"Bob", alivePlayerMock.Object},
            {"Sally", deadPlayerMock.Object}
        };
        INameManifest manifest = new []{ "Bob", "Sally" }.ToManifest();
        AcceptingFireMessageHandler handler = new (playerGrids, manifest);
        var responses = handler.HandleMessage(new NetworkMessage("Sally", new FireMessage("Bob", (0, 0))));
        responses.ShouldHaveSingleItem();
        INetworkPayload actual = responses.First();
        actual.ShouldBeOfType<FireRejectedMessage>();
    }

    [Fact]
    public void TestTargetPlayerIsNotInGame()
    {
        AcceptingFireMessageHandler handler = InitMockedPlayerGrid("Bob", "Sally");
        IEnumerable<INetworkPayload> responses = handler.HandleMessage(new NetworkMessage("Bob", new FireMessage("Steve", (0, 0))));
        responses.ShouldHaveSingleItem();
        INetworkPayload actual = responses.First();
        actual.ShouldBeOfType<FireRejectedMessage>();
    }

    [Fact]
    public void TestTargetPlayerIsNotAlive()
    {
        Mock<IPlayerGrid> alivePlayerMock = new ();
        alivePlayerMock.Setup((x) => x.IsAlive).Returns(true);
        Mock<IPlayerGrid> deadPlayerMock = new ();
        deadPlayerMock.Setup((x) => x.IsAlive).Returns(false);
        Dictionary<string, IPlayerGrid> playerGrids = new ()
        {
            {"Bob", alivePlayerMock.Object},
            {"Sally", deadPlayerMock.Object}
        };
        INameManifest manifest = new []{ "Bob", "Sally" }.ToManifest();
        AcceptingFireMessageHandler handler = new (playerGrids, manifest);
        var responses = handler.HandleMessage(new NetworkMessage("Bob", new FireMessage("Sally", (0, 0))));
        responses.ShouldHaveSingleItem();
        INetworkPayload actual = responses.First();
        actual.ShouldBeOfType<FireRejectedMessage>();
    }

    [Fact]
    public void TestTargetPreviouslyAttackedSpace()
    {
        Mock<IPlayerGrid> bobMock = new ();
        bobMock.Setup((x) => x.IsAlive).Returns(true);
        Mock<IPlayerGrid> sallyMock = new ();
        sallyMock.Setup((x) => x.IsAlive).Returns(true);
        InfoGrid sallyGrid = new ();
        sallyGrid[(0,0)] = IGridMark.Miss;
        sallyMock.Setup((x) => x.Grid).Returns(sallyGrid);
        Dictionary<string, IPlayerGrid> playerGrids = new ()
        {
            {"Bob", bobMock.Object},
            {"Sally", sallyMock.Object}
        };
        INameManifest manifest = new []{ "Bob", "Sally" }.ToManifest();
        AcceptingFireMessageHandler handler = new (playerGrids, manifest);
        var responses = handler.HandleMessage(new NetworkMessage("Bob", new FireMessage("Sally", (0, 0))));
        responses.ShouldHaveSingleItem();
        INetworkPayload actual = responses.First();
        actual.ShouldBeOfType<FireRejectedMessage>();
    }

    [Fact]
    public void TestTargetOutOfBounds()
    {
        AcceptingFireMessageHandler handler = InitMockedPlayerGrid("Bob", "Sally");
        IEnumerable<INetworkPayload> responses = handler.HandleMessage(new NetworkMessage("Bob", new FireMessage("Bob", (-1, -1))));
        responses.ShouldHaveSingleItem();
        INetworkPayload actual = responses.First();
        actual.ShouldBeOfType<FireRejectedMessage>();
    }

    [Fact]
    public void TestValidFireMessage()
    {
        Mock<IPlayerGrid> bobMock = new ();
        bobMock.Setup((x) => x.IsAlive).Returns(true);
        Mock<IPlayerGrid> sallyMock = new ();
        sallyMock.Setup((x) => x.IsAlive).Returns(true);
        InfoGrid sallyGrid = new ();
        sallyMock.Setup((x) => x.Grid).Returns(sallyGrid);
        Dictionary<string, IPlayerGrid> playerGrids = new ()
        {
            {"Bob", bobMock.Object},
            {"Sally", sallyMock.Object}
        };
        INameManifest manifest = new []{ "Bob", "Sally" }.ToManifest();
        AcceptingFireMessageHandler handler = new (playerGrids, manifest);
        var responses = handler.HandleMessage(new NetworkMessage("Bob", new FireMessage("Sally", (0, 0))));
        responses.ShouldHaveSingleItem();
        INetworkPayload actual = responses.First();
        actual.ShouldBeOfType<FireAcceptedMessage>();
    }

    [Fact]
    public void TestDoNotAcceptConfigMessages()
    {
        Ship[] ships = Array.Empty<Ship>();
        PlayerConfig config = new (ships);
        GridConfigMessage configMessage = new (config);
        var responses = InitMockedPlayerGrid("Bob").HandleMessage(new NetworkMessage("Bob", configMessage));
        Assert.Single(responses);
        Assert.Contains(typeof(FireRejectedMessage), responses.Select(r => r.GetType()));
    }

}