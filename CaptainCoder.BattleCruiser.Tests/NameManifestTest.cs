namespace CaptainCoder.BattleCruiser.Client.Tests;
public class NameGeneratorTests
{
    [Fact(Timeout=5000)]
    public void TestGetNickName()
    {
        int trials = 10_000;
        NameManifest nameManifest = new ();
        Dictionary<string, string> usernames = new();
        for (int i = 0; i < trials; i++)
        {
            string username = $"username{i}";
            Assert.True(nameManifest.GetNickName(username, out string nickname));
            usernames[username] = nickname;
        }
        Assert.Equal(trials, usernames.Count);
        // Check for uniqueness
        Assert.Equal(trials, usernames.Values.ToHashSet().Count);

        foreach ((string username, string nickname) in usernames)
        {
            Assert.False(nameManifest.GetNickName(username, out string actualNickname));
            Assert.Equal(nickname, actualNickname);
        }

    }
}