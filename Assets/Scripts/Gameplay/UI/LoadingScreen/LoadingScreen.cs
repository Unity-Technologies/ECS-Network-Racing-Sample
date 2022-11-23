using UnityEngine;
using UnityEngine.UIElements;

namespace Dots.Racing
{
    /// <summary>
    /// Loading screen component
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance;
        public static ProgressBar LoadingBar;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            LoadingBar = root.Q<ProgressBar>("loading-progress-bar");
        }
    }
}