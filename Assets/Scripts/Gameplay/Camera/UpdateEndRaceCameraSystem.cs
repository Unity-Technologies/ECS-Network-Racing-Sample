using Unity.Entities.Racing.Common;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Plays the camera transition ending 
    /// when the player has finished the race.
    /// </summary>
    [UpdateAfter(typeof(TransformSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class UpdateEndRaceCameraSystem : SystemBase
    {
        private Entity m_CurrentTarget;
        private Transform m_TargetTransform;

        protected override void OnCreate()
        {
            RequireForUpdate<Race>();
            RequireForUpdate<LocalUser>();
        }

        protected override void OnStartRunning()
        {
            var cinematicTarget = GameObject.FindWithTag("FinalCinematicTarget");
            if (cinematicTarget == null)
            {
                Enabled = false;
                return;
            }

            m_TargetTransform = cinematicTarget.transform;
            base.OnStartRunning();
        }

        protected override void OnUpdate()
        {
            var race = SystemAPI.GetSingleton<Race>();
            if (!race.IsFinishing)
            {
                return;
            }

            var playerHasFinished = false;
            foreach (var player in Query<LocalPlayerAspect>())
            {
                if (player.Player.HasFinished) 
                {
                    playerHasFinished = true;
                }
            }

            if (playerHasFinished)
            {
                // Search the next car in the Rank
                foreach (var car in Query<PlayerAspect>())
                {
                    if (car.Rank == race.PlayersFinished + 1)
                    {
                        m_CurrentTarget = car.Self;
                        break;
                    }
                }

                TimelineManager.Instance.SwitchToNearestCamera();
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