using Moq;
using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser.Tests;

public class NameGeneratorTests
{
    [Fact]
    public void Test3NameGenerator()
    {
        string[] size = { "Big", "Small", "Tiny" };
        string[] color = { "Yellow", "Blue", "Green", "Red" };
        string[] noun = { "Cat", "Frog" };

        Mock<IRandom> randomMock = new();
        randomMock.Setup((random) => random.Next(0, 3)).Returns(2); // Tiny
        randomMock.Setup((random) => random.Next(0, 4)).Returns(1); // Blue
        randomMock.Setup((random) => random.Next(0, 2)).Returns(0); // Cat
        NameGenerator generator = new(size, color, noun);

        string actual = generator.GenerateName(randomMock.Object);
        Assert.Equal("TinyBlueCat", actual);
    }
}