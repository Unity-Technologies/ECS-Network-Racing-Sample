using Unity.Burst;
using Unity.Entities;
using Unity.Entities.Racing.Common;
using Unity.Scenes;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct GameSceneLoader : ISystem
    {
        private EntityQuery m_Query;
        private RaceState previousState;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
            m_Query = state.GetEntityQuery(ComponentType.ReadOnly<SceneInfo>());
        }

        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();
            if (previousState == race.State)
                return;
            previousState = race.State;
            switch (race.State)
            {
                case RaceState.ReadyToRace:
                    LoadScene(ref state, SceneType.Race);
                    break;
                case RaceState.StartingRace:
                    UnloadScene(ref state, SceneType.Lobby);
                    break;
                case RaceState.Leaderboard:
                    LoadScene(ref state, SceneType.Lobby);
                    break;
                case RaceState.Lobby:
                    UnloadScene(ref state, SceneType.Race);
                    break;
            }
        }

        void LoadScene(ref SystemState state, SceneType sceneType)
        {
            var scenesInfo = m_Query.ToComponentDataArray<SceneInfo>(state.WorldUpdateAllocator);
            foreach (var sceneInfo in scenesInfo)
            {
                if (sceneInfo.SceneType == sceneType)
                {
                    SceneSystem.LoadSceneAsync(state.WorldUnmanaged, sceneInfo.SceneGuid);
                }
            }
        }

        void UnloadScene(ref SystemState state, SceneType sceneType)
        {
            var scenesInfo = m_Query.ToComponentDataArray<SceneInfo>(state.WorldUpdateAllocator);
            foreach (var sceneInfo in scenesInfo)
            {
                if (sceneInfo.SceneType == sceneType)
                {
                    SceneSystem.UnloadScene(state.WorldUnmanaged, sceneInfo.SceneGuid);
                }
            }
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}