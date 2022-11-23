using System.Collections.Generic;
using Dots.Racing;
using Unity.Entities.Racing.Gameplay;
using Unity.Mathematics;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Gameplay.UI
{
    public class CarSelectionUI : MonoBehaviour
    {
        private const int NUM_OF_SKINS = 16;
        
        public static CarSelectionUI Instance;
        public static List<Button> SkinButtons;
        
        public UnityAction StartGameEvent;
        
        private VisualElement m_StartButton;
        private VisualElement m_CarSelectionContainer;
        private VisualElement m_SkinContainer;
        private VisualElement m_StartButtonFill;
        private FocusController m_FocusController;
        private bool m_InCarSelection;
        private VisualElement m_CurrentButton;
        private float m_SubmitWidth;

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
            m_FocusController = root.focusController;
            m_CarSelectionContainer = root.Q<VisualElement>("container");
            m_StartButtonFill = root.Q<VisualElement>("start-button-fill");
            m_StartButton = root.Q<VisualElement>("start-button");
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
                // Focus on the first Car Skin
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
            if (!string.IsNullOrEmpty(VivoxService.Instance.PlayerId) && VivoxService.Instance.Client.Initialized)
            {
                VivoxManager.Instance.Login(PlayerNamesController.Instance.LocalPlayerName);
            }
            ShowCarSelection(false);
            CameraSwitcher.Instance.ShowFrontCamera();
            HUDController.Instance.ShowHUD(true);
            Fader.Instance.FadeOutIn();
            
            StartGameEvent.Invoke();
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
                m_SubmitWidth += Time.deltaTime * 400;
                m_SubmitWidth = math.clamp(m_SubmitWidth, 0, m_StartButton.layout.width);
                m_StartButtonFill.style.width = m_SubmitWidth;
            }
            else
            {
                // Decrease Submit
                m_SubmitWidth -= Time.deltaTime * 800;
                m_SubmitWidth = math.clamp(m_SubmitWidth, 0, m_StartButton.layout.width);
                m_StartButtonFill.style.width = m_SubmitWidth;
            }
            
            // Just check buttons if the selection changed
            if(m_CurrentButton == m_FocusController.focusedElement)
                return;
            
            if (m_FocusController.focusedElement is Button button)
            {
                OnSkinSelected(button);
                m_CurrentButton = button;
            }
        }
    }
}