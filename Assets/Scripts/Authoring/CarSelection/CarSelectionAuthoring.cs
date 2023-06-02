using System;
using Unity.Entities.Racing.Common;
using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    [Serializable]
    public struct CarSelectionSkin
    {
        public GameObject Prefab;
    }

    public class CarSelectionAuthoring : MonoBehaviour
    {
        [Header("Spawn Point")]
        public Transform SkinPosition;
        public CarSelectionSkin[] CarSkins;

        private class Baker : Baker<CarSelectionAuthoring>
        {
            public override void Bake(CarSelectionAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, new CarSelection
                {
                    SkinPosition = authoring.SkinPosition.position,
                    SkinRotation = authoring.SkinPosition.rotation
                });
            
                AddComponent<CarSelectionUpdate>(entity);
            
                var skinBuffer = AddBuffer<CarSelectionSkinData>(entity);
                foreach (var carSkin in authoring.CarSkins)
                {
                    skinBuffer.Add(new CarSelectionSkinData {Prefab = GetEntity(carSkin.Prefab, TransformUsageFlags.Renderable)});
                }
            }
        }
    }
}