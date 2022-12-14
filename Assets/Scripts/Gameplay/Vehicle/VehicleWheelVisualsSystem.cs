using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using Unity.Transforms;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Read the wheels data and apply it to the mesh renderer of the wheel
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct UpdateWheelVisualsJob : IJobEntity
    {
        [NativeDisableContainerSafetyRestriction]
        public ComponentLookup<LocalTransform> LocalTransformLookup;

        private void Execute(ref Wheel wheel, in Suspension suspension, in TransformAspect transform,
            in WheelHitData wheelHitData)
        {
            if (!LocalTransformLookup.HasComponent(wheel.VisualMesh))
            {
                return;
            }

            var springLength = wheelHitData.HasHit ? suspension.SpringLength : suspension.RestLength;
            // FIXME: transform.Up is not correct at this point, use chassis local up
            var position = transform.LocalPosition - springLength * transform.Up;
            wheel.RotationAngle += wheel.DriveForce;
            wheel.RotationAngle %= 360;
            var rotation = math.mul(
                quaternion.AxisAngle(math.up(), wheel.SteeringAngle),
                quaternion.AxisAngle(math.right(), math.radians(wheel.RotationAngle)));
            LocalTransformLookup[wheel.VisualMesh] = new LocalTransform
            {
                Position = position,
                Rotation = rotation,
                Scale = 1
            };
        }
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial struct UpdateWheelVisualsSystem : ISystem
    {
        private ComponentLookup<LocalTransform> m_LocalTransformLookup;

        public void OnCreate(ref SystemState state)
        {
            m_LocalTransformLookup = state.GetComponentLookup<LocalTransform>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_LocalTransformLookup.Update(ref state);
            var updateWheelVisualsJob = new UpdateWheelVisualsJob
            {
                LocalTransformLookup = m_LocalTransformLookup
            };
            state.Dependency = updateWheelVisualsJob.ScheduleParallel(state.Dependency);
        }
    }
}