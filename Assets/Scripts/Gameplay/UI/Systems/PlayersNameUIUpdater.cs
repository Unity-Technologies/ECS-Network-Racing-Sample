using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Collections;
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
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            if (PlayerInfoController.Instance == null)
                return;

            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            foreach (var player in Query<PlayerNameAspect>().WithNone<PlayerNameTag, LocalUser>())
            {
                var name = player.Name.ToString();
                PlayerInfoController.Instance.CreateNameTag(name, player.Self);
                ecb.AddComponent<PlayerNameTag>(player.Self);
            }

            foreach (var car in Query<PlayerAspect>())
            {
                PlayerInfoController.Instance.UpdateNamePosition(car.Self, car.LocalToWorld.Position);
            }

            PlayerInfoController.Instance.RefreshNameTags(state.EntityManager);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}