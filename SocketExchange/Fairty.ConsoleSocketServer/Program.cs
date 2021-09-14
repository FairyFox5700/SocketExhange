using System;
using System.IO;
using System.Text.RegularExpressions;
using Fairy.SocketServer;

namespace Fairty.ConsoleSocketServer
{
   
    class Program
    {

        //  Клиент при обращении к серверу получает случайно выбранный сонет
        //Шекспира из файла.

        static void Main(string[] args)
        {
            var server = new AsyncSocketServer();
            server.StartListening();
        }

        
    }
}