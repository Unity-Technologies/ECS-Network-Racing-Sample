using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Dots.Racing
{
    public class Fader : MonoBehaviour
    {
        public static Fader Instance;
        private VisualElement m_Fader;

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
            m_Fader = root.Q<VisualElement>("fader");
        }

        private void Start()
        {
            FadeOutIn();
        }

        public void FadeOutIn()
        {
            m_Fader.style.opacity = 1f;
            m_Fader.experimental.animation.Start(new StyleValues {opacity = 0f}, 1000).Ease(Easing.InCubic);
        }
    }
}