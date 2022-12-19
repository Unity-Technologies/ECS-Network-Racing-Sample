using Unity.Entities;
using Unity.Entities.Racing.Common;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Takes the main camera and place it looking at the player
    /// </summary>
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
            foreach (var localPlayer in Query<LocalPlayerAspect>())
            {
                if (!localPlayer.HasValidPosition())
                {
                    return;
                }
                m_CameraTarget.position = localPlayer.LocalToWorld.Position;
                m_CameraTarget.rotation = localPlayer.LocalToWorld.Rotation;
            }
        }
    }
}