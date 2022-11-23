using Unity.Entities.Racing.Common;
using Unity.Entities;
using UnityEngine;

namespace Dots.Racing
{
    public class CarInputAuthoring : MonoBehaviour { }
    
    public class CarInputBaker : Baker<CarInputAuthoring>
    {
        public override void Bake(CarInputAuthoring authoring)
        {
            AddComponent<CarInput>();
        }
    }
}