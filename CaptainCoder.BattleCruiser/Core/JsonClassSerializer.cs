using System.Text.Json;

internal record JsonWrapper(string ClassName, string Json);

public static class JsonUtility
{
    public static string Serialize(object toSerialize)
    {
        string json = JsonSerializer.Serialize(toSerialize);
        JsonWrapper wrapped = new (toSerialize.GetType().ToString(), json);
        return JsonSerializer.Serialize(wrapped);
    }

    public static T Deserialize<T>(string json)
    {
        JsonWrapper wrapper = JsonSerializer.Deserialize<JsonWrapper>(json) ?? throw new NotSupportedException("Deserialized null object");
        Type wrappedType = Type.GetType(wrapper.ClassName);
        object deserialized = JsonSerializer.Deserialize(wrapper.Json, wrappedType) ?? throw new NotSupportedException("Deserialized null object");
        if (deserialized is T asType) { return asType; }
        throw new JsonException($"Found {deserialized.GetType()} and could not cast to {typeof(T)}.");
    }
}