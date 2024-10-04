using Unity.Burst;
using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Update the Player Audio Sources position and volume
    /// </summary>
    [UpdateAfter(typeof(UpdateCameraTargetSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateAudioSourceSystem : ISystem
    {
        public void OnDestroy(ref SystemState state)
        {
            foreach (var (car, entity) in Query<RefRO<Player>>()
                         .WithAll<AudioSourceTag>()
                         .WithAll<GhostOwner>()
                         .WithAll<Player>()
                         .WithAll<Rank>().WithEntityAccess())
            {
                PlayerAudioManager.Instance.DeleteAudioSource(entity);
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            if (PlayerAudioManager.Instance == null)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            foreach (var (car, entity) in Query<RefRO<Player>>()
                         .WithAll<GhostOwner>()
                         .WithAll<Player>()
                         .WithAll<Rank>().WithEntityAccess()
                         .WithNone<AudioSourceTag>())
            {
                var isLocalUser = state.EntityManager.HasComponent<LocalUser>(entity);
                PlayerAudioManager.Instance.AddAudioSource(entity, isLocalUser);
                ecb.AddComponent<AudioSourceTag>(entity);
            }

            foreach (var (localToWorld, volumeData, velocity, entity) in 
                     Query<RefRO<LocalToWorld>, RefRO<VolumeData>,RefRO<PhysicsVelocity>>().WithEntityAccess())
            {
                var audioVolumeRange = new float2(volumeData.ValueRO.Min, volumeData.ValueRO.Max);
                var linearVelocityMagnitude = math.length(velocity.ValueRO.Linear);
                var audio = math.min(audioVolumeRange.y, audioVolumeRange.x + (linearVelocityMagnitude / 100f));
                
                PlayerAudioManager.Instance.UpdatePitchAndVolume(entity, audio);
                PlayerAudioManager.Instance.UpdatePosition(entity, localToWorld.ValueRO.Position,
                    state.EntityManager);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Update music clips
    /// </summary>
    [UpdateAfter(typeof(UpdateCameraTargetSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateMusicSourceSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (PlayerAudioManager.Instance == null)
            {
                return;
            }

            PlayerAudioManager.Instance.CreateAndPlayMusicAudioSourceOnce();

            if (!TryGetSingleton<Race>(out var race))
                return;

            if (race.NotStarted) 
            {
                PlayerAudioManager.Instance.PlayLobbyMusic();
            }
            else if (race.IsRaceStarting)
            {
                PlayerAudioManager.Instance.PlayRaceMusic();
            }
            else if (race.IsInProgress)
            {
                foreach (var lapProgress in Query<RefRO<LapProgress>>().WithAll<LocalUser>())
                {
                    if (lapProgress.ValueRO.HasArrived)
                    {
                        PlayerAudioManager.Instance.PlayCelebrationMusic();
                    }
                }
            }
            else if (race.HasFinished) 
            {
                PlayerAudioManager.Instance.PlayCelebrationMusic();
            }
        }
            
    }

    /// <summary>
    /// Update the Player Audio Sources position and volume
    /// </summary>
    [UpdateAfter(typeof(UpdateCameraTargetSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateUIAudioSourceSystem : ISystem
    {
        public void OnDestroy(ref SystemState state)
        {
            foreach (var (car, entity) in Query<RefRO<Player>>()
                         .WithAll<AudioSourceTag>()
                         .WithAll<GhostOwner>()
                         .WithAll<Player>()
                         .WithAll<Rank>().WithEntityAccess())
            {
                PlayerAudioManager.Instance.DeleteAudioSource(entity);
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            if (PlayerAudioManager.Instance == null)
            {
                return;
            }

            PlayerAudioManager.Instance.CreateUIAudioSource();
        }
    }
}