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
    }
    
    public class WheelsReferenceBaker : Baker<WheelsReferenceAuthoring>
    {
        public override void Bake(WheelsReferenceAuthoring referenceAuthoring)
        {
            AddComponent(new VisualWheels
            {
                WheelFR = GetEntity(referenceAuthoring.WheelFR),
                WheelFL = GetEntity(referenceAuthoring.WheelFL),
                WheelRR = GetEntity(referenceAuthoring.WheelRR),
                WheelRL = GetEntity(referenceAuthoring.WheelRL)
            });
            
        }
    }
}