using Unity.Burst;
using Unity.Scenes;
using Unity.Entities.Racing.Common;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Update the GameLoadInfo singleton with loaded sections number
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct LoadingScreenSystem : ISystem
    {
        private EntityQuery m_SceneSections;
        private Entity m_GameLoadInfoEntity;
        private bool ShouldShowLoadingScreen;
        public void OnCreate(ref SystemState state)
        {
            // Query only scene sections that are requested to load or loaded
            m_SceneSections = state.GetEntityQuery(
                ComponentType.ReadOnly<SceneSectionData>(),
                ComponentType.ReadOnly<RequestSceneLoaded>());
            m_GameLoadInfoEntity = state.EntityManager.CreateEntity(typeof(GameLoadInfo));
            state.RequireForUpdate<GameLoadInfo>();
        }
        public void OnDestroy(ref SystemState state) 
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var sceneSectionEntities = m_SceneSections.ToEntityArray(state.WorldUpdateAllocator);
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
            
            state.EntityManager.SetComponentData(m_GameLoadInfoEntity, gameLoadInfo);
            sceneSectionEntities.Dispose(state.Dependency);
            if (gameLoadInfo.IsLoaded)
            {
                // Create EnableGoInGame Entity when all the sub-scenes are loaded
                state.EntityManager.SetComponentEnabled<GameLoadInfo>(m_GameLoadInfoEntity, false);
            }
            else if (LoadingScreen.Instance != null)
            {
                ShouldShowLoadingScreen = true;
                LoadingScreen.Instance.ShowLoadingScreen(true);
            }
            
            // Disable the system when everything is loaded
            if (gameLoadInfo.IsLoaded && LoadingScreen.Instance != null && ShouldShowLoadingScreen)
            {
                // Disable Loading Screen
                ShouldShowLoadingScreen = false;
                LoadingScreen.Instance.ShowLoadingScreen(false);
            }
        }
    }
}