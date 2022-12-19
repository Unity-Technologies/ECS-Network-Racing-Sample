using System;
using System.ComponentModel;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;
using VivoxUnity;
#if AUTH_PACKAGE_PRESENT
using Unity.Services.Authentication;
#endif

namespace Unity.Entities.Racing.Gameplay
{
    public class VivoxManager : MonoBehaviour
    {
        public enum ChatCapability
        {
            TextOnly,
            AudioOnly,
            TextAndAudio
        }

        public event ParticipantValueChangedHandler OnSpeechDetectedEvent;
        public event ParticipantValueUpdatedHandler OnAudioEnergyChangedEvent;
        public event ChannelTextMessageChangedHandler OnTextMessageLogReceivedEvent;
        public event LoginStatusChangedHandler OnUserLoggedInEvent;
        public event LoginStatusChangedHandler OnUserLoggedOutEvent;
        public event ParticipantStatusChangedHandler OnParticipantAddedEvent;
        public event ParticipantStatusChangedHandler OnParticipantRemovedEvent;
        public event RecoveryStateChangedHandler OnRecoveryStateChangedEvent;

        public delegate void ChannelTextMessageChangedHandler(string sender, IChannelTextMessage channelTextMessage);
        public delegate void LoginStatusChangedHandler();
        public delegate void ParticipantStatusChangedHandler(string username, ChannelId channel, IParticipant participant);
        public delegate void ParticipantValueChangedHandler(string username, ChannelId channel, bool value);
        public delegate void ParticipantValueUpdatedHandler(string username, ChannelId channel, double value);
        public delegate void RecoveryStateChangedHandler(ConnectionRecoveryState recoveryState);

        public ILoginSession LoginSession;
        private Account m_Account;
        public static VivoxManager Instance { get; private set; }
        public string Channel => "MultipleUserChannel";
        public IReadOnlyDictionary<ChannelId, IChannelSession> ActiveChannels => LoginSession?.ChannelSessions;
        private Client _client => VivoxService.Instance.Client;
        public LoginState LoginState { get; private set; }
        public bool Muted => _client.AudioInputDevices.Muted;
        private bool IsInitiliazed { get; set; }

        private void Awake()
        {
            if (Instance != this && Instance != null)
            {
                Debug.LogWarning("Multiple VivoxVoiceManager detected in the scene. " +
                                 "Only one VivoxVoiceManager can exist at a time. " +
                                 "The duplicate VivoxVoiceManager will be destroyed.");
                Destroy(this);
                return;
            }

            Instance = this;
            IsInitiliazed = false;
        }

        private async void Start()
        {
            // if the Unity project is not linked to a Unity services project. 
            if (string.IsNullOrEmpty(Application.cloudProjectId))
            {
                Debug.LogWarning("To use Unity's dashboard services, " +
                                "you need to link your Unity project to a project ID. " +
                                "To do this, go to Project Settings to select your organization, " +
                                "select your project and then link a project ID. " +
                                "You also need to make sure your organization has access to the required products. " +
                                "Visit https://dashboard.unity3d.com to sign up.");
                return;
            }

            var options = new InitializationOptions();
            await UnityServices.InitializeAsync(options);
#if AUTH_PACKAGE_PRESENT
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
#endif
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                IsInitiliazed = true;
                VivoxService.Instance.Initialize();
            }
        }

        private void OnApplicationQuit()
        {
            // Needed to add this to prevent some unsuccessful uninit, we can revisit to do better
            if (IsInitiliazed)
            {
                Client.Cleanup();
            }

            if (IsInitiliazed && _client != null)
            {
                _client.Uninitialize();
            }
        }

        public void SetMute(bool value)
        {
            _client.AudioInputDevices.Muted = value;
        }

