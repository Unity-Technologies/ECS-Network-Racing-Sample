using Unity.Entities.Racing.Common;
using UnityEngine;

namespace Unity.Entities.Racing.Authoring
{
    public class WheelsReferenceAuthoring : MonoBehaviour
    {
        public GameObject WheelFR;
        public GameObject WheelFL;
        public GameObject WheelRR;
        public GameObject WheelRL;
        
        private class Baker : Baker<WheelsReferenceAuthoring>
        {
            public override void Bake(WheelsReferenceAuthoring referenceAuthoring)
            {
                var entity = GetEntity(referenceAuthoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, new VisualWheels
                {
                    WheelFR = GetEntity(referenceAuthoring.WheelFR, TransformUsageFlags.Dynamic),
                    WheelFL = GetEntity(referenceAuthoring.WheelFL, TransformUsageFlags.Dynamic),
                    WheelRR = GetEntity(referenceAuthoring.WheelRR, TransformUsageFlags.Dynamic),
                    WheelRL = GetEntity(referenceAuthoring.WheelRL, TransformUsageFlags.Dynamic)
                });
            
            }
        }
    }
}