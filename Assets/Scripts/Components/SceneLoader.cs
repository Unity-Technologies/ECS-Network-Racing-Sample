namespace Unity.Entities.Racing.Common
{
    public enum SceneType
    {
        None = 0x0,
        Lobby = 0x1,
        Race = 0x2,
        Main = 0x3,
        MainMenu = 0x4,
    }
    public struct SceneInfo : IComponentData
    {
        public SceneType SceneType;
        public Hash128 SceneGuid;
    }
}