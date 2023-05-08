using MQTTnet;
namespace CaptainCoder.BattleCruiser;

public static class NetworkMessageExtensions
{
    public static MqttApplicationMessage ToMqttApplicationMessage(this INetworkMessage networkMessage)
    {
        byte[] payload = NetworkSerializer.Serialize(networkMessage);
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
            .WithPayload(payload)
            .Build();
        return message;
    }

    public static INetworkMessage ToNetworkMessage(this MqttApplicationMessage message) => NetworkSerializer.Deserialize<INetworkMessage>(message.PayloadSegment.ToArray());
}