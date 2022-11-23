using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
    public class PlayerNamesController : MonoBehaviour
    {
        public static PlayerNamesController Instance;
        public GameObject PlayerNamePrefab;
        public string LocalPlayerName;
        private readonly Dictionary<Entity, GameObject> NameTags = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void CreateNameTag(string name, Entity player)
        {
            if (NameTags.ContainsKey(player))
            {
                DestroyNameTag(player);
            }

            var clone = Instantiate(PlayerNamePrefab);
            var label = clone.GetComponent<PlayerNameUpdater>().Label;
            label.text = name;
            NameTags.Add(player, clone);
        }

        public void UpdateNamePosition(Entity player, float3 position)
        {
            if (NameTags.ContainsKey(player))
            {
                NameTags[player].transform.position = position;
            }
        }

        public void DestroyNameTag(Entity player)
        {
            Destroy(NameTags[player]);
            NameTags.Remove(player);
        }

        public void RefreshNameTags(EntityManager manager)
        {
            var list = new List<Entity>();
            foreach (var nameTag in NameTags.Keys)
            {
                if (!manager.Exists(nameTag))
                {
                    list.Add(nameTag);
                }
            }

            foreach (var entity in list)
            {
                DestroyNameTag(entity);
            }
        }
    }
}