using Unity.Entities.Racing.Common;
using Unity.Entities;
using UnityEngine;

namespace Dots.Racing
{
    public class TimeoutServerAuthoring : MonoBehaviour 
    {
        public float TimeOutSeconds = 10f;
    }

    public class TimeoutServerBaker : Baker<TimeoutServerAuthoring>
    {
        public override void Bake(TimeoutServerAuthoring authoring)
        {
            var timeoutServer = new TimeOutServer { Value = authoring.TimeOutSeconds };
            AddComponent(timeoutServer);
        }
    }
}