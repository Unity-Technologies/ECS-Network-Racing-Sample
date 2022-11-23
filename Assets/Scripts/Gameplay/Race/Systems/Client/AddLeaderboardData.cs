using Unity.Entities.Racing.Common;
using Unity.Burst;
using Unity.Entities;
using static Unity.Entities.SystemAPI;

namespace Dots.Racing
{
    /// <summary>
    /// Add Leaderboard Data to the UI
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    [BurstCompile]
    public partial struct AddLeaderboardData : ISystem
    {
        private bool m_LeaderboardShown;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();

            if (race.State is not RaceState.Leaderboard)
            {
                m_LeaderboardShown = false;
                return;
            }

            if (m_LeaderboardShown)
                return;

            if (LeaderboardPanel.Instance == null)
                return;
            
            var leaderboard = GetSingletonBuffer<LeaderboardData>();
            foreach (var leaderboardData in leaderboard)
            {
                LeaderboardPanel.Instance.AddLeaderboardLine(leaderboardData.Rank, leaderboardData.Name.Value, leaderboardData.Time, leaderboardData.Ping);
            }

            m_LeaderboardShown = true;
        }
    }
}