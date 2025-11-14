using System;
using System.Runtime.InteropServices;
using Unity.NetCode;

namespace Unity.Entities.Racing.Common
{
    public struct CarInput : IInputComponentData
    {
        public float Break;
        public float Handbreak;
        public float Vertical;
        public float Horizontal;
        public bool EngineStartStop;
    }

    public struct PlayerGameInput : IComponentData 
    {
        public IntPtr Value;
        public GCHandle gCHandle;
        public GameInput.VehicleActions GetInput()
        {
            gCHandle = GCHandle.FromIntPtr(Value);
            var gameInput = gCHandle.Target as GameInput;
            return gameInput.Vehicle;
        }

        public void Free() 
        {
            if (gCHandle.IsAllocated)
                gCHandle.Free();
        }
    }
}