using Unity.Entities.Racing.Common;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    ///     Add Leaderboard Data to the UI
    /// </summary>
    /// [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct AddLeaderboardData : ISystem
    {
        private bool m_AddData;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Race>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var race = GetSingleton<Race>();

            if (!race.IsShowingLeaderboard)
            {
                m_AddData = false;
                return;
            }

            if (m_AddData)
            {
                return;
            }

            if (LeaderboardPanel.Instance == null)
            {
                return;
            }

            var leaderboard = GetSingletonBuffer<LeaderboardData>();
            foreach (var leaderboardData in leaderboard)
            {
                LeaderboardPanel.Instance.AddLeaderboardLine(leaderboardData.Rank, leaderboardData.Name.Value,
                    leaderboardData.Time, leaderboardData.Ping);
            }

            m_AddData = true;
        }
    }
}