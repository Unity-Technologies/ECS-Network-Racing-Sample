using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Mathematics;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Updates Checkpoint visual position according to the local player's position
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateCheckPointPositionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
            state.RequireForUpdate<CheckPointLocator>();
            state.RequireForUpdate<LocalUser>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();
            if (race.CanJoinRace)
                return;
            
            if (race.HasFinished)
            {
                foreach (var checkPointLocator in Query<CheckPointLocatorAspect>())
                {
                    checkPointLocator.Transform.WorldPosition = checkPointLocator.GetResetPosition;
                    checkPointLocator.Transform.WorldRotation = quaternion.identity;
                }
            }
            if (!race.IsInProgress)
                return;
            foreach (var player in Query<LocalPlayerAspect>())
            {
                if (!player.Player.InRace) 
                    return;

                var currentPosition = float3.zero;
                var currentRotation = quaternion.identity;
                foreach (var checkPoint in Query<CheckPointAspect>())
                {
                    if (checkPoint.CheckPointId ==  player.LapProgress.NextPointId)
                    {
                        currentPosition = checkPoint.Transform.WorldPosition;
                        currentRotation = checkPoint.Transform.WorldRotation;
                        break;
                    }
                }

                foreach (var checkPointLocator in Query<CheckPointLocatorAspect>())
                {
                    checkPointLocator.Transform.WorldPosition = currentPosition;
                    checkPointLocator.Transform.WorldRotation = currentRotation;
                }
            }
        }
        
        public void OnDestroy(ref SystemState state) { }
    }
}