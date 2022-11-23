using System;
using Unity.Entities;
using UnityEngine;
using Unity.Entities.Racing.Common;

namespace Dots.Racing
{
    public class NetcodeSpawnerAuthoring : MonoBehaviour
    {
        // public GameObject CarBase;
        public SkinAuthoring[] SkinPrefabs;
        public SpawnPointAuthoring[] SpawnPoints;
    }

    [Serializable]
    public struct SpawnPointAuthoring
    {
        public Transform TrackSpawnPoint;
        public Transform LobbySpawnPoint;
    }
    
    [Serializable]
    public struct SkinAuthoring
    {
        public GameObject SkinPrefab;
        public GameObject BaseType;
    }

    public class NetcodeSpawnerBaker : Baker<NetcodeSpawnerAuthoring>
    {
        public override void Bake(NetcodeSpawnerAuthoring authoring)
        {
            AddComponent(new PlayerSpawner());

            var skins = AddBuffer<SkinElement>();
            foreach (var skin in authoring.SkinPrefabs)
            {
                skins.Add(new SkinElement
                {
                    VisualEntity = GetEntity(skin.SkinPrefab), 
                    BaseType = GetEntity(skin.BaseType)
                });
            }

            var spawnPoints = AddBuffer<SpawnPoint>();
            foreach (var spawnPoint in authoring.SpawnPoints)
            {
                spawnPoints.Add(new SpawnPoint
                {
                    TrackPosition = spawnPoint.TrackSpawnPoint.position,
                    TrackRotation = spawnPoint.TrackSpawnPoint.rotation,
                    LobbyPosition = spawnPoint.LobbySpawnPoint.position,
                    LobbyRotation = spawnPoint.LobbySpawnPoint.rotation,
                });
            }
        }
    }
}