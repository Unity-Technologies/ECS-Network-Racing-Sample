using Unity.Entities.Racing.Common;
using Unity.NetCode;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Creates a player automatically for testing a race scene
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct TestStartRaceSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<Race>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();
            if (race.State is RaceState.StartingRaceAutomatically) // TODO: replace with component tag
            {
                if (TryGetSingleton<NetworkId>(out var carNetworkId))
                {
                    foreach (var ghostOwner in Query<RefRO<GhostOwner>>()
                                 .WithAll<Player>()
                                 .WithAll<Rank>()
                                 .WithAll<GhostOwner>())
                    {
                        if (ghostOwner.ValueRO.NetworkId == carNetworkId.Value)
                        {
                            state.Enabled = false;
                            state.EntityManager.CreateEntity(typeof(PlayersReadyRPC),
                                typeof(SendRpcCommandRequest));
                        }
                    }
                }
            }
        }
    }
}