        public void Login(string displayName = null)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = "Player";
            }

            m_Account = new Account(displayName);
            LoginSession = _client.GetLoginSession(m_Account);
            LoginSession.PropertyChanged += OnLoginSessionPropertyChanged;
            LoginSession.BeginLogin(LoginSession.GetLoginToken(), SubscriptionMode.Accept, null, null, null, ar =>
            {
                try
                {
                    LoginSession.EndLogin(ar);
                }
                catch (Exception e)
                {
                    // Unbind if we failed to login.
                    Debug.LogException(e);
                    LoginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
                }
            });
        }

        private void OnLoginSessionPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "RecoveryState")
            {
                OnRecoveryStateChangedEvent?.Invoke(LoginSession.RecoveryState);
                return;
            }

            if (propertyChangedEventArgs.PropertyName != "State")
            {
                return;
            }

            var loginSession = (ILoginSession)sender;
            LoginState = loginSession.State;
            switch (LoginState)
            {
                case LoginState.LoggingIn:
                {
                    break;
                }
                case LoginState.LoggedIn:
                {
                    OnUserLoggedInEvent?.Invoke();
                    break;
                }
                case LoginState.LoggingOut:
                {
                    break;
                }
                case LoginState.LoggedOut:
                {
                    OnUserLoggedOutEvent?.Invoke();
                    LoginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
                    break;
                }
            }
        }

        public void Logout()
        {
            if (LoginSession != null && LoginState != LoginState.LoggedOut && LoginState != LoginState.LoggingOut)
            {
                LoginSession.Logout();
            }
        }

        public void JoinChannel()
        {
            if (LoginState == LoginState.LoggedIn)
            {
                var transmissionSwitch = true;
                var chatCapability = ChatCapability.TextAndAudio;
                var channel = new Channel(Channel);

                var channelSession = LoginSession.GetChannelSession(channel);
                channelSession.PropertyChanged += OnChannelPropertyChanged;
                channelSession.Participants.AfterKeyAdded += OnParticipantAdded;
                channelSession.Participants.BeforeKeyRemoved += OnParticipantRemoved;
                channelSession.Participants.AfterValueUpdated += OnParticipantValueUpdated;
                channelSession.MessageLog.AfterItemAdded += OnMessageLogRecieved;
                channelSession.BeginConnect(chatCapability != ChatCapability.TextOnly,
                    chatCapability != ChatCapability.AudioOnly, transmissionSwitch, channelSession.GetConnectToken(),
                    ar =>
                    {
                        try
                        {
                            channelSession.EndConnect(ar);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    });
            }
        }

        public void DisconnectAllChannels()
        {
            if (ActiveChannels?.Count > 0)
            {
                foreach (var channelSession in ActiveChannels)
                {
                    channelSession?.Disconnect();
                }
            }
        }

        private void OnParticipantAdded(object sender, KeyEventArg<string> keyEventArg)
        {
            ValidateArgs(new[] { sender, keyEventArg });

            // INFO: sender is the dictionary that changed and trigger the event.  Need to cast it back to access it.
            var source = (IReadOnlyDictionary<string, IParticipant>)sender;
            // Look up the participant via the key.
            var participant = source[keyEventArg.Key];
            var username = participant.Account.Name;
            var channel = participant.ParentChannelSession.Key;
            var channelSession = participant.ParentChannelSession;

            // Trigger callback
            OnParticipantAddedEvent?.Invoke(username, channel, participant);
        }

        private void OnParticipantRemoved(object sender, KeyEventArg<string> keyEventArg)
        {
            ValidateArgs(new[] { sender, keyEventArg });

            // INFO: sender is the dictionary that changed and trigger the event.  Need to cast it back to access it.
            var source = (IReadOnlyDictionary<string, IParticipant>)sender;
            // Look up the participant via the key.
            var participant = source[keyEventArg.Key];
            var username = participant.Account.Name;
            var channel = participant.ParentChannelSession.Key;
            var channelSession = participant.ParentChannelSession;

            if (participant.IsSelf)
            {
                // Now that we are disconnected, unsubscribe.
                channelSession.PropertyChanged -= OnChannelPropertyChanged;
                channelSession.Participants.AfterKeyAdded -= OnParticipantAdded;
                channelSession.Participants.BeforeKeyRemoved -= OnParticipantRemoved;
                channelSession.Participants.AfterValueUpdated -= OnParticipantValueUpdated;
                channelSession.MessageLog.AfterItemAdded -= OnMessageLogRecieved;

                // Remove session.
                var user = _client.GetLoginSession(m_Account);
                user.DeleteChannelSession(channelSession.Channel);
            }

            // Trigger callback
            OnParticipantRemovedEvent?.Invoke(username, channel, participant);
        }

        private static void ValidateArgs(object[] objs)
        {
            foreach (var obj in objs)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException(obj.GetType().ToString(), "Specify a non-null/non-empty argument.");
                }
            }
        }

        private void OnParticipantValueUpdated(object sender, ValueEventArg<string, IParticipant> valueEventArg)
        {
            ValidateArgs(new[] { sender, valueEventArg });

            var source = (IReadOnlyDictionary<string, IParticipant>)sender;
            // Look up the participant via the key.
            var participant = source[valueEventArg.Key];

            var username = valueEventArg.Value.Account.Name;
            var channel = valueEventArg.Value.ParentChannelSession.Key;
            var property = valueEventArg.PropertyName;

            switch (property)
            {
                case "SpeechDetected":
                {
                    OnSpeechDetectedEvent?.Invoke(username, channel, valueEventArg.Value.SpeechDetected);
                    break;
                }
                case "AudioEnergy":
                {
                    OnAudioEnergyChangedEvent?.Invoke(username, channel, valueEventArg.Value.AudioEnergy);
                    break;
                }
            }
        }

        private void OnChannelPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            ValidateArgs(new[] { sender, propertyChangedEventArgs });
            var channelSession = (IChannelSession)sender;

            // IF the channel has removed audio, make sure all the VAD indicators aren't showing speaking.
            if (propertyChangedEventArgs.PropertyName == "AudioState" &&
                channelSession.AudioState == ConnectionState.Disconnected)
            {
                foreach (var participant in channelSession.Participants)
                {
                    OnSpeechDetectedEvent?.Invoke(participant.Account.Name, channelSession.Channel, false);
                }
            }

            // IF the channel has fully disconnected, unsubscribe and remove.
            if ((propertyChangedEventArgs.PropertyName == "AudioState" ||
                 propertyChangedEventArgs.PropertyName == "TextState") &&
                channelSession.AudioState == ConnectionState.Disconnected &&
                channelSession.TextState == ConnectionState.Disconnected)
            {
                // Now that we are disconnected, unsubscribe.
                channelSession.PropertyChanged -= OnChannelPropertyChanged;
                channelSession.Participants.AfterKeyAdded -= OnParticipantAdded;
                channelSession.Participants.BeforeKeyRemoved -= OnParticipantRemoved;
                channelSession.Participants.AfterValueUpdated -= OnParticipantValueUpdated;
                channelSession.MessageLog.AfterItemAdded -= OnMessageLogRecieved;

                // Remove session.
                var user = _client.GetLoginSession(m_Account);
                user.DeleteChannelSession(channelSession.Channel);
            }
        }

        private void OnMessageLogRecieved(object sender, QueueItemAddedEventArgs<IChannelTextMessage> textMessage)
        {
            ValidateArgs(new[] { sender, textMessage });
            var channelTextMessage = textMessage.Value;
            OnTextMessageLogReceivedEvent?.Invoke(channelTextMessage.Sender.DisplayName, channelTextMessage);
        }
    }
}