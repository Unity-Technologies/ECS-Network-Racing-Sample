using Cinemachine;
using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
    public class CameraSwitcher : MonoBehaviour
    {
        public static CameraSwitcher Instance;

        [SerializeField] private CinemachineVirtualCamera mainCamera;
        [SerializeField] private CinemachineVirtualCamera frontCamera;
        [SerializeField] private CinemachineVirtualCamera carSelectionCamera;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void ShowFrontCamera()
        {
            mainCamera.Priority = 6;
            frontCamera.Priority = 10;
        }

        public void ShowBackCamera()
        {
            frontCamera.Priority = 6;
            mainCamera.Priority = 10;
        }

        public void ShowCarSelectionCamera()
        {
            carSelectionCamera.Priority = 10;
            mainCamera.Priority = 6;
            frontCamera.Priority = 6;
        }
    }
}