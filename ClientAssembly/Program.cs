﻿//ClientAssembly
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
            Console.WriteLine("TestBench SIMULATION started");//ToDo TestBench
            Florence.ClientAssembly.Networking.CreateNetworkingClient();
            Console.WriteLine("created => NetworkingClient");//ToDo TestBench

            IntPtr intptr = Marshal.StringToHGlobalUni(Florence.ClientAssembly.Networking.Get_client_NetworkingSockets().ToString());
            Marshal.FreeHGlobal(intptr);
            Valve.Sockets.Native.GameNetworkingSockets_Init(intptr, new StringBuilder(1024));
            Console.WriteLine("completed => Initialise(IntPtr)");//ToDo TestBench
            Valve.Sockets.Native.GameNetworkingSockets_Init((nint)Florence.ClientAssembly.Networking.Get_NetworkIdentity(), new StringBuilder(1024));
            Console.WriteLine("completed => Initialise(NetworkIdentity)");//ToDo TestBench

            thread_OutputRecieve = new System.Threading.Thread(Thread_OutputRecieve);
            Console.WriteLine("created => Thread: to scan for signal from server");//ToDo TestBench
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
            Console.WriteLine("started => Concurrent Thread: Thread_Input_SIMULATION()");//ToDo TestBench
            in_praiseEventId = 0;
            input_a = 1;
            input_b = 2;
            Florence.ClientAssembly.Networking.CreateAndSendNewMessage(in_praiseEventId);
        }

        static void Thread_OutputRecieve()
        {
            Console.WriteLine("entered => Thread_OutputRecieve()");//ToDo TestBench
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