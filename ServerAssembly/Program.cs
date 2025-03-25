//ServerAssembly
using System.Runtime.InteropServices;

namespace Florence.ServerAssembly
{
    static class Program
    {
        static void Main()
        {
            Florence.ServerAssembly.Networking.CreateNetworkingServer();
            //Valve.Sockets.Native.GameNetworkingSockets_Init();
        }

    }
}