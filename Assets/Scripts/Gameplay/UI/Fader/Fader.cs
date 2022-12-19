using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Fades a black screen to make transitions
    /// </summary>
    public class Fader : MonoBehaviour
    {
        public static Fader Instance;
        private VisualElement m_Fader;

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

        private void Start()
        {
            FadeOutIn(2500);
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_Fader = root.Q<VisualElement>("fader");
        }

        public void FadeIn(float opacity = 1f, int duration = 1000) 
        {
            m_Fader.style.opacity = 0f;
            m_Fader.experimental.animation.Start(new StyleValues { opacity = opacity }, duration).Ease(Easing.InCubic);
        }

        public void FadeOutIn(int duration = 1000)
        {
            m_Fader.style.opacity = 1f;
            m_Fader.experimental.animation.Start(new StyleValues { opacity = 0f }, duration).Ease(Easing.InCubic);
        }
    }
}