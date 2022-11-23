using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dots.Racing
{
    public class LeaderboardPanel : MonoBehaviour
    {
        public static LeaderboardPanel Instance;
        public VisualTreeAsset LeaderboardLine;
        private VisualElement m_LeaderboardContainer;
        private VisualElement m_MainPanel;

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
            m_LeaderboardContainer = root.Q<VisualElement>("leaderboard-container");
            m_MainPanel = root.Q<VisualElement>("main-panel");
        }

        public void ShowLeaderboard(bool value)
        {
            m_MainPanel.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void ClearLeaderboard()
        {
            m_LeaderboardContainer.Clear();
        }

        public void AddLeaderboardLine(int rank, string playerName, float seconds, int ping)
        {
            var leaderboardLine = LeaderboardLine.Instantiate();
            leaderboardLine.Q<Label>("rank").text = GetOrdinal(rank);
            leaderboardLine.Q<Label>("name").text = playerName;

            var time = seconds == 0 ? "--:--" : TimeSpan.FromSeconds(seconds).ToString("mm':'ss");

            leaderboardLine.Q<Label>("time").text = time;
            leaderboardLine.Q<Label>("ping").text = $"{ping} ms";
            m_LeaderboardContainer.Add(leaderboardLine);
        }

        private string GetOrdinal(int rank)
        {
            return rank switch
            {
                1 => rank + "ST",
                2 => rank + "ND",
                3 => rank + "RD",
                _ => rank + "TH"
            };
        }
    }
}