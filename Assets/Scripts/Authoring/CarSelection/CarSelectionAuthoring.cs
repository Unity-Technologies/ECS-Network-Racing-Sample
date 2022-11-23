using System;
using Unity.Entities;
using Unity.Entities.Racing.Common;
using UnityEngine;


namespace Dots.Racing
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
    }

    public class CarSelectionBaker : Baker<CarSelectionAuthoring>
    {
        public override void Bake(CarSelectionAuthoring authoring)
        {
            AddComponent(new CarSelection
            {
                SkinPosition = authoring.SkinPosition.position,
                SkinRotation = authoring.SkinPosition.rotation
            });
            
            AddComponent<CarSelectionUpdate>();
            
            var skinBuffer = AddBuffer<CarSelectionSkinData>();
            foreach (var carSkin in authoring.CarSkins)
            {
                skinBuffer.Add(new CarSelectionSkinData {Prefab = GetEntity(carSkin.Prefab)});
            }
        }
    }
}