using Unity.NetCode;
using Unity.Networking.Transport;

namespace Dots.Racing
{
    [UnityEngine.Scripting.Preserve]
    public class NetcodeBootstrap : ClientServerBootstrap
    {
        public override bool Initialize(string defaultWorldName)
        {
            AutoConnectPort = 7979; // Enable auto connect
// Todo: remove define and have option in the main menu for server connection
#if VM_NETWORKENDPOINT
DefaultConnectAddress = NetworkEndpoint.Parse("128.14.159.58", AutoConnectPort);
#endif
            return base.Initialize(defaultWorldName); // Use the regular bootstrap
        }
    }
}

// // The preserve attribute is required to make sure the bootstrap is not stripped in il2cpp builds with stripping enabled
// [UnityEngine.Scripting.Preserve]
// // The bootstrap needs to extend `ClientServerBootstrap`, there can only be one class extending it in the project
// public class NetCodeBootstrap : ClientServerBootstrap
// {
//     // The initialize method is what entities calls to create the default worlds
//     public override bool Initialize(string defaultWorldName)
//     {
//         var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
//         var isFrontend = sceneName == "MainMenu";
//         
//         //Let the user decide when to connect 
//         if (isFrontend)
//         {
//             CreateLocalWorld(defaultWorldName);
//             return true;
//         }
//         Debug.Log("NetCodeBootstrap");
//         AutoConnectPort = 7979;
//         CreateDefaultClientServerWorlds();
//         return true;
//     }
// }