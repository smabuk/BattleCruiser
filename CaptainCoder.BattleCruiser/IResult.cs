public interface IResult<T> {};
public sealed record Disconnect<T> : IResult<T>;
public sealed record Message<T>(T Value) : IResult<T>;
