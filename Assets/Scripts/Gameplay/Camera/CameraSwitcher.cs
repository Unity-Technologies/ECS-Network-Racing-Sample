using Unity.Cinemachine;
using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Access and switch cameras
    /// </summary>
    public class CameraSwitcher : MonoBehaviour
    {
        public static CameraSwitcher Instance;

        [SerializeField] private CinemachineCamera mainCamera;
        [SerializeField] private CinemachineCamera frontCamera;

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
    }
}