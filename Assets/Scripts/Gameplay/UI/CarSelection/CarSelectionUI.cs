using System.Collections.Generic;
using Unity.Entities.Racing.Gameplay;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.UI
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
            }

            m_InCarSelection = value;
            m_CarSelectionContainer.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetSkinButtons(VisualElement root)
        {
            SkinButtons = new List<Button>();
            for (var i = 1; i <= NUM_OF_SKINS; i++)
            {
                var button = root.Q<Button>("skin-" + i);
                button.clicked += () => { OnSkinSelected(button); };
                button.clicked += () => { PlayerAudioManager.Instance.PlayClick(); };
                SkinButtons.Add(button);
            }
        }

        private void OnSkinSelected(VisualElement target)
        {
            SkinButtons.ForEach(b => b.RemoveFromClassList("button-active"));
            target.AddToClassList("button-active");
        }

        private void OnStartButtonClicked(ClickEvent evt)
        {
            PlayerAudioManager.Instance.PlayClick();
            ShowCarSelection(false);

            // Connect to Server of Create Client & Server
// #if UNITY_EDITOR
            if (ClientServerBootstrap.RequestedPlayType == ClientServerBootstrap.PlayType.ClientAndServer)
            {
                ServerConnectionUtils.StartClientServer(PlayerInfoController.Instance.Port);
            }
            else if (ClientServerBootstrap.RequestedPlayType == ClientServerBootstrap.PlayType.Client)
            {
                ServerConnectionUtils.ConnectToServer(PlayerInfoController.Instance.Ip,
                    PlayerInfoController.Instance.Port);
            }
// #elif UNITY_CLIENT
//             ServerConnectionUtils.ConnectToServer(PlayerInfoController.Instance.Ip, PlayerInfoController.Instance.Port);
// #else
//             // Client and Server
//             ServerConnectionUtils.StartClientServer(PlayerInfoController.Instance.Port);
// #endif
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
                OnSkinSelected(button);
                m_CurrentButton = button;
            }
        }
    }
}