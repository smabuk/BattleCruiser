using System.ComponentModel;
using System.Diagnostics;
using CaptainCoder.Core;

namespace CaptainCoder.BattleCruiser.Client;

// Devs always confuse halloween and christmas because Oct 31 equals Dec 25
public class GameHostClient : AbstractClient
{
    private IHostState? _hostState;
    public GameHostClient(string host, int port, string username) : base(host, port, username) {}

    public async Task Start(int registrationDuration, int roundDuration)
    {
        Log("Starting Host");
        _hostState = new RegistrationState(registrationDuration, roundDuration);
        do
        {
            await WaitForNextState();
            IEnumerable<INetworkPayload> messages = _hostState.Messages;
            messages.ForEach(SendPayload);
            _hostState = _hostState.NextState();
        } while(!_hostState.ShouldHalt);
        
        IEnumerable<INetworkPayload> finalMessages = _hostState.Messages;
        finalMessages.ForEach(SendPayload);
        
        Log("Host Stopped");
    }

    private async Task WaitForNextState()
    {
        Debug.Assert(_hostState != null);
        DateTime nextStateTime = DateTime.UtcNow.AddMilliseconds(_hostState.Duration);
        Log($"Entered {_hostState.GetType()} ... next state @ {nextStateTime}.");
        do
        {
            if (TryDequeueInbox(out NetworkMessage nextMessage))
            {
                Log($"Processing {nextMessage.Payload.GetType()} from {nextMessage.From}...");
                IEnumerable<INetworkPayload> messages = _hostState.MessageHandler.HandleMessage(nextMessage);
                messages.ForEach(message => SendPayload(message, nextMessage.From));
            }
            else
            {
                await Task.Delay(100);
            }
        } while (DateTime.UtcNow < nextStateTime);
        Log("Continuing to next state");
    }

    private void SendPayload(INetworkPayload payload) => SendPayload(payload);
    private void SendPayload(INetworkPayload payload, string? username = null)
    {
        Action<INetworkPayload> respond = payload switch
        {
            ConfigAcceptedMessage => PrivateResponse(username),
            InvalidConfigMessage => PrivateResponse(username),
            FireAcceptedMessage => PrivateResponse(username),
            FireRejectedMessage => PrivateResponse(username),
            

            PlayerJoinedMessage => BroadcastMessage,
            PlayerLeftMessage => BroadcastMessage,
            GameStartingMessage => BroadcastMessage,
            RoundResultMessage => BroadcastMessage,
            GameResultMessage => BroadcastMessage,
            GameStartingAt => BroadcastMessage,

            _ => throw new ArgumentException($"Could not handle payload of type {payload.GetType()}")
        };        
    }

    private Action<INetworkPayload> PrivateResponse(string? username)
    {
        Debug.Assert(username != null);
        return (payload) => PrivateMessage(payload, username);
    }
}