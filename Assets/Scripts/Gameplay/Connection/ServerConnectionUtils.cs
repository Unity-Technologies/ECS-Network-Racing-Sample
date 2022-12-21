using System.Linq;
using Unity.Entities.Racing.Common;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using System.Net;

namespace Unity.Entities.Racing.Gameplay
{
    public class ServerConnectionUtils
    {
        private const ushort k_NetworkPort = 7979;
        private static World m_Server;
        private static World m_Client;
        public static bool IsTryingToConnect = false;

        /// <summary>
        ///     Start a Client and Server in your local IP
        /// </summary>
        public static void StartClientServer(string port)
        {
            IsTryingToConnect = true;
            if (ClientServerBootstrap.RequestedPlayType != ClientServerBootstrap.PlayType.ClientAndServer)
            {
                Debug.LogError($"Creating client/server worlds is not allowed if playmode is set to {ClientServerBootstrap.RequestedPlayType}");
                return;
            }

            if (m_Server != null && m_Server.IsCreated)
                m_Server.Dispose();

            if (m_Client != null && m_Client.IsCreated)
                m_Client.Dispose();

            m_Server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            m_Client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

            //Destroy the local simulation world to avoid the game scene to be loaded into it
            //This prevent rendering (rendering from multiple world with presentation is not greatly supported)
            //and other issues.
            DestroyLocalSimulationWorld();

            World.DefaultGameObjectInjectionWorld ??= m_Server;

            SceneLoader.Instance.LoadScene(SceneType.Main);

            var networkEndpoint = NetworkEndpoint.AnyIpv4.WithPort(ParsePortOrDefault(port));
            {
                using var drvQuery = m_Server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(networkEndpoint);
            }

            networkEndpoint = NetworkEndpoint.LoopbackIpv4.WithPort(ParsePortOrDefault(port));
            {
                using var drvQuery = m_Client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(m_Client.EntityManager, networkEndpoint);
            }
        }

        /// <summary>
        ///     Connect to the server in the IP address and port
        /// </summary>
        /// <param name="ip">Server IP Address</param>
        /// <param name="port">Port</param>
        public static void ConnectToServer(string ip, string port)
        {
            IsTryingToConnect = true;
            var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            
            DestroyLocalSimulationWorld();
            
            World.DefaultGameObjectInjectionWorld ??= client;
            
            SceneLoader.Instance.LoadScene(SceneType.Main);
            
            var networkEndpoint = NetworkEndpoint.Parse(ip, ParsePortOrDefault(port));
            {
                using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, networkEndpoint);
            }
        }

        public static bool ValidateIPv4(string ipString)
        {
            if (string.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            var splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            return splitValues.All(r => byte.TryParse(r, out var tempForParsing)) && ValidateIP(ipString);
        }

        private static bool ValidateIP(string addrString)
        {
            return IPAddress.TryParse(addrString, out var address);
        }

        /// <summary>
        /// Tries to parse a port, returns true if successful, otherwise false
        /// The port will be set to whatever is parsed, otherwise the default port of k_NetworkPort
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static ushort ParsePortOrDefault(string s)
        {
            if (!ushort.TryParse(s, out var port))
            {
                Debug.LogWarning($"Unable to parse port, using default port {k_NetworkPort}");
                return k_NetworkPort;
            }

            return port;
        }
        
        private static void DestroyLocalSimulationWorld()
        {
            foreach (var world in World.All)
            {
                if (world.Flags == WorldFlags.Game)
                {
                    world.Dispose();
                    break;
                }
            }
        }
    }
}