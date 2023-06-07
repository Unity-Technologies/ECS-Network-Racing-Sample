using Unity.Entities.Racing.Common;
using Unity.Burst;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Change player's state
    /// </summary>
    [BurstCompile]
    public partial struct ChangePlayerStateJob : IJobEntity
    {
        public PlayerState CurrentState;
        public PlayerState TargetState;

        private void Execute(ref Player player)
        {
            if (player.State == CurrentState)
            {
                player.State = TargetState;
            }
        }
    }
}