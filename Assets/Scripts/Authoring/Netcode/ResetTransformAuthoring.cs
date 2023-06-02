using Unity.Entities.Racing.Common;
using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    public class ResetTransformAuthoring : MonoBehaviour
    {
        private class Baker : Baker<ResetTransformAuthoring>
        {
            public override void Bake(ResetTransformAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.WorldSpace);
                var transform = authoring.transform;
                AddComponent(entity, new ResetTransform
                {
                    Translation = transform.localPosition,
                    Rotation = transform.rotation
                });
            }
        }
    }
}