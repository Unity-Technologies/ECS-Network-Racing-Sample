using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Instantiates a player skin entity prefab.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct SetPlayerSkinSystem : ISystem
    {
        private EntityQuery m_CarWithoutSkinQuery;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SkinElement>();
            m_CarWithoutSkinQuery = state.GetEntityQuery(ComponentType.Exclude<HasVisual>(), ComponentType.ReadOnly<LapProgress>());
            state.RequireForUpdate(m_CarWithoutSkinQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().
                                CreateCommandBuffer(state.WorldUnmanaged);
            var skinBuffer = GetSingletonBuffer<SkinElement>(true);
           
            foreach (var (skin, entity) in Query<RefRO<Skin>>()
                         .WithAll<Player>()
                         .WithAll<Rank>()
                         .WithAll<GhostOwner>().WithEntityAccess()
                         .WithNone<HasVisual>())
            {
                var visual = commandBuffer.Instantiate(skinBuffer[skin.ValueRO.Id].VisualEntity);
                commandBuffer.AddComponent(visual, new Parent { Value = entity });
                commandBuffer.AddComponent<HasVisual>(entity);
                commandBuffer.AddComponent(entity,
                    new Skin
                    {
                        Id = skin.ValueRO.Id, 
                        NeedUpdate = true, 
                        VisualCar = visual
                    });
            }
        }
    }

    public struct HideVisualSkinWheelsJob : IJobParallelFor
    {
        public NativeArray<Entity> Wheels;
        [ReadOnly]
        public ComponentLookup<LocalToWorld> LocalToWorldLookup;   
        public EntityCommandBuffer.ParallelWriter ECB;
        public void Execute(int index)
        {
            var entity = Wheels[index];
            var localToWorld = LocalToWorldLookup[entity];
            ECB.SetComponent(index, entity, LocalTransform.FromPositionRotationScale(localToWorld.Position, localToWorld.Rotation, 0));
        }
    }

    /// <summary>
    /// Instantiate the entity prefab wheels for the skin.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct SetWheelVisualSystem : ISystem
    {
        private ComponentLookup<VisualWheels> m_VisualWheelsLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            m_VisualWheelsLookup = state.GetComponentLookup<VisualWheels>();
            state.RequireForUpdate<Skin>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_VisualWheelsLookup.Update(ref state);
            var commandBuffer = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().
                                CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (skin, entity) in Query<RefRW<Skin>>()
                         .WithAll<Player>()
                         .WithAll<Rank>()
                         .WithAll<GhostOwner>().WithEntityAccess())
            {
                if (!skin.ValueRO.NeedUpdate)
                {
                    continue;
                }

                if (m_VisualWheelsLookup.TryGetComponent(skin.ValueRO.VisualCar, out var visual))
                {
                    skin.ValueRW.NeedUpdate = false;
                }
            }
            
            var wheels = new NativeList<Entity>(Allocator.Temp);
            foreach (var (visualWheels, entity) in Query<RefRO<VisualWheels>>().WithNone<HasVisual>().WithEntityAccess())
            {
                commandBuffer.AddComponent<HasVisual>(entity);
                var visuals = visualWheels.ValueRO;
                wheels.Add(visuals.WheelFL);
                wheels.Add(visuals.WheelFR);
                wheels.Add(visuals.WheelRL);
                wheels.Add(visuals.WheelRR);
            }
            
            if (!wheels.IsEmpty)
            {
                UnityEngine.Debug.Log($"The number of vehicles this frame:{wheels.Length} has been updated.");
                
                // The skin visuals should be hidden because they’re already present in the vehicle ghost.
                var hideVisualWheelsJob = new HideVisualSkinWheelsJob
                {
                    Wheels = wheels.ToArray(Allocator.TempJob),
                    LocalToWorldLookup = GetComponentLookup<LocalToWorld>(true),
                    ECB = commandBuffer.AsParallelWriter()
                };

                state.Dependency = hideVisualWheelsJob.Schedule(wheels.Length, 64, state.Dependency);
            }
        }
    }
}