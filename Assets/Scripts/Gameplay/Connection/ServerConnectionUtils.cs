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
        public static string LastConnectionError = string.Empty;

        /// <summary>
        ///     Start a Client and Server in your local IP
        /// </summary>
        public static void StartClientServer(string port)
        {
            IsTryingToConnect = true;
            LastConnectionError = string.Empty;
            
            RaceLogger.LogSection("NETWORK CONNECTION");
            RaceLogger.Network("Starting Client/Server mode");
            
            if (ClientServerBootstrap.RequestedPlayType != ClientServerBootstrap.PlayType.ClientAndServer)
            {
                LastConnectionError = $"Creating client/server worlds is not allowed if playmode is set to {ClientServerBootstrap.RequestedPlayType}";
                Debug.LogError(LastConnectionError);
                RaceLogger.Error(LastConnectionError);
                IsTryingToConnect = false;
                return;
            }

            if (m_Server != null && m_Server.IsCreated)
                m_Server.Dispose();

            if (m_Client != null && m_Client.IsCreated)
                m_Client.Dispose();

            RaceLogger.Network("Creating server and client worlds");
            m_Server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
            m_Client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

            // Destroy the local simulation world safely
            SafeDestroyLocalSimulationWorld();

            World.DefaultGameObjectInjectionWorld ??= m_Server;

            RaceLogger.Network("Loading main scene");
            SceneLoader.Instance.LoadScene(SceneType.Main);

            try
            {
                var parsedPort = ParsePortOrDefault(port);
                RaceLogger.Network($"Starting server on port {parsedPort}");
                var networkEndpoint = NetworkEndpoint.AnyIpv4.WithPort(parsedPort);
                {
                    using var drvQuery = m_Server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                    drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(networkEndpoint);
                }

                RaceLogger.Network($"Connecting client to loopback:{parsedPort}");
                networkEndpoint = NetworkEndpoint.LoopbackIpv4.WithPort(parsedPort);
                {
                    using var drvQuery = m_Client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                    drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(m_Client.EntityManager, networkEndpoint);
                }
                
                RaceLogger.Success("Client/Server setup completed successfully");
            }
            catch (System.Exception ex)
            {
                LastConnectionError = $"Failed to start client/server: {ex.Message}";
                Debug.LogError(LastConnectionError);
                RaceLogger.Error(LastConnectionError);
                IsTryingToConnect = false;
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
            LastConnectionError = string.Empty;
            
            RaceLogger.LogSection("NETWORK CONNECTION");
            RaceLogger.Network($"Connecting to server at {ip}:{port}");
            
            if (!ValidateIPv4(ip))
            {
                LastConnectionError = $"Invalid IP address format: {ip}";
                Debug.LogError(LastConnectionError);
                RaceLogger.Error(LastConnectionError);
                IsTryingToConnect = false;
                return;
            }
            
            RaceLogger.Network("Creating client world");
            var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
            
            // Destroy the local simulation world safely
            SafeDestroyLocalSimulationWorld();
            
            World.DefaultGameObjectInjectionWorld ??= client;
            
            RaceLogger.Network("Loading main scene");
            SceneLoader.Instance.LoadScene(SceneType.Main);
            
            try
            {
                var parsedPort = ParsePortOrDefault(port);
                RaceLogger.Network($"Connecting to {ip}:{parsedPort}");
                var networkEndpoint = NetworkEndpoint.Parse(ip, parsedPort);
                {
                    using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                    drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, networkEndpoint);
                }
                
                RaceLogger.Success("Client connection initiated successfully");
            }
            catch (System.Exception ex)
            {
                LastConnectionError = $"Failed to connect to server: {ex.Message}";
                Debug.LogError(LastConnectionError);
                RaceLogger.Error(LastConnectionError);
                IsTryingToConnect = false;
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
        
        /// <summary>
        /// Safely destroys the local simulation world if it exists
        /// </summary>
        private static void SafeDestroyLocalSimulationWorld()
        {
            try
            {
                foreach (var world in World.All)
                {
                    if (world.Flags == WorldFlags.Game && world.IsCreated)
                    {
                        world.Dispose();
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Error disposing local simulation world: {ex.Message}");
            }
        }
    }
}