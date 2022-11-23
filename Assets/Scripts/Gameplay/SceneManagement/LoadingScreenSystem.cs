using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Scenes;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    /// <summary>
    /// Update the GameLoadInfo singleton with loaded sections number
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct LoadingScreenSystem : ISystem
    {
        private EntityQuery m_SceneSections;

        public void OnCreate(ref SystemState state)
        {
            // Query only scene sections that are requested to load or loaded
            m_SceneSections = state.GetEntityQuery(
                ComponentType.ReadOnly<SceneSectionData>(),
                ComponentType.ReadOnly<RequestSceneLoaded>());
            state.EntityManager.CreateEntity(typeof(GameLoadInfo));
            state.RequireForUpdate<GameLoadInfo>();
            state.RequireForUpdate<NetworkIdComponent>();
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var sceneSectionEntities = m_SceneSections.ToEntityArray(Allocator.TempJob);
            var loadedSceneSections = 0;
            foreach (var entity in sceneSectionEntities)
            {
                if (SceneSystem.IsSectionLoaded(state.WorldUnmanaged, entity))
                {
                    loadedSceneSections++;
                }
            }

            var gameLoadInfo = new GameLoadInfo
            {
                TotalSceneSections = sceneSectionEntities.Length,
                LoadedSceneSections = loadedSceneSections
            };

            // Update Loading Screen
            if (LoadingScreen.Instance == null)
                return;
            
            LoadingScreen.LoadingBar.value = math.lerp(LoadingScreen.LoadingBar.value, gameLoadInfo.GetProgress(),
                Time.DeltaTime);

            SetSingleton(gameLoadInfo);
            sceneSectionEntities.Dispose(state.Dependency);

            // Disable the system when everything is loaded
            if (gameLoadInfo.IsLoaded)
            {
                // Create EnableGoInGame Entity when all the sub-scenes are loaded
                state.Enabled = false;

                // Disable Loading Screen
                LoadingScreen.Instance.gameObject.SetActive(false);
            }
        }
    }
}