using Unity.Collections;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Properties;
using Unity.Transforms;

#pragma warning disable CS0414

namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Added to a player who already has a name
    /// </summary>
    public struct PlayerNameTag : IComponentData
    {
    }

    /// <summary>
    /// Stores the player's name
    /// </summary>
    public struct PlayerName : IComponentData
    {
        [GhostField] public FixedString64Bytes Name;
    }

    /// <summary>
    /// Tags the local player for filtering
    /// </summary>
    public struct LocalUser : IComponentData
    {
    }

    public enum PlayerState
    {
        None,
        Lobby,
        ReadyToRace,
        StartingRace,
        Countdown,
        Race,
        CelebrationIdle,
        Finished,
        Leaderboard
    }

    /// <summary>
    /// Stores the initial player values
    /// </summary>
    public struct Reset : IComponentData
    {
        [GhostField] public float3 TargetPosition;
        [GhostField] public quaternion TargetRotation;
        [GhostField] public bool Wheels;
        [GhostField] public bool Transform;
        
        public void ResetVehicle()
        {
            Transform = true;
            Wheels = true;
        }

        public void SetTargetTransform(float3 position, quaternion rotation)
        {
            TargetPosition = position;
            TargetRotation = rotation;
        }
    }

    /// <summary>
    /// Access the player's parameters in the race.
    /// </summary>
    public struct Player : IComponentData
    {
        [GhostField] public PlayerState State;
        public bool StartingRace => State == PlayerState.StartingRace;
        public bool InRace => State == PlayerState.Race;
        public bool HasFinished => State == PlayerState.Finished;
        public bool IsCelebrating => State == PlayerState.CelebrationIdle;
    }
}