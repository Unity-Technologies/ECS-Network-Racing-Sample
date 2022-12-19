using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Access the visual and the prefab skin
    /// </summary>
    public struct SkinElement : IBufferElementData
    {
        public Entity VisualEntity;
        public Entity BaseType ;
    }

    /// <summary>
    /// Stores the skin data and instance
    /// </summary>
    public struct Skin : IComponentData
    {
        [GhostField] public int Id;
        public bool NeedUpdate;
        public Entity VisualCar;
    }

    /// <summary>
    /// Tags players who already have a skin set
    /// </summary>
    public struct HasVisual : IComponentData
    {
    }

    /// <summary>
    /// Access all visual wheels
    /// </summary>
    public struct VisualWheels : IComponentData
    {
        public Entity WheelFR;
        public Entity WheelFL;
        public Entity WheelRR;
        public Entity WheelRL;
    }
}