using Unity.Entities;
using Unity.Entities.Racing.Common;
using Unity.NetCode;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct LoadRaceAutomatically : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkIdComponent>();
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();
            if (race.State is RaceState.StartingRaceAutomatically)
            {
                if (TryGetSingleton<NetworkIdComponent>(out var carNetworkId))
                {
                    foreach (var car in Query<PlayerAspect>())
                    {
                        if (car.NetworkId == carNetworkId.Value)
                        {
                            state.Enabled = false;
                            state.EntityManager.CreateEntity(typeof(PlayersReadyRPC),
                                typeof(SendRpcCommandRequestComponent));
                        }
                    }
                }
            }
        }
    }
}