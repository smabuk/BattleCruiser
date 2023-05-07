namespace CaptainCoder.BattleCruiser.Client;

using MQTTnet;
using MQTTnet.Client;

public interface IServerMessage {}

public record class GridConfigMessage(GridConfig Config) : IServerMessage;

// MqttApplicationMessage -- This is what gets sent

