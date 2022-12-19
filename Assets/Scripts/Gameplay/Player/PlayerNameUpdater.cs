using TMPro;
using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Makes the player name object to look at the main camera 
    /// </summary>
    public class PlayerNameUpdater : MonoBehaviour
    {
        public Transform NameTag;
        public TextMeshPro Label;
        private Transform m_Camera;

        private void Start()
        {
            m_Camera = Camera.main.transform;
        }

        private void Update()
        {
            NameTag.LookAt(m_Camera);
        }
    }
}