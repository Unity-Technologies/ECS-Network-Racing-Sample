using Unity.Entities.Racing.Common;
using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    public class CheckPointLocatorAuthoring : MonoBehaviour
    {
        private class Baker : Baker<CheckPointLocatorAuthoring>
        {
            public override void Bake(CheckPointLocatorAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                var checkPointLocator = new CheckPointLocator
                {
                    ResetPosition = authoring.transform.position
                };
                AddComponent(entity, checkPointLocator);
            }
        }
    }
}