using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

namespace Dots.Racing
{
    public class ServerConnectionUtils
    {
        private const ushort NetworkPort = 7979;

        /// <summary>
        /// Start a Client and Server in your local IP
        /// </summary>
        public static void StartClientServer()
        {
            Debug.Log($"Starting Local Client and Server: {NetworkPort}");

            World.DisposeAllWorlds();

            var server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

            var ep = NetworkEndpoint.AnyIpv4.WithPort(NetworkPort);
            {
                using var drvQuery =
                    server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(ep);
            }

            ep = NetworkEndpoint.LoopbackIpv4.WithPort(NetworkPort);
            {
                using var drvQuery =
                    client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, ep);
            }
        }

        /// <summary>
        /// Connect to the server in the IP address
        /// </summary>
        /// <param name="ip">Server IP Address</param>
        public static void ConnectToServer(string ip)
        {
            Debug.Log($"Connecting to Server: {ip} // {NetworkPort}");
            World.DisposeAllWorlds();
            var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

            var ep = NetworkEndpoint.Parse(ip, NetworkPort);
            {
                using var drvQuery =
                    client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, ep);
            }
        }
    }
}
