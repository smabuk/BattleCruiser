using System.Net.Sockets;
using System.Text;

public class ClientConnection
{

    private Socket _socket;
    private Server _server;

    public ClientConnection(Socket connection, Server server)
    {
        _socket = connection;
        _server = server;
    }

    public async Task HandleConnection()
    {
        Console.WriteLine("Connection Open");
        while (true)
        {
            // Receive message.
            var buffer = new byte[1_024];
            var received = await _socket.ReceiveAsync(buffer, SocketFlags.None);
            var clientMessage = Encoding.UTF8.GetString(buffer, 0, received);

            var eom = "<|EOM|>";
            if (clientMessage.IndexOf(eom) > -1 /* is end of message */)
            {
                Console.WriteLine(
                    $"Socket server received message: \"{clientMessage.Replace(eom, "")}\"");

                var ackMessage = "<|ACK|>";
                var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                await _socket.SendAsync(echoBytes, 0);
                Console.WriteLine(
                    $"Socket server sent acknowledgment: \"{ackMessage}\"");

                break;
            }
            Console.WriteLine($"Received Message from Client: {clientMessage}");
            _ = _server.SendMessage(clientMessage);
            // TODO: Propogate message to clients
            // Sample output:
            //    Socket server received message: "Hi friends 👋!"
            //    Socket server sent acknowledgment: "<|ACK|>"
        }
    }

    public Task<int> SendMessage(byte[] message)
    {
        return _socket.SendAsync(message);
    }


}