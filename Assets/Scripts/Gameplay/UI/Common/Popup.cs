using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Shows Messages.
    /// </summary>
    public class Popup : MonoBehaviour
    {
        public static Popup Instance { private set; get; }
        private Label m_Title;
        private Label m_Message;
        private VisualElement m_Container;
        private Button m_Button;

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
            m_Container = root.Q<VisualElement>("container");
            m_Title = root.Q<Label>("title");
            m_Message = root.Q<Label>("message");
            m_Button = root.Q<Button>("button");
            m_Button.clicked += Hide;
            m_Button.clicked += () => { PlayerAudioManager.Instance.PlayClick(); };
        }

        private void Hide()
        {
            m_Container.style.display = DisplayStyle.None;
            Fader.Instance.FadeOutIn(250);
        }

        public void Show(string title, string message, string buttonLabel = "Ok", Action Action = null)
        {
            m_Title.text = title;
            m_Message.text = message;
            m_Container.style.display = DisplayStyle.Flex;
            m_Button.text = buttonLabel;
            m_Button.clicked += () => { if (Action != null) Action(); };
            Fader.Instance.FadeIn(0.8f, 250);
        }
    }
}