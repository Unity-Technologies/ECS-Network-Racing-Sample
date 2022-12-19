using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    public struct CarInput : IInputComponentData
    {
        public float Horizontal;
        public float Vertical;
    }
}