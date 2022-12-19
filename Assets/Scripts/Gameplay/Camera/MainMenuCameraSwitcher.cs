using Cinemachine;
using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
    public class MainMenuCameraSwitcher : MonoBehaviour
    {
        public static MainMenuCameraSwitcher Instance;

        [SerializeField] private CinemachineVirtualCamera mainCamera;
        [SerializeField] private CinemachineVirtualCamera carSelectionCamera;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void ShowCarSelectionCamera()
        {
            carSelectionCamera.Priority = 10;
            mainCamera.Priority = 6;
        }
    }
}