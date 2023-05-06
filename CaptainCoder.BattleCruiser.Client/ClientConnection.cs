using System.Net;
using System.Net.Sockets;
using System.Text;
namespace CaptainCoder.BattleCruiser.Client;

public class ClientConnection
{

    public static async Task Run()
    {

        // 127.0.0.1 == localhost
        // 0.0.0.0
        IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("localhost");
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint ipEndPoint = new(ipAddress, 12345);

        using Socket client = new(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        await client.ConnectAsync(ipEndPoint);

        _ = ListenToServer();

        await HandleUserInput();

        async Task HandleUserInput()
        {
            while (true)
            {
                string userInput = Console.ReadLine()!;
                byte[] messageBytes;

                if (userInput == "END")
                {
                    messageBytes = Encoding.UTF8.GetBytes("<|EOM|>");
                    _ = await client.SendAsync(messageBytes, SocketFlags.None);
                    break;
                }

                messageBytes = Encoding.UTF8.GetBytes(userInput);
                _ = await client.SendAsync(messageBytes, SocketFlags.None);
            }
            client.Shutdown(SocketShutdown.Both);
        }

        async Task ListenToServer()
        {
            while (true)
            {
                // Send message.

                // var message = "Hi friends 👋!<|EOM|>";
                // var messageBytes = Encoding.UTF8.GetBytes(message);
                // _ = await client.SendAsync(messageBytes, SocketFlags.None);
                // Console.WriteLine($"Socket client sent message: \"{message}\"");

                // Receive ack.
                var buffer = new byte[1_024];
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                var incommingMessage = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine($"Message from Server: \"{incommingMessage}\"");
                // if (response == "<|ACK|>")
                // {
                //     Console.WriteLine(
                //         $"Socket client received acknowledgment: \"{response}\"");
                // break;
                // }
                // Sample output:
                //     Socket client sent message: "Hi friends 👋!<|EOM|>"
                //     Socket client received acknowledgment: "<|ACK|>"
            }

        }

    }

}