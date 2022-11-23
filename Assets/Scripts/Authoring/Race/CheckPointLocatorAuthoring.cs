using Dots.Racing;
using Unity.Entities.Racing.Common;
using UnityEngine;
using Unity.Entities;

namespace Dots.Racing
{
    public class CheckPointLocatorAuthoring : MonoBehaviour { }

    public class CheckPointLocatorBaker : Baker<CheckPointLocatorAuthoring>
    {
        public override void Bake(CheckPointLocatorAuthoring authoring)
        {
            AddComponent(new CheckPointLocator());
        }
    }
}