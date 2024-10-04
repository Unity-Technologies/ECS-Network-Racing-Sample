using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.Vivox;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Adds events to Vivox UI
    /// </summary>
    [RequireComponent(typeof(VivoxPlayerList))]
    public class VivoxPanel : MonoBehaviour
    {
        private VivoxSession VivoxSession => VivoxManager.Instance.Session;
        private VivoxChannel VivoxChannel => VivoxManager.Instance.Channel;
        private VivoxDevicesVolume VivoxDevices => VivoxManager.Instance.Devices;
        private VisualElement m_VoiceChat;
        private bool m_HasStartedVivox = false;
        private VivoxPlayerList m_PlayerList;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_VoiceChat = root.Q<VisualElement>("chat-voice-panel");
            m_VoiceChat.style.display = DisplayStyle.None;
            m_PlayerList = GetComponent<VivoxPlayerList>();
        }

        private void Update()
        {
            if(VivoxManager.Instance == null || VivoxSession == null || VivoxManager.Instance.Service == null || m_HasStartedVivox)
                return;

            m_HasStartedVivox = true;
            VivoxManager.Instance.Service.LoggedIn += ShowPanel;
            VivoxManager.Instance.Service.LoggedIn += OnUserLoggedIn;
            VivoxManager.Instance.Service.LoggedOut += OnUserLoggedOut;
            VivoxManager.Instance.Service.LoggedOut += HidePanel;

            if (VivoxManager.Instance.Session.IsLogged)
            {
                OnUserLoggedIn();
            }
            else
            {
                OnUserLoggedOut();
            }
        }

        private void OnDestroy()
        {
            if (VivoxManager.Instance == null || VivoxSession == null)
                return;
            
            VivoxManager.Instance.Service.LoggedIn -= OnUserLoggedIn;
            VivoxManager.Instance.Service.LoggedOut -= OnUserLoggedOut;
            VivoxManager.Instance.Service.LoggedOut -= HidePanel;
            VivoxManager.Instance.Service.LoggedIn -= ShowPanel;
        }

        private void ShowPanel()
        {
            Debug.Log("<color=green>[VIVOX] ShowPanel has been triggered</color>");
            m_VoiceChat.style.display = DisplayStyle.Flex;
            VivoxDevices.SetMicrophoneMute(true);
        }

        private void HidePanel()
        {
            Debug.Log("<color=green>[VIVOX] HidePanel has been triggered</color>");
            m_VoiceChat.style.display = DisplayStyle.None;
            VivoxDevices.SetMicrophoneMute(true);
        }

        private void OnUserLoggedIn()
        {
            Debug.Log("<color=green>[VIVOX] OnUserLoggedIn has been triggered</color>");
            m_PlayerList.Init();
            VivoxChannel.JoinChannel();
        }

        private void OnUserLoggedOut()
        {
            VivoxChannel.DisconnectAllChannels();
        }
    }
}
