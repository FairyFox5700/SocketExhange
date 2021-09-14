using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Fairy.ConsoleSocketClient;
using Fairy.SocketClient;

namespace Fairy.SocketServer
{
    public class AsyncSocketServer
    {
        public static ManualResetEvent AllCompleted = new(false);
        public static ISonnetGenerator SonnetGenerator = new SonnetGenerator();
        private readonly IPAddress _ipAddress;
        private const int PortNumber = 8400;
        public AsyncSocketServer()
        {
            _ipAddress = GetLocalIP();
            ListenerSocket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        }

        public Socket ListenerSocket { get; set; }

        private IPAddress GetLocalIP()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            return  ipHostInfo.AddressList[0]; 
        }

        public void StartListening()
        {
            IPEndPoint localEndPoint = new IPEndPoint(_ipAddress, PortNumber);
            try
            {
                Console.WriteLine($"Listening started port:{PortNumber} protocol type: {ProtocolType.Tcp}");
                ListenerSocket.Bind(localEndPoint);
                ListenerSocket.Listen(100);
 
                while(true)
                {
                    AllCompleted.Reset();
                    Console.WriteLine("Waiting for a connection...");
                    ListenerSocket.BeginAccept( new AsyncCallback(AcceptCallback),ListenerSocket);
                    AllCompleted.WaitOne();
                }
 
            }
            catch(Exception e)
            {
                throw new Exception("listening error" + e.ToString());
            }
 
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            AllCompleted.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject();
            state.WorkSocket = handler;
            ListenerSocket.BeginAccept(AcceptCallback, ListenerSocket);
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            StateObject state = (StateObject) ar.AsyncState;
            Socket handler = state.WorkSocket;
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.ClientString.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));
                content = state.ClientString.ToString();
                if (content.ToUpper().Contains("SONNET"))
                {
                    Console.WriteLine("Received sonnet request from client. \n Data : {1}", content.Length, content);
                    Send(handler, SonnetGenerator.GenerateRandomShakespeareSonnet());
                }
                else if (content.ToUpper().Contains("<EOF>"))
                {
                    Console.WriteLine("Received end of file from client. \n Data : {1}", content.Length, content);
                    Send(handler, content);
                }
                else
                {
                    handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, callback: ReadCallback, state);
                }
            }
        }
      
        private static void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            handler.BeginSend(byteData, 0, byteData.Length, 0, callback: SendCallback, handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket) ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                handler.Shutdown(SocketShutdown.Send);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
