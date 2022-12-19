using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Places, and updates player name according to the player's position.
    /// </summary>
    public class PlayerInfoController : MonoBehaviour
    {
        public static PlayerInfoController Instance;
        
        [Header("Player Name")] 
        public GameObject PlayerNamePrefab;
        public string LocalPlayerName;
        private readonly Dictionary<Entity, GameObject> NameTags = new();

        [Header("Connection Settings")] 
        public string Ip;
        public string Port;

        [Header("Skin Selected")] 
        public int SkinId;
        
        private void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
                DontDestroyOnLoad(gameObject);
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

        public void SetConnectionInfo(string ip, string port)
        {
            Ip = ip;
            Port = port;
        }
    }
}