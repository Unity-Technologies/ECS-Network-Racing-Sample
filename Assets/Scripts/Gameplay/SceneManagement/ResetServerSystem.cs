using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Scenes;
using UnityEngine;

namespace Dots.Racing
{
    [BurstCompile]
    public partial struct ResetServerSystem : ISystem
    {
        private EntityQuery _query;
        private EntityQuery _networkStreamQuery;
        private EntityQuery _subSceneQuery;

        public void OnCreate(ref SystemState state)
        {
            _query = state.GetEntityQuery(ComponentType.ReadOnly<ResetServerOnDisconnect>());
            _subSceneQuery = state.GetEntityQuery(ComponentType.ReadOnly<SceneReference>());
            _networkStreamQuery = state.GetEntityQuery(ComponentType.ReadOnly<NetworkStreamConnection>());
            state.RequireForUpdate(_query);
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var connections = _networkStreamQuery.CalculateEntityCount();
            if (connections > 0)
                return;
            
            // All players disconnected 
            // We reset the race scene 
            Debug.Log("Resetting server");

            var subScenes = _subSceneQuery.ToEntityArray(state.WorldUpdateAllocator);
            foreach (var subScene in subScenes)
            {
                SceneSystem.UnloadScene(state.WorldUnmanaged, subScene);
                SceneSystem.LoadSceneAsync(state.WorldUnmanaged, subScene);
            }

            state.EntityManager.DestroyEntity(_query);

        }
    }
}