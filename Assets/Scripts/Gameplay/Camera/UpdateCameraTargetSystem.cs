using Unity.Entities.Racing.Common;
using Unity.Mathematics;
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
            foreach (var (transform, localToWorld) 
                     in Query<RefRO<LocalTransform>, RefRO<LocalToWorld>>().WithAll<LocalUser>())
            {
                var isFinite = math.isfinite(transform.ValueRO.Position);
                var hasValidPosition = isFinite.x && isFinite.y && isFinite.z;
                if (!hasValidPosition)
                {
                    return;
                }
                m_CameraTarget.position = localToWorld.ValueRO.Position;
                m_CameraTarget.rotation = localToWorld.ValueRO.Rotation;
            }
        }
    }
}