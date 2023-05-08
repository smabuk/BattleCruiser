using MQTTnet;
using MQTTnet.Server;

namespace CaptainCoder.BattleCruiser.Server;

public class BattleCruiserServer
{
    public BattleCruiserServer(int port, bool logging = false) => (Port, Logging) = (port, logging);
    public int Port { get; }
    public bool Logging { get; }

    public async Task Start()
    {
         /*
         * This sample starts a simple MQTT server and prints the logs to the output.
         *
         * IMPORTANT! Do not enable logging in live environment. It will decrease performance.
         *
         * See sample "Run_Minimal_Server" for more details.
         */

        var mqttFactory = Logging ? new MqttFactory(new ConsoleLogger()) : new MqttFactory();

        var mqttServerOptions = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(Port)
            .Build();

        using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
        {
            
            mqttServer.ClientConnectedAsync += OnClientConnect;
            await mqttServer.StartAsync();

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            // Stop and dispose the MQTT server if it is no longer needed!
            await mqttServer.StopAsync();
        }
    }

    private Task OnClientConnect(ClientConnectedEventArgs args)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("CLIENT CONNECTED");
        Console.ResetColor();
        return Task.CompletedTask;
    }
}