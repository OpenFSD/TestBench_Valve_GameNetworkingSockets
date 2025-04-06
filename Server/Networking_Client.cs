using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Valve.Sockets;


namespace Valve
{
    public class Networking
    {
        static private Valve.Sockets.NetworkingIdentity identity;
        static private NetworkingSockets client = null;
        static private NetworkingMessage netMessage;
        
        public Networking()
        {
            netMessage = new NetworkingMessage();
        }
 
        static public void CreateNetworkingClient()
        {
            client = new NetworkingSockets();

            uint connection = 0;

            StatusCallback status = (ref StatusInfo info) => {
                switch (info.connectionInfo.state)
                {
                    case ConnectionState.None:
                        break;

                    case ConnectionState.Connected:
                        Console.WriteLine("Client connected to server - ID: " + connection);
                        break;

                    case ConnectionState.ClosedByPeer:
                    case ConnectionState.ProblemDetectedLocally:
                        client.CloseConnection(connection);
                        Console.WriteLine("Client disconnected from server");
                        break;
                }
            };

            Valve.Sockets.NetworkingUtils utils = new Valve.Sockets.NetworkingUtils();
            utils.SetStatusCallback(status);

            Address address = new Address();

            address.SetAddress(Valve.Networking.Get_Local_IPAddress(), 3074);

            connection = client.Connect(ref address);

#if VALVESOCKETS_SPAN
	MessageCallback message = (in NetworkingMessage netMessage) => {
		Console.WriteLine("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
	};
#else
            const int maxMessages = 20;

            NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
#endif

            while (!Console.KeyAvailable)
            {
                client.RunCallbacks();

#if VALVESOCKETS_SPAN
		client.ReceiveMessagesOnConnection(connection, message, 20);
#else
                int netMessagesCount = client.ReceiveMessagesOnConnection(connection, netMessages, maxMessages);

                if (netMessagesCount > 0)
                {
                    for (int i = 0; i < netMessagesCount; i++)
                    {
                        ref NetworkingMessage netMessage = ref netMessages[i];

                        Console.WriteLine("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

                        netMessage.Destroy();
                    }
                }
#endif

                Thread.Sleep(15);
            }
        }

        public static void CreateAndSendNewMessage(byte praiseEventId)
        {
            Console.WriteLine("entered => CreateAndSendNewMessage()");//ToDo
            byte[] data = new byte[64];
            data[0] = praiseEventId;

            switch (praiseEventId)
            {
            case 0:
   
                break;

            case 1:

                break;
            }

            Address address = new Address();
            address.SetAddress("192.168.8.2", 3074);
            uint connection = client.Connect(ref address);

            client.SendMessageToConnection(connection, data);//ToDo
        }

        public static void CopyPayloadFromMessage()
        {
            byte[] buffer = new byte[1024];
            netMessage.CopyTo(buffer);

            switch (buffer[0])
            {
            case 0:

                break;

            case 1:

                break;
            }
        }

        public static void SetA_HookForDebugInformation()
        {
            DebugCallback debug = (type, message) =>
            {
                Console.WriteLine("Debug - Type: " + type + ", Message: " + message);
            };

            NetworkingUtils utils = new NetworkingUtils();

            utils.SetDebugCallback(DebugType.Everything, debug);
        }

        public static ref Valve.Sockets.NetworkingIdentity Get_Identity()
        {
            return ref identity;
        }
        
        private static string Get_Local_IPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
