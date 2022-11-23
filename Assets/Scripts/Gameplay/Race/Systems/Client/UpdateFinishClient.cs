using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Entities;

namespace Dots.Racing
{
    /// <summary>
    /// Update Finish Timer UI, so the players know the time left in the race
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateFinishTimer : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalUser>();
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            SystemAPI.TryGetSingletonEntity<LocalUser>(out var localUser);
            var playerState = state.EntityManager.GetComponentData<Player>(localUser).State;
            var race = SystemAPI.GetSingleton<Race>();

            if (playerState is not (PlayerState.Race or PlayerState.Finished) || race.State is not RaceState.Finished) 
                return;
            
            var currentTimer = race.CurrentTimer;
            HUDController.Instance.ShowFinishCounter((int)currentTimer);
        }
    }
}