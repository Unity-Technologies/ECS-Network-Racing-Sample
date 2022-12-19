using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Instantiates a player skin entity prefab.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    [BurstCompile]
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

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            var skinBuffer = GetSingletonBuffer<SkinElement>(true);

            foreach (var player in Query<PlayerAspect>().WithNone<HasVisual>())
            {
                var visual = commandBuffer.Instantiate(skinBuffer[player.Skin.Id].VisualEntity);
                commandBuffer.AddComponent(visual, new Parent { Value = player.Self });
                commandBuffer.AddComponent(player.Self,
                    new Skin { Id = player.Skin.Id, NeedUpdate = true, VisualCar = visual });
                commandBuffer.AddComponent<HasVisual>(player.Self);
            }

            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }
    }

    /// <summary>
    /// Instantiate the entity prefab wheels for the skin.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    [BurstCompile]
    public partial struct SetWheelVisualSystem : ISystem
    {
        private ComponentLookup<VisualWheels> m_VisualWheelsLookup;

        public void OnCreate(ref SystemState state)
        {
            m_VisualWheelsLookup = state.GetComponentLookup<VisualWheels>();
            state.RequireForUpdate<Skin>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            m_VisualWheelsLookup.Update(ref state);
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

            foreach (var player in Query<PlayerAspect>())
            {
                if (!player.Skin.NeedUpdate)
                {
                    return;
                }

                if (m_VisualWheelsLookup.TryGetComponent(player.Skin.VisualCar, out var visual))
                {
                    player.UpdateSkin();

                    foreach (var wheel in Query<WheelAspect>())
                    {
                        if (wheel.ChassisReference == player.Self)
                        {
                            switch (wheel.Wheel.Placement)
                            {
                                case WheelPlacement.FrontRight:
                                    wheel.SetVisualMesh(visual.WheelFR);
                                    break;
                                case WheelPlacement.FrontLeft:
                                    wheel.SetVisualMesh(visual.WheelFL);
                                    break;
                                case WheelPlacement.RearRight:
                                    wheel.SetVisualMesh(visual.WheelRR);
                                    break;
                                case WheelPlacement.RearLeft:
                                    wheel.SetVisualMesh(visual.WheelRL);
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