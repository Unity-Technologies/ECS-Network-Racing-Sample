using UnityEngine;
using UnityEngine.UIElements;

namespace Dots.Racing
{
    public class UIMobileInput : MonoBehaviour
    {
        public static UIMobileInput Instance;

        public float Horizontal;
        public float Vertical;

        private VisualElement m_LeftButton;
        private VisualElement m_RightButton;
        private VisualElement m_ThrottleButton;
        private VisualElement m_BrakeButton;

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
            m_LeftButton = root.Q<VisualElement>("left-button");
            m_RightButton = root.Q<VisualElement>("right-button");
            m_BrakeButton = root.Q<VisualElement>("brake-button");
            m_ThrottleButton = root.Q<VisualElement>("throttle-button");

#if UNITY_IPHONE || UNITY_ANDROID
            m_LeftButton.style.display = DisplayStyle.Flex;
            m_RightButton.style.display = DisplayStyle.Flex;
            m_BrakeButton.style.display = DisplayStyle.Flex;
            m_ThrottleButton.style.display = DisplayStyle.Flex;

            m_ThrottleButton.RegisterCallback<MouseDownEvent>(_ => { Vertical = 1f; });
            m_ThrottleButton.RegisterCallback<MouseUpEvent>(_ => { Vertical = 0f; });
            m_BrakeButton.RegisterCallback<MouseDownEvent>(_ => { Vertical = -1f; });
            m_BrakeButton.RegisterCallback<MouseUpEvent>(_ => { Vertical = 0f; });

            m_LeftButton.RegisterCallback<MouseDownEvent>(_ => { Horizontal = -1f; });
            m_LeftButton.RegisterCallback<MouseUpEvent>(_ => { Horizontal = 0f; });
            m_RightButton.RegisterCallback<MouseDownEvent>(_ => { Horizontal = 1f; });
            m_RightButton.RegisterCallback<MouseUpEvent>(_ => { Horizontal = 0f; });
#endif
        }
    }
}