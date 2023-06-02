using Unity.Burst;
using Unity.Entities.Racing.Common;
using Unity.Scenes;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Loads and Unloads Subscenes according to the game state
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct GameSceneLoader : ISystem
    {
        private EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
            m_Query = state.GetEntityQuery(ComponentType.ReadOnly<SceneInfo>());
        }

        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();
            if (race.LastState == race.State)
            {
                return;
            }

            if (race.IsReadyToRace)
            {
                LoadScene(ref state, SceneType.Race);
            }
            else if (race.IsRaceStarting)
            {
                UnloadScene(ref state, SceneType.Lobby);
            }
            else if (race.IsShowingLeaderboard)
            {
                LoadScene(ref state, SceneType.Lobby);
            }
            else if (race.NotStarted)
            {
                UnloadScene(ref state, SceneType.Race);
            }
        }

        private void LoadScene(ref SystemState state, SceneType sceneType)
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

        private void UnloadScene(ref SystemState state, SceneType sceneType)
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
    }
}