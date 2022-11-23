using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Racing.Gameplay;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
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
            if (PlayerNamesController.Instance == null)
                return;

            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            foreach (var player in Query<PlayerNameAspect>().WithNone<PlayerNameTag, LocalUser>())
            {
                var name = player.Name.ToString();
                PlayerNamesController.Instance.CreateNameTag(name, player.Self);
                ecb.AddComponent<PlayerNameTag>(player.Self);
            }

            foreach (var car in Query<PlayerAspect>())
            {
                PlayerNamesController.Instance.UpdateNamePosition(car.Self, car.LocalToWorld.Position);
            }

            PlayerNamesController.Instance.RefreshNameTags(state.EntityManager);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}