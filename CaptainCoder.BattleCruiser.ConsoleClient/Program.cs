using CaptainCoder.BattleCruiser;
using CaptainCoder.BattleCruiser.Client;



ShipConfig[] ships = new ShipConfig[]
        {
            new ShipConfig((0, 0), ShipType.Battleship, Orientation.EastWest),
            new ShipConfig((1, 0), ShipType.Destroyer, Orientation.NorthSouth),
            new ShipConfig((2, 1), ShipType.Submarine, Orientation.EastWest),
        };
GridConfig config = new(7, 7, ships);

// Console.WriteLine(config.ToJson());

IServerMessage message = new GridConfigMessage(config);
string json = JsonUtility.Serialize(message);
Console.WriteLine(json);

IServerMessage result = JsonUtility.Deserialize<IServerMessage>(json);
Console.WriteLine(result);

GridConfigMessage gridConfigMessage = result as GridConfigMessage;
Console.WriteLine(string.Join(", ", gridConfigMessage.Config.Ships));
// HandleMessage(result);

// void HandleMessage(IServerMessage toHandle)
{
    // Use factor or whatever to decode appropriately
}


// ClientConnection connection = new("localhost", 12345);

// connection.OnMessageReceived += MessageReceived;
// connection.OnConnected += Connected;
// connection.OnDisconnected += Disconnect;
// connection.OnConnecting += Connecting;

// _ = connection.Connect();

// bool running = true;
// while (running)
// {
//     await Task.Delay(100);
// }

// void MessageReceived(IServerMessage message)
// {
//     Console.WriteLine($"MESSAGE: {message.PayloadString}");
// }

// void Disconnect()
// {
//     Console.WriteLine("Disconnected.");
//     running = false;
// }

// void Connected()
// {
//     Console.WriteLine("Connected!");
//     HandleUserInput();
// }

// void Connecting()
// {
//     Console.WriteLine("Connecting...");
// }

// void HandleUserInput()
// {
//     Console.WriteLine("HERE?");
//     Console.WriteLine($"IsConnected: {connection.IsConnected}");
//     while (connection.IsConnected)
//     {
//         Console.Write(" > ");
//         string input = Console.ReadLine()!;
//         connection.EnqueueMessage(input);
//     }
// }