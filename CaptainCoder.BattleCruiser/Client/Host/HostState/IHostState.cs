using System.Diagnostics;
using CaptainCoder.Core;

namespace CaptainCoder.BattleCruiser.Client;

public interface IHostState
{
    /// <summary>
    /// The message handler to user for incoming messages
    /// </summary>
    public IMessageHandler MessageHandler { get; }
    /// <summary>
    /// Approximately how often, in milliseconds, the Tick() method should occur
    /// </summary>
    public int Duration { get; }
    /// <summary>
    /// Ticks the current state returning the next state as well as any messages
    /// to broadcast.
    /// </summary>
    public IEnumerable<INetworkPayload> Messages { get; }
    public IHostState NextState();
    public bool ShouldHalt { get; }
    
    
}
