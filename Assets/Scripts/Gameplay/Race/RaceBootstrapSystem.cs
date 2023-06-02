using Unity.Burst;
using Unity.Entities.Racing.Authoring;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct RaceBootstrapSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RaceBootstrap>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            var race = GetSingleton<RaceBootstrap>();
            state.EntityManager.Instantiate(race.RacePrefab);
        }
    }
}  