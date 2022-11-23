using Unity.Entities.Racing.Common;
using Unity.Entities;
using UnityEngine;

namespace Dots.Racing
{
    public class ResetTransformAuthoring : MonoBehaviour { }

    public class ResetTransformBaker : Baker<ResetTransformAuthoring>
    {
        public override void Bake(ResetTransformAuthoring authoring)
        {
            var transform = authoring.transform;
            AddComponent(new ResetTransform
            {
                Translation = transform.localPosition,
                Rotation = transform.rotation
            });
        }
    }
}