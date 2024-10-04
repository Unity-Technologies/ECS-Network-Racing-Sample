using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Checks if the player goes beyond the limits.
    /// Checks if the player is upside down.
    /// Resets the player in both cases.
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct ResetTransformJob : IJobEntity
    {
        [ReadOnly] public AABB Bounds;

        private void Execute(in ResetTransform resetTransform, ref LocalTransform localTransform)
        {
            if (!Bounds.Contains(localTransform.Position))
            {
                localTransform.Position = resetTransform.Translation;
                localTransform.Rotation = resetTransform.Rotation;
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    [WithAll(typeof(GhostOwner))]
    [WithAll(typeof(Player))]
    [WithAll(typeof(Rank))]
    public partial struct VehicleFlipChecker : IJobEntity
    {
        private void Execute(in LocalToWorld localToWorld, ref Reset reset)
        {
            var distanceSq = math.distancesq(localToWorld.Up, math.up());

            // Flip the vehicle if it's up side down
            if (distanceSq > 3.9f)
            {
                var targetRotation = math.mul(localToWorld.Rotation, quaternion.RotateZ(math.PI));
                reset.SetTargetTransform(localToWorld.Position, targetRotation);
                reset.ResetVehicle();
            }
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct CheckLevelBoundsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LevelBounds>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var levelBounds = GetSingleton<LevelBounds>();
            foreach (var (localToWorld, reset) 
                     in Query<RefRO<LocalToWorld>, RefRW<Reset>>()
                         .WithAll<GhostOwner>()
                         .WithAll<Player>()
                         .WithAll<Rank>())
            {
                if (!levelBounds.Value.Contains(localToWorld.ValueRO.Position))
                {
                    reset.ValueRW.ResetVehicle();
                }
            }

            var resetTransformJob = new ResetTransformJob
            {
                Bounds = levelBounds.Value
            };
            var carFlipChecker = new VehicleFlipChecker();
            state.Dependency = carFlipChecker.Schedule(state.Dependency);
            state.Dependency = resetTransformJob.Schedule(state.Dependency);
        }
    }
}