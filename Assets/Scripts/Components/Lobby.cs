using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// RPC sent when the players are ready to start the race
    /// </summary>
    public struct PlayersReadyRPC : IRpcCommand
    {
    }

    /// <summary>
    /// RPC sent when any player cancel the event to start the race
    /// </summary>
    public struct CancelPlayerReadyRPC : IRpcCommand
    {
    }

    /// <summary>
    /// RPC sent to reset a car to the initial position
    /// </summary>
    public struct ResetCarRPC : IRpcCommand
    {
        public int Id;
    }
}