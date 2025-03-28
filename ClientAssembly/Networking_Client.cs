using Florence.ServerAssembly;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Valve.Sockets;


namespace Florence.ClientAssembly
{
    public class Networking
    {
        static private uint connection = 0;
        static private NetworkingSockets client = null;
        static private Valve.Sockets.NetworkingMessage netMessage;

        public Networking()
        {

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
                        CopyPayloadFromMessage();
                        break;

                    case ConnectionState.ClosedByPeer:
                    case ConnectionState.ProblemDetectedLocally:
                        client.CloseConnection(connection);
                        Console.WriteLine("Client disconnected from server");
                        break;
                }
            };

            NetworkingUtils utils = new NetworkingUtils();
            utils.SetStatusCallback(status);

            Address address = new Address();

            address.SetAddress(Florence.ClientAssembly.Networking.Get_Local_IPAddress(), 3074);//ToDo

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
                        CopyPayloadFromMessage();//added

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
                    byte[] bytes = new byte[4];
                    BinaryPrimitives.WriteUInt32BigEndian(bytes, Florence.ServerAssembly.Program.Get_NetworkIdentity());
                    for (byte index = 1; index < 5; index++)
                    {
                        data[index] = bytes[index];
                    }
                    bytes = new byte[2];
                    BinaryPrimitives.WriteInt16BigEndian(bytes, Florence.ServerAssembly.Program.input_a);
                    for (byte index = 5; index < 7; index++)
                    {
                        data[index] = bytes[index];
                    }
                    bytes = new byte[2];
                    BinaryPrimitives.WriteInt16BigEndian(bytes, Florence.ServerAssembly.Program.input_b);
                    for (byte index = 7; index < 9; index++)
                    {
                        data[index] = bytes[index];
                    }
                    break;

                case 1:
                    
                    break;
            }
            client.SendMessageToConnection(connection, data);
        }

        public static void CopyPayloadFromMessage()
        {
            byte[] buffer = new byte[1024];
            netMessage.CopyTo(buffer);

            Florence.ServerAssembly.Program.out_praiseEventId = buffer[0];
            switch (buffer[0])
            {
                case 0:
                    Program.output_answer = BitConverter.ToInt32(buffer, 1);
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
            DebugCallback debug = (type, message) =>
            {
                Console.WriteLine("Debug - Type: " + type + ", Message: " + message);
            };

            NetworkingUtils utils = new NetworkingUtils();

            utils.SetDebugCallback(DebugType.Everything, debug);
        }

        public static NetworkingSockets Get_client_NetworkingSockets()
        {
            return client;
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

        public static uint Get_NetworkIdentity()
        {
            return connection;
        }
    }
}