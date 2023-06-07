using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Entities.Racing.Gameplay
{
    public class AudioUI : MonoBehaviour
    {
        private VisualElement m_AudioIcon;
        public static Button AudioButton;
        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            AudioButton = root.Q<Button>("audio-menu-button");
            m_AudioIcon = AudioButton.Q<VisualElement>("volume-icon");
            AudioButton.clicked += ToggleVolume;
            UpdatingIcon();
        }
        
        private void ToggleVolume()
        {
            PlayerAudioManager.Instance.ToggleVolume();
            UpdatingIcon();
            PlayerAudioManager.Instance.PlayClick();
        }

        private void UpdatingIcon()
        {
            if (PlayerAudioManager.Instance.IsMute)
            {
                m_AudioIcon.RemoveFromClassList("volume-icon");
                m_AudioIcon.AddToClassList("volume-mute");
            }
            else
            {
                m_AudioIcon.AddToClassList("volume-icon");
                m_AudioIcon.RemoveFromClassList("volume-mute");
            }
        }
    }
}