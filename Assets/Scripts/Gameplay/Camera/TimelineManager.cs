using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Access and play timeline directors 
    /// </summary>
    public class TimelineManager : MonoBehaviour
    {
        public static TimelineManager Instance;
        public PlayableDirector CountdownDirector;
        public CinemachineCamera[] FinishCameras;
        public PlayableDirector LeaderboardDirector;
        private int m_CurrentCameraIndex;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void PlayCountdownTimeline()
        {
            // Reset camera to Back
            if (CameraSwitcher.Instance != null)
            {
                CameraSwitcher.Instance.ShowBackCamera();
            }

            if (CountdownDirector != null)
            {
                CountdownDirector.time = 0;
                CountdownDirector.Play();
            }
        }

        public void SwitchToNearestCamera()
        {
            var minDistance = 1000f;
            var nearIndex = 0;
            for (var i = 0; i < FinishCameras.Length; i++)
            {
                var virtualCamera = FinishCameras[i];
                var cameraDistance = math.distance(virtualCamera.transform.position, virtualCamera.LookAt.position);
                if (cameraDistance < minDistance)
                {
                    minDistance = cameraDistance;
                    nearIndex = i;
                }
            }

            // Reset the last selected camera
            FinishCameras[m_CurrentCameraIndex].Priority = 5;
            // Assign new index
            m_CurrentCameraIndex = nearIndex;
            FinishCameras[m_CurrentCameraIndex].Priority = 12;
        }

        public void ResetFinalCameras()
        {
            foreach (var virtualCamera in FinishCameras)
            {
                virtualCamera.Priority = 5;
            }
        }

        public void PlayLeaderboardTimeline()
        {
            LeaderboardDirector.time = 0;
            LeaderboardDirector.Play();
        }
    }
}