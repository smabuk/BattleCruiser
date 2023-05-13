namespace CaptainCoder.BattleCruiser.Client;

public record NetworkMessage(string ClientId, INetworkPayload Payload);