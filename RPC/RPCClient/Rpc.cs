using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RPCClient
{
    public static class Rpc
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("RPC Client");
            string n = args.Length > 0 ? args[0] : "30";

            var rpcClient = new RpcClient();
            
            var callTaskList = new List<Task>();
            
            var timer = new Stopwatch();
            timer.Start();
            
            for (var i = 0; i < 40; i++)
            {
                var callTask =  rpcClient.CallAsync(n);
                callTaskList.Add(callTask);
            }

            await Task.WhenAll(callTaskList);
            
            timer.Stop();
            Console.WriteLine($"Finished 40 Tasks where n={n}\n" +
                              $"Elapsed: {timer.ElapsedMilliseconds:# ###} ms");
            
            rpcClient.Close();

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}