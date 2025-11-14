using Unity.Entities.Racing.Common;
using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    public class VehiclePlayerAuthoring : MonoBehaviour
    {
        [Header("Engine Sound")] 
        [SerializeField, Range(0, 1)]
        private float MinAudioVolume = 0.4f;
        [SerializeField, Range(0, 1)]
        private float MaxAudioVolume = 1.0f;

        private class Baker : Baker<VehiclePlayerAuthoring>
        {
            public override void Bake(VehiclePlayerAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent<LapProgress>(entity);
                AddComponent<Skin>(entity);
                AddComponent<Player>(entity);
                AddComponent<PlayerName>(entity);
                AddComponent<Rank>(entity);
                AddComponent<Reset>(entity);
                AddComponent<CarInput>(entity);

                AddComponent(entity, new VolumeData
                {
                    Min = authoring.MinAudioVolume,
                    Max = authoring.MaxAudioVolume,
                });
            }
        }
    }
}
