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
    /// Access to player's name component
    /// </summary>
    public readonly partial struct PlayerNameAspect : IAspect
    {
        public readonly Entity Self;
        readonly RefRO<PlayerName> m_PlayerName;
        public FixedString64Bytes Name => m_PlayerName.ValueRO.Name;
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

    /// <summary>
    /// Access the only the local player information
    /// </summary>
    public readonly partial struct LocalPlayerAspect : IAspect
    {
        private readonly RefRW<CarInput> CarInput;
        private readonly RefRO<LocalTransform> m_LocalTransform;
        private readonly RefRO<LapProgress> m_LapProgress;
        private readonly RefRW<Player> m_Player;
        private readonly RefRW<LocalUser> m_LocalUser;
        private readonly RefRO<LocalToWorld> m_LocalToWorld;
        public Player Player => m_Player.ValueRO;
        public LapProgress LapProgress => m_LapProgress.ValueRO;
        public LocalToWorld LocalToWorld => m_LocalToWorld.ValueRO;

        public bool HasAnyInput()
        {
            return math.abs(CarInput.ValueRO.Horizontal) > 0 || math.abs(CarInput.ValueRO.Vertical) > 0;
        }

        public bool HasValidPosition()
        {
            var isFinite = math.isfinite(m_LocalTransform.ValueRO.Position);
            return isFinite.x && isFinite.y && isFinite.z;
        }
    }

    /// <summary>
    /// Access all players information
    /// </summary>
    public readonly partial struct PlayerAspect : IAspect
    {
        public readonly Entity Self;
        readonly RefRW<LocalTransform> m_LocalTransform;
        readonly RefRO<LocalToWorld> m_LocalToWorld;
        readonly RefRO<GhostOwner> m_GhostOwner;
        readonly RefRO<Rank> m_Rank;
        readonly RefRO<PlayerName> m_Name;
        readonly RefRW<Player> m_Player;
        readonly RefRW<Skin> m_Skin;
        readonly RefRW<LapProgress> m_LapProgress;
        readonly RefRW<PhysicsVelocity> m_Velocity;
        readonly RefRW<Reset> m_Reset;

        public int NetworkId => m_GhostOwner.ValueRO.NetworkId;
        public int Rank => m_Rank.ValueRO.Value;
        public FixedString64Bytes Name => m_Name.ValueRO.Name;
        public bool IsCelebrating => Player.IsCelebrating && LapProgress.CelebrationIdleDelay > 0;
        public bool FinishedCelebration => Player.IsCelebrating && LapProgress.CelebrationIdleDelay <= 0;
        public bool HasArrived => (Player.IsCelebrating || Player.HasFinished) && LapProgress.HasArrived;

        [CreateProperty] public LapProgress LapProgress => m_LapProgress.ValueRO;
        public Player Player => m_Player.ValueRO;
        public Skin Skin => m_Skin.ValueRO;
        public PhysicsVelocity Velocity => m_Velocity.ValueRO;
        public Reset Reset => m_Reset.ValueRO;
        public LocalToWorld LocalToWorld => m_LocalToWorld.ValueRO;

        public void ResetPlayer()
        {
            m_LocalTransform.ValueRW.Position = Reset.TargetPosition;
            m_LocalTransform.ValueRW.Rotation = Reset.TargetRotation;
            ResetVelocity();
            SetPlayerTransformReady();
        }

        public void AddedToLeaderboard()
        {
            m_LapProgress.ValueRW.AddedToLeaderboard = true;
        }

        public void ResetLapProgress()
        {
            m_LapProgress.ValueRW.Reset();
        }

        public void ResetCheckpoint()
        {
            m_LapProgress.ValueRW.CurrentCheckPoint = 0;
        }

        public void SetCheckPoint(float3 value)
        {
            m_LapProgress.ValueRW.LastCheckPointPosition = value;
        }

        public void IncreaseLapCount()
        {
            m_LapProgress.ValueRW.CurrentLap++;
        }

        public void RestLapCount()
        {
            m_LapProgress.ValueRW.CurrentLap = 0;
        }

        public void ReduceCelebrationTimer(float value)
        {
            m_LapProgress.ValueRW.CelebrationIdleDelay -= value;
        }

        public void UpdateSkin(bool value = false)
        {
            m_Skin.ValueRW.NeedUpdate = value;
        }

        public void SetCelebration(float time, double elapseTime)
        {
            m_Player.ValueRW.State = PlayerState.CelebrationIdle;
            m_LapProgress.ValueRW.CelebrationIdleDelay = time;
            m_LapProgress.ValueRW.ArrivalTime = elapseTime;
        }

        public void SetFinishedRace()
        {
            m_Player.ValueRW.State = PlayerState.Finished;
        }

        public void ResetVelocity()
        {
            m_Velocity.ValueRW.Linear = float3.zero;
            m_Velocity.ValueRW.Angular = float3.zero;
        }

        public void ResetVehicle()
        {
            m_Reset.ValueRW.Transform = true;
            m_Reset.ValueRW.Wheels = true;
        }

        public void SetTargetTransform(float3 position, quaternion rotation)
        {
            m_Reset.ValueRW.TargetPosition = position;
            m_Reset.ValueRW.TargetRotation = rotation;
        }

        public void SetPlayerTransformReady()
        {
            m_Reset.ValueRW.Transform = false;
        }

        public void SetPlayerWheelsReady()
        {
            m_Reset.ValueRW.Wheels = false;
        }
    }
}