using System.Diagnostics.CodeAnalysis;

namespace CaptainCoder.BattleCruiser.Client;

public interface INameManifest
{
    public IReadOnlyCollection<string> NickNames { get; }
    public IReadOnlyCollection<string> UserNames { get; }

    /// <summary>
    /// Given a <paramref name="username"/> retrieves the users nickname. Returns
    /// true if a nickname was generated and false if the nickname was already found.
    /// </summary>
    public bool GetNickName(string username, out string nickname);
}

public class NameManifest : INameManifest
{
    public static readonly NameGenerator NicknameGenerator;

    static NameManifest()
    {
        string[] adjective = {
            "Abundant", "Brilliant", "Calm", "Dazzling", "Elegant", "Fierce", "Gentle", "Harmonious", "Intrepid", "Joyful",
            "Lively", "Majestic", "Noble", "Optimistic", "Playful", "Radiant", "Serene", "Tranquil", "Vibrant", "Whimsical",
            "Zealous", "Adventurous", "Benevolent", "Charismatic", "Diligent", "Enchanting", "Fascinating", "Gracious",
            "Humble", "Inquisitive", "Jubilant", "Knowledgeable", "Modest", "Passionate", "Resilient", "Sincere", "Thoughtful",
            "Versatile", "Witty", "Yielding", "Zesty", "Ambitious", "Bountiful", "Cheerful", "Delightful", "Enthusiastic",
            "Fearless", "Generous", "Hopeful", "Inspiring", "Jovial", "Kindhearted", "Magical", "Nurturing", "Open-minded",
            "Peaceful", "Radiant", "Spirited", "Tenacious", "Unwavering", "Vivacious", "Wondrous", "Zealful", "Captivating",
            "Dynamic", "Euphoric", "Fascinating", "Graceful", "Honest", "Inventive", "Jubilant", "Kind", "Marvelous",
            "Nurturing", "Optimistic", "Passionate", "Reliable", "Sincere", "Tranquil", "Upbeat", "Versatile", "Wise",
            "Youthful", "Zestful", "Adventurous", "Balanced", "Charming", "Daring", "Effervescent", "Free-spirited", "Genuine",
            "Humble", "Inspiring", "Joyous", "Lively", "Motivated", "Noble", "Original", "Playful", "Quirky"
        };
        string[] color = { "Red", "Green", "Blue", "Yellow", "Purple", "Orange", "Pink", "Cyan", "Magenta", "Brown" };
        string[] animal = {
            "Lion", "Tiger", "Elephant", "Giraffe", "Monkey", "Zebra", "Kangaroo", "Penguin", "Koala", "Dolphin",
            "Leopard", "Cheetah", "Hippopotamus", "Panda", "Gorilla", "Crocodile", "Flamingo", "Sloth", "Ostrich", "Llama",
            "Polarbear", "Peacock", "Octopus", "Rhino", "Pangolin", "Squirrel", "Jaguar", "Lynx", "Otter", "Camel",
            "Hedgehog", "Eagle", "Seahorse", "Meerkat", "Gazelle", "Chameleon", "Cobra", "Koalabear", "Walrus", "Armadillo",
            "Baboon", "Coyote", "Jellyfish", "Lemur", "Puffin", "Raccoon", "Starfish", "Tapir", "Wombat", "Yak",
            "Buffalo", "Dromedary", "Fennecfox", "Gibbon", "Iguana", "Jackal", "Kookaburra", "Nightingale", "Ocelot", "Quokka",
            "Reindeer", "Salamander", "Toucan", "Umbrellabird", "Vulture", "Wildebeest", "Xenopus", "Yabby", "Zebu", "Flamingo",
            "Panda", "Giraffe", "Kangaroo", "Lion", "Elephant", "Tiger", "Monkey", "Zebra", "Koala", "Dolphin",
            "Leopard", "Cheetah", "Penguin", "Hippopotamus", "Gorilla", "Crocodile", "Sloth", "Rhino", "Jaguar", "Otter"
        };
        NicknameGenerator = new NameGenerator(adjective, color, animal);
    }

    private readonly Dictionary<string, string> _userNametoNickName = new();
    private readonly Dictionary<string, string> _nickNametoUserName = new();

    /// <summary>
    /// Retrieves all UserNames that have been added to this manifest.
    /// </summary>
    public IReadOnlyCollection<string> UserNames => _userNametoNickName.Keys;
    /// <summary>
    /// Retrieves all NickNames that have been added to this manifest.
    /// </summary>
    public IReadOnlyCollection<string> NickNames => _nickNametoUserName.Keys;

    /// <summary>
    /// Given a <paramref name="username"/> retrieves the users nickname. Returns
    /// true if a nickname was generated and false if the nickname was already found.
    /// </summary>
    public bool GetNickName(string username, out string nickname)
    {
        if (_userNametoNickName.TryGetValue(username, out nickname))
        {
            return false;
        }
        do
        {
            nickname = NicknameGenerator.GenerateName();
        } while (_nickNametoUserName.ContainsKey(nickname));
        _userNametoNickName[username] = nickname;
        _nickNametoUserName[nickname] = username;
        return true;
    }

    public bool TryGetNickName(string username, [MaybeNullWhen(false)] out string nickname) => 
        _userNametoNickName.TryGetValue(username, out nickname);

}