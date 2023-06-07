using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Updates Checkpoint visual position according to the local player's position
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateCheckPointPositionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
            state.RequireForUpdate<CheckPointLocator>();
            state.RequireForUpdate<LocalUser>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();
            
            if (race.CanJoinRace)
                return;

            if (race.HasFinished)
            {
                foreach (var checkPointLocator in Query<CheckPointLocatorAspect>())
                {
                    checkPointLocator.SetLocalTransform(checkPointLocator.GetResetPosition, quaternion.identity);
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
                    if (checkPoint.CheckPointId == player.LapProgress.NextPointId)
                    {
                        currentPosition = checkPoint.LocalPosition;
                        currentRotation = checkPoint.LocalRotation;
                        break;
                    }
                }

                foreach (var checkPointLocator in Query<CheckPointLocatorAspect>())
                {
                    checkPointLocator.SetLocalTransform(currentPosition, currentRotation);
                }
            }
        }
    }
}