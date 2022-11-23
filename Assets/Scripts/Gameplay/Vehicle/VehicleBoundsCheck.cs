using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct ResetTransformJob : IJobEntity
    {
        [ReadOnly] public AABB Bounds;

        void Execute(in ResetTransform resetTransform, ref TransformAspect transformAspect)
        {
            if (!Bounds.Contains(transformAspect.WorldPosition))
            {
                transformAspect.WorldPosition = resetTransform.Translation;
                transformAspect.WorldRotation = resetTransform.Rotation;
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct VehicleFlipChecker : IJobEntity
    {
        void Execute(ref PlayerAspect playerAspect)
        {
            var distanceSq = math.distancesq(playerAspect.LocalToWorld.Up, math.up());
            
            // Flip the vehicle if it's up side down
            if (distanceSq > 3.9f)
            {
                var targetRotation  = math.mul(playerAspect.LocalToWorld.Rotation, quaternion.RotateZ(math.PI));
                playerAspect.SetTargetTransform(playerAspect.LocalToWorld.Position, targetRotation);
                playerAspect.ResetVehicle();
            }
        }
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct LevelBoundsChecker : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LevelBounds>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var levelBounds = GetSingleton<LevelBounds>();
            foreach (var player in Query<PlayerAspect>())
            {
                if (!levelBounds.Value.Contains(player.LocalToWorld.Position))
                {
                    player.ResetVehicle();
                }
            }
            var resetTransformJob = new ResetTransformJob
            {
                Bounds = levelBounds.Value
            };
            var carFlipChecker = new VehicleFlipChecker { };
            state.Dependency = carFlipChecker.ScheduleParallel(state.Dependency);
            state.Dependency = resetTransformJob.ScheduleParallel(state.Dependency);
        }
    }
}