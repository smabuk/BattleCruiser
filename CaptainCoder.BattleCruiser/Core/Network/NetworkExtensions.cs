using MQTTnet;
namespace CaptainCoder.BattleCruiser;

public static class NetworkExtensions
{
    public static MqttApplicationMessage ToMqttApplicationMessage(this INetworkPayload networkMessage)
    {
        byte[] payload = NetworkSerializer.Serialize(networkMessage);
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
            .WithPayload(payload)
            .Build();
        return message;
    }

    public static INetworkPayload ToNetworkMessage(this MqttApplicationMessage message) => NetworkSerializer.Deserialize<INetworkPayload>(message.PayloadSegment.ToArray());
}