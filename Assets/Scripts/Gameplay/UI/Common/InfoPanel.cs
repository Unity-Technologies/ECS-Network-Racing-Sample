using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Shows diagnostics of the performance, and Ping.
    /// </summary>
    public class InfoPanel : MonoBehaviour
    {
        private readonly string m_FPSFormattedString = "{value} FPS";
        private Button m_BackButton;
        private VisualElement m_BackIcon;
        private Label m_EntitiesLabel;
        private Label m_FPSLabel;
        private VisualElement m_InfoPanel;
        private bool m_PanelHidden;
        private Label m_PingLabel;
        private Label m_PlayersLabel;
        private Label m_SystemsLabel;

        public static InfoPanel Instance { private set; get; }

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
            m_InfoPanel = root.Q<VisualElement>("info-panel-body");
            m_BackButton = root.Q<Button>("info-panel-back-button");
            m_FPSLabel = root.Q<Label>("fps-value");
            m_PlayersLabel = root.Q<Label>("players-value");
            m_PingLabel = root.Q<Label>("ping-value");
            m_EntitiesLabel = root.Q<Label>("entities-value");
            m_SystemsLabel = root.Q<Label>("systems-value");
            m_BackIcon = root.Q<VisualElement>("back-icon");
            m_BackButton.clicked += ShowPanel;
            m_BackButton.clicked += () => { PlayerAudioManager.Instance.PlayClick(); };
        }

        private void ShowPanel()
        {
            var height = m_InfoPanel.layout.height + 10f;

            if (m_PanelHidden)
            {
                m_InfoPanel.experimental.animation.Position(new Vector3(0, 0), 500).Ease(Easing.OutCubic)
                    .OnCompleted(() => { m_PanelHidden = false; });
                m_BackIcon.experimental.animation.Rotation(Quaternion.Euler(0, 0, 0), 300).Ease(Easing.OutQuad);
            }
            else
            {
                m_InfoPanel.experimental.animation.Position(new Vector3(0, -height), 500).Ease(Easing.OutCubic)
                    .OnCompleted(() => { m_PanelHidden = true; });
                m_BackIcon.experimental.animation.Rotation(Quaternion.Euler(0, 0, 180f), 300).Ease(Easing.OutQuad);
            }
        }

        public void SetNumberOfPlayers(int number)
        {
            m_PlayersLabel.text = number.ToString();
        }

        public void SetFPSLabel(float fps)
        {
            m_FPSLabel.text = m_FPSFormattedString.Replace("{value}", math.round(fps).ToString("0.0"));
        }

        public void SetPingLabel(int estimatedRTT, int deviationRTT)
        {
            m_PingLabel.text = estimatedRTT < 1000
                ? $"{estimatedRTT}±{deviationRTT}ms"
                : $"~{estimatedRTT + deviationRTT / 2:0}ms";
            ;
        }

        public void SetSystemsLabel(uint value)
        {
            m_SystemsLabel.text = value.ToString();
        }

        public void SetEntitiesLabel(int value)
        {
            m_EntitiesLabel.text = value.ToString();
        }
    }
}