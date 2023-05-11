using CaptainCoder.BattleCruiser;
using CaptainCoder.BattleCruiser.Client;

GameHostClient hostClient = new ("localhost", 12345);

await hostClient.Connect();