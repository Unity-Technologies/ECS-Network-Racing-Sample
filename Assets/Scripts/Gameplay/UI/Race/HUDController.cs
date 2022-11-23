using System.Collections;
using Unity.Entities.Racing.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Dots.Racing
{
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance;
        public static Button StartRaceButton;
        public static Button CancelStartButton;
        public static Button ResetCarButton;
        public static Button AudioButton;

        private Button m_MenuButton;
        private Button m_BackButton;
        private VisualElement m_MainHUD;
        private VisualElement m_RaceInfoPanel;
        private VisualElement m_BottomPanel;
        private VisualElement m_LeftMenu;
        private VisualElement m_AudioIcon;
        private Label m_RankLabel;
        private Label m_MainCenterLabel;
        private Label m_BottomMessageLabel;
        private Label m_LapLabel;

        private bool m_Counting;
        private bool m_ShowingFinish;
        private bool m_LeftMenuEnabled;
        private string[] m_CounterTextValue = {"3", "2", "1", "GO!"};

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            ShowHUD(false);
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            // Rank Position
            m_RankLabel = root.Q<Label>("position-value");
            // Lap Count
            m_LapLabel = root.Q<Label>("lap-value");
            // HUD Labels
            m_MainCenterLabel = root.Q<Label>("main-center-label");
            m_BottomMessageLabel = root.Q<Label>("bottom-message-label");
            // Panel
            m_MainHUD = root.Q<VisualElement>("main-hud");
            m_RaceInfoPanel = root.Q<VisualElement>("race-info-panel");
            m_LeftMenu = root.Q<VisualElement>("left-menu");
            m_BottomPanel = root.Q<VisualElement>("bottom-panel");
            // Buttons
            StartRaceButton = root.Q<Button>("start-race-button");
            CancelStartButton = root.Q<Button>("cancel-start-button");
            ResetCarButton = root.Q<Button>("reset-button");
            AudioButton = root.Q<Button>("audio-button");
            m_AudioIcon = AudioButton.Q<VisualElement>("icon");
            m_MenuButton = root.Q<Button>("menu-button");
            m_BackButton = root.Q<Button>("back-button");

            m_MenuButton.clicked += () => { ShowLeftMenu(true); };
            m_BackButton.clicked += () => { ShowLeftMenu(false); };
            AudioButton.clicked += ToggleVolume;
        }

        private void ToggleVolume()
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

            PlayerAudioManager.Instance.ToggleVolume();
        }

        public void ShowHUD(bool value)
        {
            m_MainHUD.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void ShowLeftMenu(bool value, bool showMenuButton = true)
        {
            if (value && m_LeftMenuEnabled)
                return;

            var layout = m_LeftMenu.layout;
            if (value)
            {
                ShowMenuButton(false);
                m_LeftMenu.experimental.animation.Layout(new Rect(Vector2.zero, layout.size), 500).Ease(Easing.OutCirc);
            }
            else
            {
                m_LeftMenu.experimental.animation.Layout(new Rect(new Vector2(-layout.width, 0), layout.size), 500)
                    .Ease(Easing.OutCirc)
                    .OnCompleted(() => { ShowMenuButton(true); });
            }
            m_LeftMenuEnabled = value;
        }

        public void SetLap(int currentLap, int totalLaps)
        {
            m_LapLabel.text = $"{currentLap}/{totalLaps}";
        }

        public void SetPosition(int position, int numPlayers)
        {
            m_RankLabel.text = $"{position}/{numPlayers}";
        }

        public void ShowRaceInfoPanel(bool value)
        {
            if (value)
            {
                m_RaceInfoPanel.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_RaceInfoPanel.style.display = DisplayStyle.None;
                StopCoroutine(ShowFinish(0));
            }
        }

        public void ShowMenuButton(bool value)
        {
            m_MenuButton.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void ShowCancelButton(bool value)
        {
            CancelStartButton.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void ShowBottomMessage(bool value, string message = "")
        {
            m_BottomMessageLabel.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            m_BottomPanel.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            m_BottomMessageLabel.text = message;
        }

        public void ShowLobbyState()
        {
            // Show and Hide Elements
            ShowCancelButton(false);
            ShowBottomMessage(false);
            ShowRaceInfoPanel(false);
            ShowMenuButton(true);

            // Clean texts and variables
            m_MainCenterLabel.text = "";
            m_BottomMessageLabel.text = "";
            m_Counting = false;
            m_ShowingFinish = false;
            m_RaceInfoPanel.style.opacity = 1;
        }

        public void StartCountDown()
        {
            //TODO: Rewrite this to get the value from System
            // Show Countdown
            m_MainCenterLabel.style.display = DisplayStyle.Flex;

            if (m_Counting)
                return;
            m_Counting = true;
            AnimateCounter(0);
        }

        private void AnimateCounter(int index)
        {
            if (index >= m_CounterTextValue.Length)
                return;

            m_MainCenterLabel.text = m_CounterTextValue[index];
            m_MainCenterLabel.style.opacity = 1;
            m_MainCenterLabel.transform.scale = Vector3.one;
            m_MainCenterLabel.transform.position = Vector3.zero;
            m_MainCenterLabel.experimental.animation.Position(new Vector3(0, -50f), 1000).Ease(Easing.OutCubic);
            m_MainCenterLabel.experimental.animation.Scale(1.5f, 1000).Ease(Easing.OutCubic);
            m_MainCenterLabel.experimental.animation.Start(new StyleValues {opacity = 0f}, 1000);
            m_MainCenterLabel.experimental.animation.Start(new StyleValues {opacity = 0f}, 1000)
                .Ease(Easing.InCubic)
                .OnCompleted(() => { AnimateCounter(++index); });
        }

        public void Finish(bool value = true, int rank = 0)
        {
            if (value)
            {
                if (m_ShowingFinish)
                    return;

                m_ShowingFinish = true;
                StartCoroutine(ShowFinish(rank));
            }
            else
            {
                m_MainCenterLabel.text = "";
                m_ShowingFinish = false;
                StopCoroutine(ShowFinish(0));
            }
        }

        public void ShowFinishCounter(int timer)
        {
            m_BottomMessageLabel.text = $"THE RACE WILL FINISH IN {timer}";
            m_BottomMessageLabel.style.display = DisplayStyle.Flex;
            m_BottomPanel.style.display = DisplayStyle.Flex;
        }

        public void EnableStartButton(bool value)
        {
            if (value)
            {
                StartRaceButton.style.opacity = 1f;
                StartRaceButton.pickingMode = PickingMode.Position;
            }
            else
            {
                StartRaceButton.style.opacity = 0.5f;
                StartRaceButton.pickingMode = PickingMode.Ignore;
            }
        }

        private IEnumerator ShowFinish(int rank)
        {
            // Show finish message in the center of the screen for 5 seconds
            m_MainCenterLabel.style.display = DisplayStyle.Flex;
            m_MainCenterLabel.text = "FINISH";
            m_MainCenterLabel.style.opacity = 1;
            m_MainCenterLabel.transform.scale = Vector3.one;
            m_MainCenterLabel.transform.position = Vector3.zero;
            m_RaceInfoPanel.experimental.animation.Start(new StyleValues {opacity = 0f}, 500).Ease(Easing.InCubic);
            yield return new WaitForSeconds(2f);
            m_MainCenterLabel.text = $"{GetOrdinal(rank)} PLACE";
            m_MainCenterLabel.experimental.animation.Position(new Vector3(0, -500f), 500).Ease(Easing.OutCubic);
            m_MainCenterLabel.experimental.animation.Scale(0.3f, 500).Ease(Easing.OutCubic);
            yield return null;
        }

        private string GetOrdinal(int rank)
        {
            return rank switch
            {
                1 => rank + "ST",
                2 => rank + "ND",
                3 => rank + "RD",
                _ => rank + "TH"
            };
        }
    }
}