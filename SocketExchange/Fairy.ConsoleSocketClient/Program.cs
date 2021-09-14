using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Fairy.ConsoleSocketClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Client started.");
            Console.WriteLine("Press the ENTER key to cancel...\n");
            var client = new AsyncSocketClient();
            var cts = new CancellationTokenSource();
            client.StartClient("localhost");
     
            while(true)
            {
                try
                {
                    Console.WriteLine("Enter a request: ");
                    string req = Console.ReadLine();
                    client.Send(req);
                    client.StartReceiving();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
 
            }
        }
            
        /* Task cancelTask = Task.Run(() =>
         {
             while (Console.ReadKey().Key != ConsoleKey.Enter)
             {
                 Console.WriteLine("Press the ENTER key to cancel...");
             }

             Console.WriteLine("\nENTER key pressed: cancelling downloads.\n");
             client.StopAsync();
             cts.Cancel();
         });
       
         await Task.WhenAll(new[] { cancelTask, clientTask, });*/
    }
}