using CommandLine;
using CaptainCoder.BattleCruiser.Client;
using CaptainCoder.BattleCruiser;

await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(Run);

async Task Run(Options options)
{
    if (options.Mode is ClientMode.Player)
    {
        await RunPlayer(options);
    }
    else
    {
        await RunHost(options);
    }
}

async Task RunPlayer(Options options)
{
    Console.Clear();
    Console.WriteLine("Running in Player Mode:");
    string? hostUserName = options.Host?.Trim();
    if (!ValidateHostName(hostUserName))
    {
        hostUserName = PromptUser("Enter a HostName", ValidateHostName);
    }
    PlayerClient playerClient = new(options.IP, options.Port, options.UserName, hostUserName!);
    playerClient.IsLogging = true;
    // await playerClient.Connect();

    Ship[] ships = new Ship[]
        {
            new Ship((0, 0), ShipType.Battleship, Orientation.EastWest),
            new Ship((1, 0), ShipType.Destroyer, Orientation.NorthSouth),
            new Ship((2, 1), ShipType.Submarine, Orientation.EastWest),
        };
    PlayerConfig config = new(7, 7, ships);

    INetworkPayload gridConfigMessage = new GridConfigMessage(config);

    playerClient.IsLogging = true;

    playerClient.OnConnected += SendMessageTest;

    await playerClient.Connect();

    void SendMessageTest()
    {
        Console.WriteLine("Connected!");
        playerClient.EnqueueMessage(gridConfigMessage, "private/TheCaptainCoder");
    }  
}

async Task RunHost(Options options)
{
    Console.Clear();
    Console.WriteLine("Running in Host Mode:");
    GameHostClient hostClient = new(options.IP, options.Port, options.UserName);
    hostClient.IsLogging = true;
    await hostClient.Connect();
}


bool ValidateHostName(string? name)
{
    string? trimmed = name?.Trim();
    if (string.IsNullOrWhiteSpace(trimmed)) { return false; }
    // More checks here
    return true;
}

string PromptUser(string prompt, Predicate<string?> validator)
{
    Console.Write($"{prompt}:");
    string input = Console.ReadLine()!;
    if (!validator(input))
    {
        Console.WriteLine("Invalid input");
        return PromptUser(prompt, validator);
    }
    return input.Trim();
}

class Options
{
    [Option('u', "username", Required = true, HelpText = "The UserName to connect with.")]
    public string UserName { get; set; } = null!;
    [Option('i', "ip", Required = false, HelpText = "The IP Address of the MQTT server. Defaults to localhost")]
    public string IP { get; set; } = "localhost";
    [Option('p', "port", Required = false, HelpText = "The port to connect to. Defaults to 12345")]
    public int Port { get; set; } = 12345;
    [Option('m', "mode", Required = false, HelpText = "The mode to run in. Player or Host.")]
    public ClientMode Mode { get; set; } = ClientMode.Player;
    [Option('h', "host", Required = false, HelpText = "The name of the game Host when run in Player Mode.")]
    public string? Host { get; set; }
}

public enum ClientMode
{
    Player,
    Host,
}