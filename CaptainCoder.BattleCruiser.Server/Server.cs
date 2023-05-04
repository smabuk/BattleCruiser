using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;

public class Server
{
    private readonly ConcurrentBag<ClientConnection> _connections = new();

    public async Task Start()
    {
        IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("localhost");
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint ipEndPoint = new(ipAddress, 11_000);

        using Socket listener = new(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        listener.Bind(ipEndPoint);
        listener.Listen(100);

        while (true)
        {
            Console.WriteLine("Waiting for connection...");
            Socket handler = await listener.AcceptAsync();
            ClientConnection client = new(handler, this);
            _connections.Add(client);
            _ = client.HandleConnection();
        }
    }

    public async Task SendMessage(string message)
    {
        foreach (ClientConnection connection in _connections)
        {
            var echoBytes = Encoding.UTF8.GetBytes(message);
            _ = connection.SendMessage(echoBytes);
        }
    }

}