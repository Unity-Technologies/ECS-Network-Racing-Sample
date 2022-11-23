using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using static  Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    /// <summary>
    /// Apply down force to the vehicle chases and scale it with speed to avoid losing controls
    /// https://en.wikipedia.org/wiki/Downforce
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct ChasesDownforceJob : IJobEntity
    {
        public PhysicsWorld PhysicsWorld;

        void Execute(Entity entity, in PhysicsMass mass, in VehicleChassis vehicleChassis, in TransformAspect transform)
        {
            var index = PhysicsWorld.GetRigidBodyIndex(entity);
            if (index == -1 || index >= PhysicsWorld.NumDynamicBodies)
                return;
            var filter = PhysicsWorld.GetCollisionFilter(index);
            filter.CollidesWith = (uint) vehicleChassis.CollisionMask;

            var start = transform.LocalPosition - mass.CenterOfMass * 5; // TODO: Expose as authoring field 
            var end = transform.LocalPosition - mass.CenterOfMass - transform.Up;
            if (!math.isfinite(end).x)
                return;
            var input = new RaycastInput
            {
                Start = start,
                End = end,
                Filter = filter
            };

            if (PhysicsWorld.CollisionWorld.CastRay(input, out var result))
            {
                return;
            }

            var downforce = vehicleChassis.DownForce * -transform.Up;
            PhysicsWorld.ApplyImpulse(index, downforce, transform.LocalPosition);
        }
    }
    
    [BurstCompile]
    [UpdateInGroup(typeof(PhysicsSimulationGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ChasesDownforceSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
            var downforceJob = new ChasesDownforceJob
            {
                PhysicsWorld = physicsWorld,
            };
            state.Dependency = downforceJob.Schedule(state.Dependency);
            state.Dependency.Complete();
        }
    }
}