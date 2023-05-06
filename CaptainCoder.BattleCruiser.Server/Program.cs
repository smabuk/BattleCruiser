using CaptainCoder.BattleCruiser.Server;

BattleCruiserServer server = new(12345, true);

await server.Start();


// await Server_Simple_Samples.Run_Server_With_Logging();