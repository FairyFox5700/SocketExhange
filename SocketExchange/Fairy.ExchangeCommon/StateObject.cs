using System.Net.Sockets;
using System.Text;

namespace Fairy.SocketClient
{
    public class StateObject
    {
        public Socket WorkSocket = null;
        public const int BufferSize = 256;
        public byte[] Buffer = new byte[BufferSize];  
        public StringBuilder ClientString= new();  
    }
}