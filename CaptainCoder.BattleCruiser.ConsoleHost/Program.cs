using CommandLine;
using CaptainCoder.BattleCruiser.Client;

await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(Run);

async Task Run(Options options)
{
    Console.WriteLine(options.IP);
    GameHostClient hostClient = new (options.IP, options.Port, options.UserName);
    hostClient.IsLogging = true;
    await hostClient.Connect();
}

void HandleErrors(IEnumerable<Error> errors)
{
    
}

class Options
{
    [Option('h', "host", Required = false, HelpText = "The IP Address of the MQTT server. Defaults to localhost")]
    public string IP { get; set; } = "localhost";
    [Option('p', "port", Required = false, HelpText = "The port to connect to. Defaults to 12345")]
    public int Port { get; set; } = 12345;
    [Option('u', "username", Required = true, HelpText = "The UserName to connect with.")]
    public string UserName { get; set; }
}