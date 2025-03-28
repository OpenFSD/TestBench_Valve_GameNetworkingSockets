using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Florence.ServerAssembly
{
    public class Networking
    {
        static private List<nint> clientsToSendTo = new List<nint>(1);
        static private nint connection;
        static private Valve.Sockets.NetworkingSockets server = null;
        static private Valve.Sockets.NetworkingMessage netMessage;

        public Networking()
        {

        }

        static public void CreateNetworkingServer()
        {
            Valve.Sockets.NetworkingSockets server = new Valve.Sockets.NetworkingSockets();

            uint pollGroup = server.CreatePollGroup();

            Valve.Sockets.StatusCallback status = (ref Valve.Sockets.StatusInfo info) =>
            {
                switch (info.connectionInfo.state)
                {
                    case Valve.Sockets.ConnectionState.None:
                        break;

                    case Valve.Sockets.ConnectionState.Connecting:
                        server.AcceptConnection(info.connection);
                        server.SetConnectionPollGroup(pollGroup, info.connection);
                        break;

                    case Valve.Sockets.ConnectionState.Connected:
                        Console.WriteLine("Client connected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                        break;

                    case Valve.Sockets.ConnectionState.ClosedByPeer:
                    case Valve.Sockets.ConnectionState.ProblemDetectedLocally:
                        server.CloseConnection(info.connection);
                        Console.WriteLine("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                        break;
                }
            };

            Valve.Sockets.NetworkingUtils utils = new Valve.Sockets.NetworkingUtils();
            utils.SetStatusCallback(status);

            Valve.Sockets.Address address = new Valve.Sockets.Address();

            address.SetAddress(Florence.ServerAssembly.Networking.Get_Local_IPAddress(), 3074);//ToDo

            uint listenSocket = server.CreateListenSocket(ref address);

#if VALVESOCKETS_SPAN
	        MessageCallback message = (in NetworkingMessage netMessage) => {
		        Console.WriteLine("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
	        };
#else
            const int maxMessages = 20;

            Valve.Sockets.NetworkingMessage[] netMessages = new Valve.Sockets.NetworkingMessage[maxMessages];
#endif

            while (!Console.KeyAvailable)
            {
                server.RunCallbacks();

#if VALVESOCKETS_SPAN
		        server.ReceiveMessagesOnPollGroup(pollGroup, message, 20);
#else
                int netMessagesCount = server.ReceiveMessagesOnPollGroup(pollGroup, netMessages, maxMessages);

                if (netMessagesCount > 0)
                {
                    for (int i = 0; i < netMessagesCount; i++)
                    {
                        ref Valve.Sockets.NetworkingMessage netMessage = ref netMessages[i];

                        Console.WriteLine("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

                        netMessage.Destroy();
                    }
                }
#endif

                Thread.Sleep(15);
            }
            server.DestroyPollGroup(pollGroup);
        }

        public static void CreateAndSendNewMessage(byte praiseEventId)
        {
            Console.WriteLine("entered => CreateAndSendNewMessage()");//ToDo
            byte[] data = new byte[64];
            data[0] = praiseEventId;
            switch (praiseEventId)
            {
                case 0:
                    byte[] bytes = new byte[4];
                    BinaryPrimitives.WriteInt32BigEndian(bytes, Florence.ServerAssembly.Program.output_answer);
                    for (byte index = 1; index < 5; index++)
                    {
                        data[index] = bytes[index];
                    }
                    break;

                case 1:

                    break;

                default:
                    break;

            }
            server.SendMessageToConnection((uint)clientsToSendTo.IndexOf(0), data);
        }

        public static void CopyPayloadFromMessage()
        {
            byte[] buffer = new byte[1024];
            netMessage.CopyTo(buffer);
            
            Florence.ServerAssembly.Program.out_praiseEventId = buffer[0];
            switch (buffer[0])
            {
                case 0:
                    clientsToSendTo.Insert(0, BitConverter.ToInt32(buffer, 1));
                    Program.input_a = BitConverter.ToInt16(buffer, 5);
                    Program.input_b = BitConverter.ToInt16(buffer, 7);
                    break;

                case 1:
                    //ToDo
                    break;

                default:
                    break;
            }
        }

        public static void SetA_HookForDebugInformation()
        {
            Valve.Sockets.DebugCallback debug = (type, message) =>
            {
                Console.WriteLine("Debug - Type: " + type + ", Message: " + message);
            };

            Valve.Sockets.NetworkingUtils utils = new Valve.Sockets.NetworkingUtils();

            utils.SetDebugCallback(Valve.Sockets.DebugType.Everything, debug);
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

        public static nint Get_NetworkIdentity()
        {
            return connection;
        }
    }
}
