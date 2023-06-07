using Unity.Entities.Racing.Common;
using Unity.Burst;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Update Finish Timer UI, so the players know the time left in the race
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateFinishUITimer : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalUser>();
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var race = SystemAPI.GetSingleton<Race>();
            if (!race.IsFinishing)
                return;

            foreach (var localPlayer in Query<LocalPlayerAspect>())
            {
                if (localPlayer.Player.InRace || localPlayer.Player.IsCelebrating || localPlayer.Player.HasFinished)
                {
                    var currentTimer = (int)race.CurrentTimer;
                    if (HUDController.Instance != null)
                    {
                        HUDController.Instance.ShowFinishCounter(currentTimer);
                    }
                }
            }
        }
    }
}