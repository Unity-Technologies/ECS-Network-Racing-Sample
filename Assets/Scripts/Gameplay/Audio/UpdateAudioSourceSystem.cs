using Dots.Racing;
using Unity.Burst;
using Unity.Entities.Racing.Common;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    ///     Update the Player Audio Sources
    /// </summary>
    [BurstCompile]
    [UpdateAfter(typeof(UpdateCameraTargetSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateAudioSourceSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
            foreach (var car in Query<PlayerAspect>().WithAll<AudioSourceTag>())
            {
                PlayerAudioManager.Instance.DeleteAudioSource(car.Self);
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            if (PlayerAudioManager.Instance == null)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            foreach (var car in Query<PlayerAspect>().WithNone<AudioSourceTag>())
            {
                var isLocalUser = state.EntityManager.HasComponent<LocalUser>(car.Self);
                PlayerAudioManager.Instance.AddAudioSource(car.Self, isLocalUser);
                ecb.AddComponent<AudioSourceTag>(car.Self);
            }

            foreach (var audio in Query<AudioAspect>())
            {
                PlayerAudioManager.Instance.UpdatePitchAndVolume(audio.Self, audio.GetVolumeRange());
                PlayerAudioManager.Instance.UpdatePosition(audio.Self, audio.LocalToWorld.Position,
                    state.EntityManager);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}