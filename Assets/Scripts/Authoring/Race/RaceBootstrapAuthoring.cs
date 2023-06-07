using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    public class RaceBootstrapAuthoring : MonoBehaviour
    {
        public GameObject RacePrefab;
        
        private class Baker : Baker<RaceBootstrapAuthoring>
        {
            public override void Bake(RaceBootstrapAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
                AddComponent(entity, new RaceBootstrap
                {
                    RacePrefab = GetEntity(authoring.RacePrefab, TransformUsageFlags.None)
                });
            }
        }
    }

    public struct RaceBootstrap : IComponentData
    {
        public Entity RacePrefab;
    }
}