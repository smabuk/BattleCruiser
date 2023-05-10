using CaptainCoder.BattleCruiser;
using CaptainCoder.BattleCruiser.Client;

ShipConfig[] ships = new ShipConfig[]
        {
            new ShipConfig((0, 0), ShipType.Battleship, Orientation.EastWest),
            new ShipConfig((1, 0), ShipType.Destroyer, Orientation.NorthSouth),
            new ShipConfig((2, 1), ShipType.Submarine, Orientation.EastWest),
        };
GridConfig config = new(7, 7, ships);

INetworkMessage gridConfigMessage = new GridConfigMessage(config);

ClientConnection connection = new ("localhost", 12345);

connection.OnConnected += SendMessageTest;
connection.OnConnecting += () => Console.WriteLine("Connecting...");
connection.OnDisconnected += () => Console.WriteLine("Disconnected!");
connection.OnMessageReceived += OnMessageReceived;

await connection.ConnectAndProcessMessages();

void SendMessageTest()
{
    Console.WriteLine("Connected!");
    connection.EnqueueMessage(gridConfigMessage);
}

void OnMessageReceived(INetworkMessage message)
{
    Console.WriteLine(message);
}