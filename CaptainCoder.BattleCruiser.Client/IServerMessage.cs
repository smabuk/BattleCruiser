namespace CaptainCoder.BattleCruiser.Client;

using MQTTnet;
using MQTTnet.Client;

public interface IServerMessage
{
    public string PayloadString { get; }
}

public class ServerMessage : IServerMessage
{
    public MqttApplicationMessage Message { get; }

    public ServerMessage(MqttApplicationMessage message)
    {
        Message = message;
        PayloadString = Message.ConvertPayloadToString();
    }

    public string PayloadString {get; }
}

