namespace Unity.Entities.Racing.Common
{
    /// <summary>
    /// Check the scene sections to load and update the GameLoadInfo singleton with loaded sections number
    /// </summary>
    public struct GameLoadInfo : IComponentData, IEnableableComponent
    {
        public int TotalSceneSections;
        public int LoadedSceneSections;

        public float GetProgress()
        {
            if (TotalSceneSections == 0)
            {
                return 0;
            }

            return (float) LoadedSceneSections / TotalSceneSections;
        }

        public bool IsLoaded => TotalSceneSections > 0 && LoadedSceneSections == TotalSceneSections;
    }
}