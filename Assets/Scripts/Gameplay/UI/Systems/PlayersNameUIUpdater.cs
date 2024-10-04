using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.NetCode;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Creates player name entity.
    /// Updates player name's position 
    /// according to the player's position.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    [UpdateAfter(typeof(LocalUserAssignmentSystem))]
    public partial struct PlayersNameUIUpdater : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (PlayerInfoController.Instance == null)
                return;

            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            foreach (var (playerName, entity) in Query<RefRO<PlayerName>>()
                         .WithNone<PlayerNameTag, LocalUser>().WithEntityAccess())
            {
                var name = playerName.ValueRO.Name.ToString();
                PlayerInfoController.Instance.CreateNameTag(name, entity);
                ecb.AddComponent<PlayerNameTag>(entity);
            }

            foreach (var (localToWorld, entity) in Query<RefRO<LocalToWorld>>()
                         .WithAll<Player>()
                         .WithAll<Rank>()
                         .WithAll<GhostOwner>().WithEntityAccess())
            {
                PlayerInfoController.Instance.UpdateNamePosition(entity, localToWorld.ValueRO.Position);
            }

            PlayerInfoController.Instance.RefreshNameTags(state.EntityManager);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}