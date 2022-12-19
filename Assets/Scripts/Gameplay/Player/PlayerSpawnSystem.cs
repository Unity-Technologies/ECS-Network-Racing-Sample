using Unity.Burst;
using Unity.Collections;
using Unity.Entities.Racing.Common;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct RequestSpawnCarSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var name = PlayerInfoController.Instance.LocalPlayerName;
            if (name == string.Empty)
            {
                name = "Player";
            }
            
            var requestSpawnEntity = state.EntityManager.CreateEntity(typeof(SendRpcCommandRequestComponent));
            state.EntityManager.AddComponentData(requestSpawnEntity,
                new SpawnPlayerRequest
                {
                    Name = name,
                    Id = PlayerInfoController.Instance.SkinId
                });

            state.Enabled = false;
        }
    }

    /// <summary>
    /// Gets the connection and spawn the player
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(GhostSimulationSystemGroup))]
    public partial struct PlayerSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Must wait for the spawner entity scene to be streamed in, most likely instantaneous in
            // this sample but good to be sure
            state.RequireForUpdate<AutoConnect>();
            state.RequireForUpdate<PlayerSpawner>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var spawnPointBuffer = GetSingletonBuffer<SpawnPoint>(true);
            var skinBuffer = GetSingletonBuffer<SkinElement>(true);
            foreach (var request in Query<SpawnPlayerRequestAspect>())
            {
                commandBuffer.DestroyEntity(request.Self);

                var entityNetwork = request.SourceConnection;
                var networkId = state.EntityManager.GetComponentData<NetworkIdComponent>(entityNetwork);
                Debug.Log($"Spawning player for connection {networkId.Value}");

                // Instantiate the Car Base for this skin
                var player = commandBuffer.Instantiate(skinBuffer[request.Id].BaseType);

                // The network ID owner must be set on the ghost owner component on the players
                // this is used internally for example to set up the CommandTarget properly
                commandBuffer.SetComponent(player, new GhostOwnerComponent {NetworkId = networkId.Value});
                var name = request.Name;
                name = name == "" ? $"Player{networkId.Value}" : name;
                commandBuffer.SetComponent(player, new PlayerName {Name = name});
                commandBuffer.SetComponent(player, new Player {State = PlayerState.Lobby});

                // Mark that this connection has had a player spawned for it so we won't process it again
                commandBuffer.AddComponent<PlayerSpawned>(entityNetwork);

                // Give each NetworkId their own spawn pos:
                var index = (networkId.Value - 1) % (spawnPointBuffer.Length - 1);
                var position = spawnPointBuffer[index].LobbyPosition;
                var rotation = spawnPointBuffer[index].LobbyRotation;
                commandBuffer.SetComponent(player, new LocalTransform
                {
                    Position = position,
                    Rotation = rotation,
                    Scale = 1
                });
                commandBuffer.SetComponent(player, new LapProgress
                {
                    LastCheckPointPosition = position,
                    LapCount = 10
                });
                commandBuffer.SetComponent(player, new Reset
                {
                    TargetPosition = position,
                    TargetRotation = rotation
                });
                commandBuffer.SetComponent(player, new Skin
                {
                    Id = request.Id
                });

                // Add the player to the linked entity group on the connection so it is destroyed
                // automatically on disconnect (destroyed with connection entity destruction)
                commandBuffer.AppendToBuffer(entityNetwork, new LinkedEntityGroup {Value = player});

                // Create an entity to allow server to reset when all players disconnect 
                if (!HasSingleton<ResetServerOnDisconnect>())
                {
                    var e = state.EntityManager.CreateEntity();
                    commandBuffer.AddComponent<ResetServerOnDisconnect>(e);
                }
            }

            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }
    }
}


