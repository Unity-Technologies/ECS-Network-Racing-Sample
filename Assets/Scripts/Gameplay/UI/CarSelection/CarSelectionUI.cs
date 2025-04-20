using System.Collections.Generic;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Creates the list of skins
    /// </summary>
    public class CarSelectionUI : MonoBehaviour
    {
        private const int NUM_OF_SKINS = 16;

        public static CarSelectionUI Instance;
        public static List<Button> SkinButtons;

        private VisualElement m_StartButton;
        private VisualElement m_CarSelectionContainer;
        private VisualElement m_StartButtonFill;
        private FocusController m_FocusController;
        private bool m_InCarSelection;
        private VisualElement m_CurrentButton;
        private float m_SubmitWidth;
        private int m_SelectedSkinIndex = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_FocusController = root.focusController;
            m_CarSelectionContainer = root.Q<VisualElement>("car-selection-container");
            m_StartButtonFill = root.Q<VisualElement>("car-selection-start-button-fill");
            m_StartButton = root.Q<VisualElement>("car-selection-start-button");
            m_StartButton.RegisterCallback<ClickEvent>(OnStartButtonClicked);
            SetSkinButtons(root);
            
            // Log that car selection UI is ready
            RaceLogger.Info("Car Selection UI initialized");
        }

        private void OnDisable()
        {
            m_StartButton.UnregisterCallback<ClickEvent>(OnStartButtonClicked);
        }

        public void ShowCarSelection(bool value)
        {
            if (value)
            {
                // Focus on the first Car Skin Button
                SkinButtons[0].Focus();
                RaceLogger.LogSection("CAR SELECTION");
                RaceLogger.Gameplay("Car selection screen opened");
            }
            else if (m_InCarSelection)
            {
                // Only log when we're actually closing the screen, not when initially setting it to false
                RaceLogger.Gameplay("Car selection screen closed");
            }

            m_InCarSelection = value;
            m_CarSelectionContainer.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetSkinButtons(VisualElement root)
        {
            SkinButtons = new List<Button>();
            for (var i = 1; i <= NUM_OF_SKINS; i++)
            {
                var skinIndex = i; // Capture the index for use in the lambda
                var button = root.Q<Button>("skin-" + i);
                button.clicked += () => { OnSkinSelected(button, skinIndex); };
                button.clicked += () => { PlayerAudioManager.Instance.PlayClick(); };
                SkinButtons.Add(button);
            }
        }

        private void OnSkinSelected(VisualElement target, int skinIndex)
        {
            SkinButtons.ForEach(b => b.RemoveFromClassList("button-active"));
            target.AddToClassList("button-active");
            
            // Only log if the selection actually changed
            if (m_SelectedSkinIndex != skinIndex)
            {
                string playerName = PlayerInfoController.Instance != null ? 
                    PlayerInfoController.Instance.LocalPlayerName : 
                    "Player";
                
                RaceLogger.Success($"Player '{playerName}' selected car skin #{skinIndex}");
                
                // Log through RaceLoggerManager if available
                if (RaceLoggerManager.Instance != null)
                {
                    RaceLoggerManager.Instance.LogPlayerAction(playerName, $"Selected car skin #{skinIndex}");
                }
                
                m_SelectedSkinIndex = skinIndex;
            }
        }

        private void OnStartButtonClicked(ClickEvent evt)
        {
            PlayerAudioManager.Instance.PlayClick();
            
            // Log start button clicked
            string playerName = PlayerInfoController.Instance != null ? 
                PlayerInfoController.Instance.LocalPlayerName : 
                "Player";
            
            RaceLogger.Success($"Player '{playerName}' confirmed car selection and started the game");
            
            if (RaceLoggerManager.Instance != null)
            {
                RaceLoggerManager.Instance.LogPlayerAction(playerName, "Confirmed car selection and started the game");
            }
            
            ShowCarSelection(false);

#if AUTH_PACKAGE_PRESENT
                VivoxManager.Instance?.Session?.Login(PlayerInfoController.Instance.LocalPlayerName);
#endif

            // Connect to Server of Create Client & Server
            if (ClientServerBootstrap.RequestedPlayType == ClientServerBootstrap.PlayType.ClientAndServer)
            {
                ServerConnectionUtils.StartClientServer(PlayerInfoController.Instance.Port);
                RaceLogger.Network("Starting Client/Server mode");
            }
            else if (ClientServerBootstrap.RequestedPlayType == ClientServerBootstrap.PlayType.Client)
            {
                RaceLogger.Network($"Connecting to server at {PlayerInfoController.Instance.Ip}:{PlayerInfoController.Instance.Port}");
                ServerConnectionUtils.ConnectToServer(PlayerInfoController.Instance.Ip,
                    PlayerInfoController.Instance.Port);
            }
        }

        private void Update()
        {
            if (!m_InCarSelection)
                return;

            // Check if Submit is hold
            if (Input.GetButton("Submit"))
            {
                // Button width has not been initialized
                if (m_StartButton.layout.width <= 0)
                    return;

                if (m_SubmitWidth >= m_StartButton.layout.width)
                {
                    OnStartButtonClicked(new ClickEvent());
                }

                // Increase Submit 
                m_SubmitWidth += Time.deltaTime * 400f;
                m_SubmitWidth = math.clamp(m_SubmitWidth, 0, m_StartButton.layout.width);
                m_StartButtonFill.style.width = m_SubmitWidth;
            }
            else
            {
                // Decrease Submit
                m_SubmitWidth -= Time.deltaTime * 800f;
                m_SubmitWidth = math.clamp(m_SubmitWidth, 0, m_StartButton.layout.width);
                m_StartButtonFill.style.width = m_SubmitWidth;
            }

            // Just check buttons if the selection changed
            if (m_CurrentButton == m_FocusController.focusedElement)
                return;

            if (m_FocusController.focusedElement is Button button)
            {
                // Find the index of the button
                int index = -1;
                for (int i = 0; i < SkinButtons.Count; i++)
                {
                    if (SkinButtons[i] == button)
                    {
                        index = i + 1; // +1 because our indices start at 1
                        break;
                    }
                }
                
                OnSkinSelected(button, index);
                m_CurrentButton = button;
            }
        }
    }
}