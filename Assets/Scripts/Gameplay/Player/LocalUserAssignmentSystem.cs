using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.NetCode;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Adds Local User component to filter it.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct LocalUserAssignmentSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var id = GetSingleton<NetworkId>();
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            foreach (var (ghostOwner, entity) in 
                     Query<RefRO<GhostOwner>>()
                         .WithAll<Player>()
                         .WithAll<Rank>()
                         .WithNone<LocalUser>().WithEntityAccess())
            {
                if (ghostOwner.ValueRO.NetworkId == id.Value)
                {
                    ecb.AddComponent<LocalUser>(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}