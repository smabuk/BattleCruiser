using CaptainCoder.BattleCruiser;
using CaptainCoder.BattleCruiser.Client;

ShipConfig[] ships = new ShipConfig[]
        {
            new ShipConfig((0, 0), ShipType.Battleship, Orientation.EastWest),
            new ShipConfig((1, 0), ShipType.Destroyer, Orientation.NorthSouth),
            new ShipConfig((2, 1), ShipType.Submarine, Orientation.EastWest),
        };
GridConfig config = new(7, 7, ships);

INetworkPayload gridConfigMessage = new GridConfigMessage(config);

PlayerClient playerClient = new ("localhost", 12345, "SomePlayer", "TheCaptainCoder");
playerClient.IsLogging = true;

playerClient.OnConnected += SendMessageTest;

await playerClient.Connect();

void SendMessageTest()
{
    Console.WriteLine("Connected!");
    playerClient.EnqueueMessage(gridConfigMessage, "private/TheCaptainCoder");
}