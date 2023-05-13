using CaptainCoder.Core;
namespace CaptainCoder.BattleCruiser;

public interface INetworkPayload {}

/// <summary>
/// A <see cref="GridConfigMessage"/> is sent by a client when they join a game
/// to specify their grid state.
/// </summary>
public record class GridConfigMessage(GridConfig Config) : INetworkPayload;

/// <summary>
/// A <see cref="IGridConfigResultMessage"/> is sent by the server to acknowledge a
/// player's <see cref="GridConfigMessage"/>
/// </summary>
public interface IGridConfigResultMessage : INetworkPayload {};

/// <summary>
/// Sent when the player's configuration was accepted.
/// </summary>
public record class ConfigAcceptedMessage : IGridConfigResultMessage;

/// <summary>
/// Sent when the player's configuration was not accepted.
/// </summary>
public record class InvalidConfigMessage(string Message) : IGridConfigResultMessage; 

/// <summary>
/// A <see cref="PlayerJoinedMessage"/> is sent by the server when a player has
/// successfully submitted a <see cref="GridConfigMessage"/>. This message is
/// broadcast to all players.
/// </summary>
public record class PlayerJoinedMessage(string PlayerId) : INetworkPayload;

/// <summary>
/// A <see cref="PlayerLeftMessage"/> is sent by the server when a player
/// disconnects from the server. This message is broadcast to all players.
/// </summary>
public record class PlayerLeftMessage(string PlayerId) : INetworkPayload;

/// <summary>
/// A <see cref="InitialBattleGridMessage"/> is sent by the server when the
/// battle begins. This message is broadcast to all players.
/// </summary>
// TODO: Needs to include Width / Height and Array of UserIDs / position on board
public record class InitialBattleGridMessage() : INetworkPayload;

public record class FireMessage(string PlayerId, Position Target) : INetworkPayload;
public interface IFireMessageResult : INetworkPayload {} 
public record class FireAcceptedMessage : IFireMessageResult;
public record class FireRejectedMessage(string Message) : IFireMessageResult;

// TODO: Needs to include Round #, Hit / Miss information, Eliminated Player Info
public record class RoundResult() : INetworkPayload;


public record class GameResult(int TotalRounds, string[] WinnerIds) : INetworkPayload;


// public record class 
// MqttApplicationMessage -- This is what gets sent