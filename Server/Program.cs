//ServerAssembly
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Florence.ServerAssembly
{
    static class Program
    {
        static public Int16 input_a, input_b;
        static public Int32 output_answer;
        private static bool outputReady = false;
        private static bool requestLaunchConcurrentThread = false;
        private static System.Threading.Thread thread_Concurrent = null;
        private static System.Threading.Thread thread_InputCapture = null;
        private static System.Threading.Thread thread_OutputSend = null;
        static public byte in_praiseEventId, out_praiseEventId;

        static void Main()
        {
            Florence.ServerAssembly.Networking.CreateNetworkingServer();

            Valve.Sockets.NetworkingIdentity networkingIdentity = new Valve.Sockets.NetworkingIdentity();
            Valve.Sockets.Native.GameNetworkingSockets_Init(ref networkingIdentity, new StringBuilder(1024));

            thread_Concurrent = new System.Threading.Thread(Thread_Concurrent);
            thread_Concurrent.Start();
            thread_InputCapture = new System.Threading.Thread(Thread_InputCapture);
            thread_InputCapture.Start();
            thread_OutputSend = new System.Threading.Thread(Thread_OutputSend);
            thread_OutputSend.Start();
            while (true)
            {
           
            }
        }

        static void Thread_Concurrent()
        {
            while (requestLaunchConcurrentThread == false)
            {
                
            }
            requestLaunchConcurrentThread = false;
            output_answer = input_a + input_b;
            outputReady = true;
        }

        static void Thread_InputCapture()
        {
            while (true)
            {
                Florence.ServerAssembly.Networking.CopyPayloadFromMessage();

                requestLaunchConcurrentThread = true;
            }
        }

        static void Thread_OutputSend()
        {
            while (outputReady == false)
            {
                
            }
            outputReady = false;
            Florence.ServerAssembly.Networking.CreateAndSendNewMessage(0);
        }

    }
}