using System;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Based on Physics Category Names.
    /// </summary>
    [Flags]
    public enum CollisionCategories
    {
        Static = 0x1,
        Dynamic = 0x2,
        Triggers = 0x4,
        Car = 0x8,
    }
    /// <summary>
    /// Assign the chassis entity to the wheel for later processing
    /// </summary>
    public struct ChassisReference : IComponentData
    {
        public Entity Value;
    }
}