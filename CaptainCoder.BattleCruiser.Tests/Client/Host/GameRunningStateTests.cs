using Moq;
using Shouldly;
using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser.Client.Tests;

public class GameRunningStateTests
{

// public static FireResult[] 
// GenerateFireResults(IReadOnlyDictionary<string, IPlayerGrid> PlayerGrids, 
// IReadOnlyDictionary<string, FireMessage> PlayerTargets)

    [Fact]
    public void TestGenerateFireResults()
    {
        string Sally = "Sally";
        string Bob = "Bob";
        string Dusty = "Dusty";
        // Bob
        // (0, 0)
        // (1, 0)
        // (2, 0)
        PlayerConfig bobsConfig = new(
            new []{
                new Ship((0,0), ShipType.Submarine, Orientation.Vertical)
            }
        );
        // Sally
        // (1, 1)
        // (1, 2)
        PlayerConfig sallysConfig = new(
            new []{
                new Ship((1,1), ShipType.Destroyer, Orientation.Horizontal)
            }
        );
        // Dusty
        // (0, 2)
        // (0, 3)
        PlayerConfig dustyConfig = new(
            new []{
                new Ship((0,2), ShipType.Destroyer, Orientation.Horizontal)
            }
        );

        Dictionary<string, IPlayerGrid> playerGrids = new()
        {
            {Bob, new PlayerGrid(Bob, bobsConfig)},
            {Sally, new PlayerGrid(Sally, sallysConfig)},
            {Dusty, new PlayerGrid(Dusty, dustyConfig)},
        };

        Dictionary<string, FireMessage> targets = new()
        {
            {Bob, new FireMessage(Sally, (0,0))},
            {Dusty, new FireMessage(Sally, (0,0))},
            {Sally, new FireMessage(Bob, (1,0))},
        };

        FireResult[] results = GameRunningState.ApplyFireMessages(playerGrids, targets);
        
        results.Length.ShouldBe(2);
        FireResult actualSally = results.Where(result => result.TargetId == Sally).First();
        actualSally.TargetId.ShouldBe(Sally);
        actualSally.Position.ShouldBe(new Position(0,0));
        actualSally.Result.ShouldBe(new AttackResult(IGridMark.Miss));
        actualSally.AttackerIds.Length.ShouldBe(2);
        actualSally.AttackerIds.ShouldContain(Bob);
        actualSally.AttackerIds.ShouldContain(Dusty);

        FireResult bobAttacked = results.Where(result => result.TargetId == Bob).First();
        bobAttacked.TargetId.ShouldBe(Bob);
        bobAttacked.Position.ShouldBe(new Position(1,0));
        bobAttacked.Result.ShouldBe(new AttackResult(IGridMark.Hit(ShipType.Submarine)));
        bobAttacked.AttackerIds.ShouldHaveSingleItem();
        bobAttacked.AttackerIds.ShouldContain(Sally);

        // Bob
        // (0, 0)
        // (1, 0) -- Hit
        // (2, 0)

        // Sally
        // (1, 1)
        // (1, 2)

        // Dusty
        // (0, 2)
        // (0, 3)
        targets = new()
        {
            {Bob, new FireMessage(Sally, (1,1))},
            {Dusty, new FireMessage(Bob, (0,0))},
            {Sally, new FireMessage(Bob, (2,0))},
        };

        results = GameRunningState.ApplyFireMessages(playerGrids, targets);
        results.Length.ShouldBe(3);

        actualSally = results.Where(result => result.TargetId == Sally).First();
        FireResult expected = new (Sally, (1,1), new AttackResult(IGridMark.Hit(ShipType.Destroyer)), new []{Bob});
        
        var bobHits = results.Where(result => result.TargetId == Bob).ToArray();
        bobHits.Length.ShouldBe(2);
        
        FireResult bobHit00 = bobHits.Where(results => results.Position == new Position(0,0)).First();
        expected = new (Bob, (0,0), new SunkResult(ShipType.Submarine), new []{Dusty});
        bobHit00.ShouldBeEquivalentTo(expected);

        FireResult bobHit20 = bobHits.Where(results => results.Position == new Position(2,0)).First();
        expected = new (Bob, (2,0), new SunkResult(ShipType.Submarine), new []{Sally});

        // Bob
        // (0, 0) -- Sunk
        // (1, 0) -- Sunk
        // (2, 0) -- Sunk

        // Sally
        // (1, 1) -- Hit
        // (1, 2)

        // Dusty
        // (0, 2)
        // (0, 3)

        targets = new()
        {
            {Dusty, new FireMessage(Sally, (1,2))},
            {Sally, new FireMessage(Dusty, (0,2))},
        };
        
        results = GameRunningState.ApplyFireMessages(playerGrids, targets);
        results.Length.ShouldBe(2);

        actualSally = results.Where(result => result.TargetId == Sally).First();
        expected = new (Sally, (1,2), new SunkResult(ShipType.Destroyer), new []{Dusty});
        actualSally.ShouldBeEquivalentTo(expected);

        var actualDusty = results.Where(result => result.TargetId == Dusty).First();
        expected = new (Dusty, (0,2), new AttackResult(IGridMark.Hit(ShipType.Destroyer)), new []{Sally});
        actualDusty.ShouldBeEquivalentTo(expected);
    }


}