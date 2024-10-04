using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
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
                foreach (var (transform, checkPointLocator) 
                         in Query<RefRW<LocalTransform>, RefRO<CheckPointLocator>>())
                {
                    transform.ValueRW.Rotation = quaternion.identity;
                    transform.ValueRW.Position = checkPointLocator.ValueRO.ResetPosition;
                }
            }

            if (!race.IsInProgress)
                return;
            
            foreach (var (player, lapProgress) 
                     in Query<RefRO<Player>, RefRO<LapProgress>>().WithAll<LocalUser>())
            {
                if (!player.ValueRO.InRace)
                    return;

                var currentPosition = float3.zero;
                var currentRotation = quaternion.identity;
                foreach (var (checkPoint, localTransform) in 
                         Query<RefRO<CheckPoint>, RefRO<LocalTransform>>())
                {
                    if (checkPoint.ValueRO.Id == lapProgress.ValueRO.NextPointId)
                    {
                        currentPosition = localTransform.ValueRO.Position;
                        currentRotation = localTransform.ValueRO.Rotation;
                        break;
                    }
                }

                foreach (var transform in Query<RefRW<LocalTransform>>().WithAll<CheckPointLocator>())
                {
                    transform.ValueRW.Rotation = currentRotation;
                    transform.ValueRW.Position = currentPosition;
                }
            }
        }
    }
}