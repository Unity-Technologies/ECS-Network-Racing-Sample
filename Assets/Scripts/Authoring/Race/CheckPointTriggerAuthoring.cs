using Unity.Entities.Racing.Common;
using Unity.Entities;
using UnityEngine;

namespace Dots.Racing
{
    public class CheckPointTriggerAuthoring : MonoBehaviour
    {
        public int Id = 1;
    }

    public class CheckPointBaker : Baker<CheckPointTriggerAuthoring>
    {
        public override void Bake(CheckPointTriggerAuthoring authoring)
        {
            AddComponent(new CheckPoint {Id = authoring.Id});
        }
    }
}