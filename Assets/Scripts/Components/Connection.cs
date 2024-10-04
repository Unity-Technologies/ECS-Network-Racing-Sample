using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Tags component to allow auto connection to the multiplayer server
    /// </summary>
    public struct AutoConnect : IComponentData { }

    /// <summary>
    /// Sets the state for resetting the server if all players got disconnected.
    /// </summary>
    public struct ResetServerOnDisconnect : IComponentData
    {
    }

    public struct TimeOutServer : IComponentData
    {
        public double Value;
    }
}