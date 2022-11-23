using Unity.Entities.Racing.Common;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Dots.Racing
{
    public class LevelBoundsAuthoring : MonoBehaviour
    {
        public Bounds Bounds;

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
        }
    }

    public class LevelBoundsBaker : Baker<LevelBoundsAuthoring>
    {
        public override void Bake(LevelBoundsAuthoring authoring)
        {
            var bounds = new AABB
            {
                Center = authoring.Bounds.center,
                Extents = authoring.Bounds.extents
            };
            AddComponent(new LevelBounds(){Value = bounds});
        }
    }

}