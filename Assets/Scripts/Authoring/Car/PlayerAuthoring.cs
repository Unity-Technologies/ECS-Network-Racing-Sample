using Unity.Entities.Racing.Common;
using Unity.Entities;
using UnityEngine;

namespace Dots.Racing
{
    public class PlayerAuthoring : MonoBehaviour { }

    public class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            AddComponent<Player>();
        }
    }
}