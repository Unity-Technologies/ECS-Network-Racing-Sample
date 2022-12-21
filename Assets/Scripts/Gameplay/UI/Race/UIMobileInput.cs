using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Captures Mobile device input
    /// </summary>
    public class UIMobileInput : MonoBehaviour
    {
        public static UIMobileInput Instance;

        public float Horizontal;
        public float Vertical;

        private Button m_LeftButton;
        private Button m_RightButton;
        private Button m_ThrottleButton;
        private Button m_BrakeButton;

        private void Awake()
        {
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
            if (Instance == null)
            {
                Instance = this;
            }
#else
            Destroy(gameObject);
#endif
        }

        public void Show() 
        {
            m_LeftButton.style.display = DisplayStyle.Flex;
            m_RightButton.style.display = DisplayStyle.Flex;
            m_BrakeButton.style.display = DisplayStyle.Flex;
            m_ThrottleButton.style.display = DisplayStyle.Flex;
        }

        public void Hide() 
        {
            m_LeftButton.style.display = DisplayStyle.None;
            m_RightButton.style.display = DisplayStyle.None;
            m_BrakeButton.style.display = DisplayStyle.None;
            m_ThrottleButton.style.display = DisplayStyle.None;
        }

#if UNITY_IPHONE || UNITY_ANDROID
        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_LeftButton = root.Q<Button>("left-button");
            m_RightButton = root.Q<Button>("right-button");
            m_BrakeButton = root.Q<Button>("brake-button");
            m_ThrottleButton = root.Q<Button>("throttle-button");

            

            m_LeftButton.clickable.activators.Clear();
            m_RightButton.clickable.activators.Clear();
            m_BrakeButton.clickable.activators.Clear();
            m_ThrottleButton.clickable.activators.Clear();

            m_ThrottleButton.RegisterCallback<PointerDownEvent>(_ => { Vertical = 1f; });
            m_ThrottleButton.RegisterCallback<PointerUpEvent>(_ => { Vertical = 0f; });
            m_BrakeButton.RegisterCallback<PointerDownEvent>(_ => { Vertical = -1f; });
            m_BrakeButton.RegisterCallback<PointerUpEvent>(_ => { Vertical = 0f; });

            m_LeftButton.RegisterCallback<PointerDownEvent>(_ => { Horizontal = -1f; });
            m_LeftButton.RegisterCallback<PointerUpEvent>(_ => { Horizontal = 0f; });
            m_RightButton.RegisterCallback<PointerDownEvent>(_ => { Horizontal = 1f; });
            m_RightButton.RegisterCallback<PointerUpEvent>(_ => { Horizontal = 0f; });

        }
#endif
    }
}