using System.Runtime.InteropServices;
using Unity.Entities.Racing.Common;
using Unity.NetCode;

namespace Unity.Entities.Racing.Gameplay
{
    [UpdateInGroup(typeof(GhostInputSystemGroup), OrderFirst = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class InputClientInitializerSystem : SystemBase
    {
        private GameInput m_GameInput;
        private GCHandle m_Handle;

        protected override void OnCreate()
        {
            RequireForUpdate(GetEntityQuery(
                ComponentType.ReadOnly<CarInput>(),
                ComponentType.ReadOnly<GhostOwnerIsLocal>(),
                ComponentType.Exclude<PlayerGameInput>()));

            m_GameInput = new GameInput();
            m_GameInput.Enable();
            m_GameInput.Vehicle.Enable();
            m_Handle = GCHandle.Alloc(m_GameInput, GCHandleType.Pinned);
        }

        protected override void OnUpdate()
        {
            var cmd = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                        .CreateCommandBuffer(World.Unmanaged);

            foreach (var (input, entity) in SystemAPI
                            .Query<RefRO<CarInput>>()
                            .WithEntityAccess()
                            .WithNone<PlayerGameInput>())
            {
                cmd.AddComponent(entity, new PlayerGameInput
                {
                    Value = GCHandle.ToIntPtr(m_Handle)
                });
            }
        }

        protected override void OnDestroy()
        {
            if (m_Handle.IsAllocated)
                m_Handle.Free();
        }
    }
}