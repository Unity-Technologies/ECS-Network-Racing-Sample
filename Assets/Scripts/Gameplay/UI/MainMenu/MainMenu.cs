using System;
using System.Collections;
using Gameplay.UI;
using Unity.Entities.Racing.Gameplay;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dots.Racing
{
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance;

        public TextField NameField;
        public TextField IpField;
        public TextField PortField;
        private VisualElement m_MainMenuPanel;
        private Button m_StartButton;
        private VisualElement[] m_FocusRing;
        private FocusController m_FocusController;
        private int m_FocusRingIndex;
        private bool m_InMainMenu = true;

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
            m_MainMenuPanel = root.Q<VisualElement>("container");
            NameField = root.Q<TextField>("name-field");
            IpField = root.Q<TextField>("ip-field");
            PortField = root.Q<TextField>("port-field");
            NameField.value = Environment.UserName.ToUpper();
            m_StartButton = root.Q<Button>("start-button");
            m_StartButton.clicked += OnStartButtonClicked;
            m_FocusController = root.focusController;
        }

        private void Start()
        {
            // Set the focus ring manually
            m_FocusRing = new VisualElement[] {NameField, IpField, PortField, m_StartButton};
            // Set focus to the name text field
            NameField.Focus();
            StartCoroutine(UpdateInput());
        }

        private void OnDisable()
        {
            m_StartButton.clicked -= OnStartButtonClicked;
        }

        private void OnStartButtonClicked()
        {
            PlayerNamesController.Instance.LocalPlayerName = NameField.value;
            // Disable Main Menu
            m_MainMenuPanel.style.display = DisplayStyle.None;
            if (CameraSwitcher.Instance != null)
            {
                CameraSwitcher.Instance.ShowCarSelectionCamera();
                CarSelectionUI.Instance.ShowCarSelection(true);
            }

            StopCoroutine(UpdateInput());
            m_InMainMenu = false;
        }

        private IEnumerator UpdateInput()
        {
            while (m_InMainMenu)
            {
                // TODO: Check Navigation when Unity Input System is compatible with DOTS
                var vertical = Input.GetAxis("Vertical");
                vertical -= Input.GetAxis("LeftStickY"); // Inverted axis
                vertical += Input.GetAxis("DPadY");
                vertical = math.clamp(vertical, -1, 1);

                // Threshold level for Sticks
                if (math.abs(vertical) > 0.2f)
                {
                    switch (vertical)
                    {
                        case < 0f:
                            m_FocusRingIndex = (m_FocusRingIndex + 1) % m_FocusRing.Length;
                            FocusElement(m_FocusRingIndex);
                            break;
                        case > 0f:
                            m_FocusRingIndex = math.abs((m_FocusRingIndex - 1) % m_FocusRing.Length);
                            FocusElement(m_FocusRingIndex);
                            break;
                    }

                    yield return new WaitForSeconds(0.25f);
                }

                yield return null;
            }

            yield return null;
        }

        private void FocusElement(int index)
        {
            if (m_FocusController.focusedElement != m_FocusRing[index])
                m_FocusRing[index].Focus();
        }
    }
}