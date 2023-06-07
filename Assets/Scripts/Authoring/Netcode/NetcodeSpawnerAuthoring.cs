using System;
using UnityEngine;
using Unity.Entities.Racing.Common;

namespace Unity.Entities.Racing.Authoring
{
    public class NetcodeSpawnerAuthoring : MonoBehaviour
    {
        public SkinAuthoring[] SkinPrefabs;
        public SpawnPointAuthoring[] SpawnPoints;
        
        private class Baker : Baker<NetcodeSpawnerAuthoring>
        {
            public override void Bake(NetcodeSpawnerAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
                AddComponent<PlayerSpawner>(entity);

                var skins = AddBuffer<SkinElement>(entity);
                foreach (var skin in authoring.SkinPrefabs)
                {
                    skins.Add(new SkinElement
                    {
                        VisualEntity = GetEntity(skin.SkinPrefab, TransformUsageFlags.Renderable), 
                        BaseType = GetEntity(skin.BaseType, TransformUsageFlags.None)
                    });
                }

                var spawnPoints = AddBuffer<SpawnPoint>(entity);
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
}