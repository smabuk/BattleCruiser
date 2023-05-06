using CaptainCoder.BattleCruiser.Client;

ClientConnection connection = new("localhost", 12345);

connection.OnMessageReceived += MessageReceived;
connection.OnConnected += Connected;
connection.OnDisconnected += Disconnect;
connection.OnConnecting += Connecting;

_ = connection.Connect();

bool running = true;
while (running)
{
    await Task.Delay(100);
}

void MessageReceived(IServerMessage message)
{
    Console.WriteLine($"MESSAGE: {message.PayloadString}");
}

void Disconnect()
{
    Console.WriteLine("Disconnected.");
    running = false;
}

void Connected()
{
    Console.WriteLine("Connected!");
    HandleUserInput();
}

void Connecting()
{
    Console.WriteLine("Connecting...");
}

void HandleUserInput()
{
    Console.WriteLine("HERE?");
    Console.WriteLine($"IsConnected: {connection.IsConnected}");
    while (connection.IsConnected)
    {
        Console.Write(" > ");
        string input = Console.ReadLine()!;
        connection.EnqueueMessage(input);
    }
}