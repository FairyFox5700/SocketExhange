using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Threading;  
using System.Text;
using Fairy.SocketClient;

public class AsyncSocketClient 
{
    private const int PortNumber = 8400;
    
    #region manual_reset_events
    private  ManualResetEvent ConnectionCompeted = new (false);  
    private  ManualResetEvent SendCompleted = new (false);  
    private ManualResetEvent ReceiveCompleted = new (false);  
    #endregion

    private  String Response = String.Empty;
    private  Socket _client;
    private readonly IPEndPoint _remoteEP;

    public AsyncSocketClient()
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        _remoteEP = EstablishConnection("localhost");
        _client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    }
    private IPEndPoint EstablishConnection(string dns)
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(dns);  
        IPAddress ipAddress = ipHostInfo.AddressList[0];  
        return new IPEndPoint(ipAddress, PortNumber);
    }
    public void StartClient(string dns)
    {
        try
        {
            int attempts = 0;
            while(!_client.Connected)
            {
                try
                {
                    attempts++;
                    _client.BeginConnect(_remoteEP, new AsyncCallback(ConnectCallback), _client);
                    ConnectionCompeted.WaitOne();
                }
                catch(SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Connection attempts: " + attempts.ToString());
                }
            }
            Console.Clear();
            Console.WriteLine("Connected to server");
        }
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }  
  
    public void StopAsync()
    {
        _client.Shutdown(SocketShutdown.Both);  
        _client.Close();  
    }
     
    public void StartReceiving()
    {
        Receive();
        ReceiveCompleted.WaitOne();
        Console.WriteLine("Response received : {0}", Response);
    }
    public void Send( String data) 
    {
        byte[] byteData = Encoding.ASCII.GetBytes(data);
        _client.BeginSend(byteData, 0, byteData.Length, 0, (SendCallback), _client);
        SendCompleted.WaitOne();
    }
    private void ConnectCallback(IAsyncResult ar) 
    {  
        try 
        {
            Socket client = (Socket) ar.AsyncState;
            client?.EndConnect(ar);
            Console.WriteLine("Socket connected to {0}", client?.RemoteEndPoint?.ToString());
            ConnectionCompeted.Set();  
        } 
        catch (Exception e) 
        {  
            Console.WriteLine(e.ToString());  
        }  
    }  

  
    public void Disconnect()
    {
        _client.Shutdown(SocketShutdown.Both);
        _client.Close();
    }

    public void Receive() 
    {  
        try 
        {
            StateObject state = new StateObject
            {
                WorkSocket = _client,
            };
            _client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
            ReceiveCompleted.WaitOne();
        }
        catch (Exception e) 
        {  
            Console.WriteLine(e.ToString());  
        }  
    }  

    private void ReceiveCallback( IAsyncResult ar ) 
    {  
        try 
        {
            StateObject state = (StateObject) ar.AsyncState;  
            Socket client = state.WorkSocket;  
  
            int bytesRead = client.EndReceive(ar);  
  
            if (bytesRead > 0) 
            {
                state.ClientString.Append(Encoding.ASCII.GetString(state.Buffer,0,bytesRead));
                client.BeginReceive(state.Buffer,0,StateObject.BufferSize,0, ReceiveCallback, state);  
            } 
            else 
            {
                if (state.ClientString.Length > 1) 
                {  
                    Response = state.ClientString.ToString();  
                }
                ReceiveCompleted.Set();  
            }  
        } 
        catch (Exception e) 
        {  
            Console.WriteLine(e.ToString());  
        }  
    }
    private void SendCallback(IAsyncResult ar) 
    {  
        try 
        {
            Socket client = (Socket) ar.AsyncState;
            int bytesSent = client.EndSend(ar);  
            Console.WriteLine("Sent {0} bytes to server.", bytesSent);
            SendCompleted.Set();  
        } 
        catch (Exception e) 
        {  
            Console.WriteLine(e.ToString());  
        }  
    }
}  