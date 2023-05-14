namespace CaptainCoder.BattleCruiser.Client;

public record NetworkMessage(string From, INetworkPayload Payload);