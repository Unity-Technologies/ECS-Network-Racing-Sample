using UnityEngine;
using UnityEngine.UIElements;
#if !UNITY_SERVER
using UnityEngine.UIElements.Experimental;
#endif

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    ///     Loading screen component
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance;
        private VisualElement m_Container;
        private VisualElement m_LoadingCircle;
        private Label m_TextLabel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_Container = root.Q<VisualElement>("loading-screen-container");
            m_LoadingCircle = root.Q<VisualElement>("loading-circle");
            m_TextLabel = m_Container.Q<Label>("loading-label");
            RotateLoading();
        }

        public void ShowLoadingScreen(bool value, string label = "LOADING...")
        {
#if !UNITY_SERVER
            m_TextLabel.text = label;
            m_Container.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
#endif
        }

        private void RotateLoading()
        {
#if !UNITY_SERVER
            m_LoadingCircle.experimental.animation.Rotation(Quaternion.Euler(0f, 0, -180f), 500).Ease(Easing.Linear)
                .OnCompleted(() =>
                {
                    m_LoadingCircle.experimental.animation.Rotation(Quaternion.Euler(0f, 0f, -360f), 500)
                        .Ease(Easing.Linear).OnCompleted(RotateLoading);
                });
#endif
        }
    }
}