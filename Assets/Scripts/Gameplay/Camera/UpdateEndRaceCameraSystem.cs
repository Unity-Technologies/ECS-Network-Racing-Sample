using Unity.Entities.Racing.Common;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    [UpdateAfter(typeof(TransformSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class UpdateEndRaceCameraSystem : SystemBase
    {
        private Entity m_CurrentTarget;

        //   private const float ChangeCameraTime = 2f;
        private float m_CurrentTimer;
        private Transform m_TargetTransform;

        protected override void OnCreate()
        {
            RequireForUpdate<Race>();
            RequireForUpdate<LocalUser>();
        }

        protected override void OnStartRunning()
        {
            var race = SystemAPI.GetSingleton<Race>();
            var cinematicTarget = GameObject.FindWithTag("FinalCinematicTarget");
            if (cinematicTarget == null)
            {
                Enabled = false;
                return;
            }

            m_TargetTransform = cinematicTarget.transform;
            m_CurrentTimer = race.WaitToShowLeaderboardTimer;
            base.OnStartRunning();
        }

        protected override void OnUpdate()
        {
            var race = SystemAPI.GetSingleton<Race>();
            if (race.State is not RaceState.Finished)
            {
                return;
            }

            m_CurrentTimer -= SystemAPI.Time.DeltaTime;
            if (m_CurrentTimer <= 0)
            {
                // Search the next car in the Rank
                foreach (var car in Query<PlayerAspect>())
                {
                    if (car.Rank == race.PlayersFinished + 1)
                    {
                        m_CurrentTarget = car.Self;
                    }
                }

                foreach (var car in Query<PlayerAspect>().WithAll<LocalUser>())
                {
                    if (car.LapProgress.Finished)
                    {
                        // Switch to the nearest camera
                        TimelineManager.Instance.SwitchToNearestCamera();
                    }
                }

                // Restart Timer
                m_CurrentTimer = race.WaitToShowLeaderboardTimer;
            }

            if (!EntityManager.Exists(m_CurrentTarget))
            {
                return;
            }

            var targetPosition = EntityManager.GetComponentData<LocalTransform>(m_CurrentTarget);
            m_TargetTransform.position = targetPosition.Position;
        }
    }
}