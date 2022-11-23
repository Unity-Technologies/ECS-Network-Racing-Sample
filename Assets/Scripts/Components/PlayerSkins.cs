using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    public struct SkinElement : IBufferElementData
    {
        public Entity VisualEntity;
        public Entity BaseType ;
    }

    public struct Skin : IComponentData
    {
        [GhostField] public int Id;
        public bool NeedUpdate;
        public Entity VisualCar;
    }

    public struct HasVisual : IComponentData
    {
    }

    public struct VisualWheels : IComponentData
    {
        public Entity WheelFR;
        public Entity WheelFL;
        public Entity WheelRR;
        public Entity WheelRL;
    }
}