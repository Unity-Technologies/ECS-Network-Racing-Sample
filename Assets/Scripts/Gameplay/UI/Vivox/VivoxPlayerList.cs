using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities.Racing.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;
using VivoxUnity;
using Random = Unity.Mathematics.Random;

public class VivoxPlayerList : MonoBehaviour
{
    public Texture2D MicrophoneOn;
    public Texture2D MicrophoneOff;
    public Vector2 MinMaxAlphaIndicatorSpeaker;
    public Vector2 MinMaxScaleIndicatorSpeaker;

    private Button m_MicrophoneButton;
    private VisualElement m_MicrophoneIcon;
    private VisualElement m_SpeakerSignal;
    private Label m_SpeakerName;

    private IParticipant lastParticipant;
    private Random m_Random;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var container = root.Q<VisualElement>("chat-voice-panel").Q<VisualElement>("container");
        m_SpeakerName = container.Q<Label>("name");
        m_MicrophoneButton = container.Q<Button>("button");
        m_SpeakerSignal = m_MicrophoneButton.Q<VisualElement>("signal");
        m_MicrophoneIcon = m_MicrophoneButton.Q<VisualElement>("icon");
        m_MicrophoneButton.clicked += OnMuteClicked;
        m_Random = new Random((uint)Time.frameCount + 2);
    }

    private void Start()
    {
        VivoxManager.Instance.OnParticipantAddedEvent += OnParticipantAdded;
        VivoxManager.Instance.OnParticipantRemovedEvent += OnParticipantRemoved;
    }

    private void OnDestroy()
    {
        VivoxManager.Instance.OnParticipantAddedEvent -= OnParticipantAdded;
        VivoxManager.Instance.OnParticipantRemovedEvent -= OnParticipantRemoved;
    }

    private void OnParticipantAdded(string username, ChannelId channel, IParticipant participant)
    {
        participant.PropertyChanged += OnParticipantOnPropertyChanged;
    }

    private void OnParticipantRemoved(string username, ChannelId channel, IParticipant participant)
    {
        participant.PropertyChanged -= OnParticipantOnPropertyChanged;
    }

    private void OnMuteClicked()
    {
        SetMute(!VivoxManager.Instance.Muted);
    }

    private void SetMute(bool value)
    {
        VivoxManager.Instance.SetMute(value);
        var style = new StyleBackground(value ? MicrophoneOff : MicrophoneOn);
        m_MicrophoneIcon.style.backgroundImage = style;
    }

    private void OnParticipantOnPropertyChanged(object obj, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case "SpeechDetected":
                var participant = obj as IParticipant;

                if (lastParticipant == null && participant.SpeechDetected)
                {
                    lastParticipant = participant;
                    m_SpeakerName.text = participant.Account.DisplayName;
                    AnimateIcon();
                }
                else if (lastParticipant == participant && !participant.SpeechDetected)
                {
                    lastParticipant = null;
                    m_SpeakerSignal.style.scale = new StyleScale(Vector2.one);
                    m_SpeakerName.text = "";
                    AssignIndicatorAlpha(MinMaxAlphaIndicatorSpeaker.x);
                }
                break;
        }
    }

    private void AnimateIcon() 
    {
        if (lastParticipant == null)
            return;
        var nextScale = m_Random.NextFloat(MinMaxScaleIndicatorSpeaker.x, MinMaxScaleIndicatorSpeaker.y);
        AssignIndicatorAlpha(MinMaxAlphaIndicatorSpeaker.y);
        m_SpeakerSignal.experimental.animation.Scale(nextScale, 100).OnCompleted(AnimateIcon);
    }

    private void AssignIndicatorAlpha(float alpha) 
    {
        var currentColor = m_SpeakerSignal.style.backgroundColor.value;
        currentColor.a = alpha;
        m_SpeakerSignal.style.backgroundColor = new StyleColor(currentColor);
    }
}
