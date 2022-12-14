using Unity.Entities;
using Unity.Entities.Racing.Common;
using UnityEditor;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Dots.Racing
{
#if UNITY_EDITOR
    public class SceneInfoAuthoring : MonoBehaviour
    {
        public SceneType SceneType;
        public SceneAsset SceneAsset;
    }

    public class SceneLoaderBaker : Baker<SceneInfoAuthoring>
    {
        public override void Bake(SceneInfoAuthoring authoring)
        {
            var guid = new Hash128(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(authoring.SceneAsset)));
            var sceneInfo = new SceneInfo()
            {
                SceneType = authoring.SceneType,
                SceneGuid = guid
            };
            AddComponent(sceneInfo);
        }
    }
#endif
}