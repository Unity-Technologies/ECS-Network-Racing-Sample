using Unity.Entities;
using UnityEngine;
using Unity.Entities.Racing.Common;

namespace Dots.Racing
{
    public class AutoConnectAuthoring : MonoBehaviour { }

    public class AutoConnectBaker : Baker<AutoConnectAuthoring>
    {
        public override void Bake(AutoConnectAuthoring authoring)
        {
            AddComponent<AutoConnect>();
        }
    }
}