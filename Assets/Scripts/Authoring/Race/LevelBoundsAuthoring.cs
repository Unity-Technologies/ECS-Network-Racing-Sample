using Unity.Entities.Racing.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    public class LevelBoundsAuthoring : MonoBehaviour
    {
        public Bounds Bounds;

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
        }

        private class Baker : Baker<LevelBoundsAuthoring>
        {
            public override void Bake(LevelBoundsAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
                var bounds = new AABB
                {
                    Center = authoring.Bounds.center,
                    Extents = authoring.Bounds.extents
                };
                AddComponent(entity, new LevelBounds() { Value = bounds });
            }
        }
    }
}