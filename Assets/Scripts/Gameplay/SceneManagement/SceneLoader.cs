using System.Collections;
using Unity.Entities.Racing.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Entities.Racing.Gameplay
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance;

        private void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
                DontDestroyOnLoad(gameObject);
            } 
        }

        public void LoadScene(SceneType sceneType)
        {
            StartCoroutine(LoadSceneAsync(sceneType.ToString())); 
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            LoadingScreen.Instance.ShowLoadingScreen(true);
            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                if (operation.progress >= 0.9f)
                {
                    operation.allowSceneActivation = true;
                }
                yield return null;
            }
            
            LoadingScreen.Instance.ShowLoadingScreen(false);
        }
    }
}