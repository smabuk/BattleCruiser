using System.Text;
using System.Text.Json;

internal record JsonWrapper(string ClassName, string Json);

public static class NetworkSerializer
{
    public static byte[] Serialize(object toSerialize)
    {
        string json = JsonSerializer.Serialize(toSerialize);
        JsonWrapper wrapped = new (toSerialize.GetType().ToString(), json);
        string serializedJson = JsonSerializer.Serialize(wrapped);
        return Encoding.UTF8.GetBytes(serializedJson);
    }

    public static T Deserialize<T>(byte[] data)
    {
        string json = Encoding.UTF8.GetString(data);
        JsonWrapper wrapper = JsonSerializer.Deserialize<JsonWrapper>(json) ?? throw new NotSupportedException("Deserialized null object");
        Type wrappedType = Type.GetType(wrapper.ClassName);
        object deserialized = JsonSerializer.Deserialize(wrapper.Json, wrappedType) ?? throw new NotSupportedException("Deserialized null object");
        if (deserialized is T asType) { return asType; }
        throw new JsonException($"Found {deserialized.GetType()} and could not cast to {typeof(T)}.");
    }
}