using UnityEngine;
using Unity.Entities.Racing.Common;

namespace Unity.Entities.Racing.Authoring
{
    public class AutoConnectAuthoring : MonoBehaviour
    {
        private class Baker : Baker<AutoConnectAuthoring>
        {
            public override void Bake(AutoConnectAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
                AddComponent<AutoConnect>(entity);
            }
        }
    }
}