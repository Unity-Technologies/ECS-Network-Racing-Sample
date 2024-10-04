using Unity.Cinemachine;
using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
    public class MainMenuCameraSwitcher : MonoBehaviour
    {
        public static MainMenuCameraSwitcher Instance;

        [SerializeField] private CinemachineCamera m_MainCamera;
        [SerializeField] private CinemachineCamera m_CarSelectionCamera;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void ShowCarSelectionCamera()
        {
            m_CarSelectionCamera.Priority = 10;
            m_MainCamera.Priority = 6;
        }
    }
}