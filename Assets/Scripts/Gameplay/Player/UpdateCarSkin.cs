using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
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
            state.RequireForUpdate<SkinElement>();
            m_CarWithoutSkinQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(LapProgress)
                },
                None = new ComponentType[]
                {
                    typeof(HasVisual)
                }
            });
            state.RequireForUpdate(m_CarWithoutSkinQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            var skinBuffer = GetSingletonBuffer<SkinElement>(true);

            foreach (var (skin, entity) in Query<RefRO<Skin>>()
                         .WithAll<Player>()
                         .WithAll<Rank>()
                         .WithAll<GhostOwner>().WithEntityAccess()
                         .WithNone<HasVisual>())
            {
                var visual = commandBuffer.Instantiate(skinBuffer[skin.ValueRO.Id].VisualEntity);
                commandBuffer.AddComponent(visual, new Parent { Value = entity });
                commandBuffer.AddComponent(entity,
                    new Skin { Id = skin.ValueRO.Id, NeedUpdate = true, VisualCar = visual });
                commandBuffer.AddComponent<HasVisual>(entity);
            }

            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
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
            m_VisualWheelsLookup = state.GetComponentLookup<VisualWheels>();
            state.RequireForUpdate<Skin>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_VisualWheelsLookup.Update(ref state);
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (skin, entity) in Query<RefRW<Skin>>()
                         .WithAll<Player>()
                         .WithAll<Rank>()
                         .WithAll<GhostOwner>().WithEntityAccess())
            {
                if (!skin.ValueRO.NeedUpdate)
                {
                    return;
                }

                if (m_VisualWheelsLookup.TryGetComponent(skin.ValueRO.VisualCar, out var visual))
                {
                    skin.ValueRW.NeedUpdate = false;

                    foreach (var (wheel, chassisRef) 
                             in Query<RefRW<Wheel>,RefRO<ChassisReference>>())
                    {
                        if (chassisRef.ValueRO.Value == entity)
                        {
                            switch (wheel.ValueRO.Placement)
                            {
                                case WheelPlacement.FrontRight:
                                    wheel.ValueRW.VisualMesh = visual.WheelFR;
                                    break;
                                case WheelPlacement.FrontLeft:
                                    wheel.ValueRW.VisualMesh = visual.WheelFL;
                                    break;
                                case WheelPlacement.RearRight:
                                    wheel.ValueRW.VisualMesh = visual.WheelRR;
                                    break;
                                case WheelPlacement.RearLeft:
                                    wheel.ValueRW.VisualMesh = visual.WheelRL;
                                    break;
                            }
                        }
                    }
                }
            }

            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }
    }
}