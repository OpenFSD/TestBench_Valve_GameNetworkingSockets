//ClientAssembly
using System.Runtime.InteropServices;

namespace Florence.ServerAssembly
{
    static class Program
    {
        static void Main()
        {
            Florence.ClientAssembly.Networking.CreateNetworkingClient();
            //Valve.Sockets.Native.GameNetworkingSockets_Init();
        }

    }
}