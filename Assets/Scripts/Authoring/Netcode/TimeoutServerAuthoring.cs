using Unity.Entities.Racing.Common;
using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    public class TimeoutServerAuthoring : MonoBehaviour
    {
        public float TimeOutSeconds = 10f;

        private class Baker : Baker<TimeoutServerAuthoring>
        {
            public override void Bake(TimeoutServerAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
                AddComponent(entity, new TimeOutServer { Value = authoring.TimeOutSeconds });
            }
        }
    }
}