using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Properties;
using Unity.Transforms;
using UnityEngine;

namespace Unity.Entities.Racing.Common
{
    public struct PlayerNameTag : IComponentData
    {
    }

    public struct PlayerName : IComponentData
    {
        [GhostField] public FixedString64Bytes Name;
    }

    public readonly partial struct PlayerNameAspect : IAspect
    {
        public readonly Entity Self;
        readonly RefRO<PlayerName> m_PlayerName;
        public FixedString64Bytes Name => m_PlayerName.ValueRO.Name;
    }

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
        Finished,
        Leaderboard,
    }

    public struct Reset : IComponentData
    {
        [GhostField] public float3 TargetPosition;
        [GhostField] public quaternion TargetRotation;
        [GhostField] public bool Wheels;
        [GhostField] public bool Transform;
    }

    public struct Player : IComponentData
    {
        [GhostField] public PlayerState State;
    }

    public readonly partial struct LocalPlayerAspect : IAspect
    {
        public readonly Entity Self;
        readonly RefRO<LocalUser> m_localUser;
        readonly RefRW<Player> m_Player;
        private readonly RefRO<CarInput> m_CarInput;
        public Player Player => m_Player.ValueRO;
        public bool HasAnyInput()
        {
            return math.abs(m_CarInput.ValueRO.Horizontal) > 0 ||  math.abs(m_CarInput.ValueRO.Vertical)>0;
        }
    }

    public readonly partial struct PlayerAspect : IAspect
    {
        public readonly Entity Self;
        public readonly TransformAspect Transform;
        readonly RefRO<LocalToWorld> m_LocalToWorld;
        readonly RefRO<GhostOwnerComponent> m_GhostOwner;
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

        [CreateProperty]
        public LapProgress LapProgress => m_LapProgress.ValueRO;

        public void AddedToLeaderboard()
        {
            m_LapProgress.ValueRW.AddedToLeaderboard = true;
        }

        public void ResetLapProgress()
        {
            m_LapProgress.ValueRW.Reset();
        }
        
        public void ExitRace()
        {
            m_LapProgress.ValueRW.InRace = false;
        }

        public void SetCheckPoint(float3 value)
        {
            m_LapProgress.ValueRW.LastCheckPointPosition = value;
        }

        public void CountdownTeleportTimer(float deltaTime)
        {
            m_LapProgress.ValueRW.TimerToMovePlayer -= deltaTime;
        }

        public Player Player => m_Player.ValueRO;
        public Skin Skin => m_Skin.ValueRO;
        public PhysicsVelocity Velocity => m_Velocity.ValueRO;
        public Reset Reset => m_Reset.ValueRO;
        public LocalToWorld LocalToWorld => m_LocalToWorld.ValueRO;
        
        public void UpdateSkin (bool value = false)
        {
            m_Skin.ValueRW.NeedUpdate = value;
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

        public bool HasValidPosition()
        {
            var isFinite = math.isfinite(Transform.LocalPosition);
            return isFinite.x && isFinite.y && isFinite.z;
        }
    }
}