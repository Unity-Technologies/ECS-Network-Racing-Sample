using System.Linq;
using Unity.Entities.Racing.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;
using VivoxUnity;

public class VivoxPanel : MonoBehaviour
{
    private VisualElement m_VoiceChat;
    private VivoxManager m_VivoxVoiceManager;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        m_VoiceChat = root.Q<VisualElement>("chat-voice-panel");
    }

    private void Start()
    {
        m_VivoxVoiceManager = VivoxManager.Instance;
        m_VivoxVoiceManager.OnUserLoggedInEvent += ShowPanel;
        m_VivoxVoiceManager.OnUserLoggedInEvent += OnUserLoggedIn;
        m_VivoxVoiceManager.OnUserLoggedOutEvent += OnUserLoggedOut;

        if (m_VivoxVoiceManager.LoginState == LoginState.LoggedIn)
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
        m_VivoxVoiceManager.OnUserLoggedInEvent -= OnUserLoggedIn;
        m_VivoxVoiceManager.OnUserLoggedOutEvent -= OnUserLoggedOut;
    }

    private void ShowPanel()
    {
        m_VoiceChat.style.display = DisplayStyle.Flex;
        m_VivoxVoiceManager.OnUserLoggedInEvent -= ShowPanel;
        m_VivoxVoiceManager.SetMute(true);
    }

    //This should be can be added to a button
    private void LogoutOfVivoxService()
    {
        m_VivoxVoiceManager.DisconnectAllChannels();
        m_VivoxVoiceManager.Logout();
    }

    private void OnUserLoggedIn()
    {
        var lobbyChannel = m_VivoxVoiceManager.ActiveChannels.FirstOrDefault(ac => ac.Channel.Name == m_VivoxVoiceManager.Channel);
        if ((m_VivoxVoiceManager && m_VivoxVoiceManager.ActiveChannels.Count == 0) || lobbyChannel == null)
        {
            // Do nothing, participant added will take care of this
            m_VivoxVoiceManager.JoinChannel();
        }
        else
        {
            if (lobbyChannel.AudioState == ConnectionState.Disconnected)
            {
                // Ask for hosts since we're already in the channel and part added won't be triggered.
                lobbyChannel.BeginSetAudioConnected(true, true, ar =>
                {
                    Debug.Log("Now transmitting into lobby channel");
                });
            }
        }
    }

    private void OnUserLoggedOut()
    {
        m_VivoxVoiceManager.DisconnectAllChannels();
    }
}
