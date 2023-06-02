using Unity.Entities.Racing.Common;
using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    public class CheckPointTriggerAuthoring : MonoBehaviour
    {
        public int Id = 1;

        private class Baker : Baker<CheckPointTriggerAuthoring>
        {
            public override void Bake(CheckPointTriggerAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, new CheckPoint { Id = authoring.Id });
            }
        }
    }
}