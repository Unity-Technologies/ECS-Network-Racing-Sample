using Unity.Entities.Racing.Common;
using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    public class CarInputAuthoring : MonoBehaviour
    {
        private class Baker : Baker<CarInputAuthoring>
        {
            public override void Bake(CarInputAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
                AddComponent<CarInput>(entity);
            }
        }
    }
}