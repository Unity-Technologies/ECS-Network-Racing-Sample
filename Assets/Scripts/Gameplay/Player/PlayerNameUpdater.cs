using TMPro;
using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
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