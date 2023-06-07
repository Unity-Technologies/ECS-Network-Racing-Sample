using Unity.Burst;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    ///     Calculate ackermann steering angles and rotate the steering wheels accordingly
    ///     https://en.wikipedia.org/wiki/Ackermann_steering_geometry
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct SteeringJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(in Steering steering, in WheelDriveControls wheelDriveControls,
            ref LocalTransform localTransform, ref Wheel wheel)
        {
            float steeringAngle;
            var steeringAmount = wheelDriveControls.SteerAmount;
            if (steeringAmount > 0)
            {
                var dir = wheel.Placement == WheelPlacement.FrontLeft ? 1 : -1;
                steeringAngle = steering.CalculateSteeringAngle(steeringAmount, dir);
            }
            else if (steeringAmount < 0)
            {
                var dir = wheel.Placement == WheelPlacement.FrontRight ? 1 : -1;
                steeringAngle = steering.CalculateSteeringAngle(steeringAmount, dir);
            }
            else
            {
                steeringAngle = 0.0f;
            }

            wheel.SteeringAngle = steeringAngle;

            var targetRotation = quaternion.AxisAngle(math.up(), steeringAngle);
            localTransform.Rotation = math.slerp(localTransform.Rotation, targetRotation,
                steering.SteeringTime * DeltaTime);
        }
    }

    [UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
    [UpdateAfter(typeof(ProcessVehicleWheelsInputSystem))]
    public partial struct WheelSteeringSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var steeringJob = new SteeringJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            state.Dependency = steeringJob.ScheduleParallel(state.Dependency);
        }
    }
}