using Unity.Entities;
using Unity.Entities.Racing.Common;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    [UpdateAfter(typeof(TransformSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class UpdateCameraTargetSystem : SystemBase
    {
        private Transform m_CameraTarget;

        protected override void OnCreate()
        {
            RequireForUpdate<LocalUser>();
        }

        protected override void OnStartRunning()
        {
            m_CameraTarget = GameObject.FindWithTag("CameraTarget").transform;
            base.OnStartRunning();
        }

        protected override void OnUpdate()
        {
            foreach (var player in Query<PlayerAspect>().WithAll<LocalUser>())
            {
                if (!player.HasValidPosition())
                {
                    return;
                }

                m_CameraTarget.position = player.LocalToWorld.Position;
                m_CameraTarget.rotation = player.LocalToWorld.Rotation;
            }
        }
    }
}