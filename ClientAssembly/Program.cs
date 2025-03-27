//ClientAssembly
using System.Runtime.InteropServices;
using System.Text;

namespace Florence.ServerAssembly
{
    static class Program
    {
        static private uint connection;
        static public Int16 input_a, input_b;
        static public Int32 output_answer;
        static private System.Threading.Thread thread_OutputRecieve = null;
        static public byte in_praiseEventId, out_praiseEventId;

        static void Main()
        {
            Florence.ClientAssembly.Networking.CreateNetworkingClient();
            //Valve.Sockets.Native.GameNetworkingSockets_Init();
            Valve.Sockets.Native.GameNetworkingSockets_Init((nint)Florence.ClientAssembly.Networking.Get_NetworkIdentity(), new StringBuilder(1024));

            thread_OutputRecieve = new System.Threading.Thread(Thread_OutputRecieve);
            thread_OutputRecieve.Start();

            Console.WriteLine("\npress any key to SIMULATE input.");
            Console.ReadKey();
            Thread_Input_SIMULATION();

            while(true)
            {

            }
        }

        static void Thread_Input_SIMULATION()
        {
            in_praiseEventId = 0;
            input_a = 1;
            input_b = 2;
            Florence.ClientAssembly.Networking.CreateAndSendNewMessage(in_praiseEventId);
        }

        static void Thread_OutputRecieve()
        {
            while (true)
            {
                Florence.ClientAssembly.Networking.CopyPayloadFromMessage();
                Console.WriteLine("Answer Recieved => " + Program.output_answer);
            }
        }

        public static uint Get_NetworkIdentity()
        {
            return connection;
        }
    }
}