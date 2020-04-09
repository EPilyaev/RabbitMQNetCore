using System;
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

            Console.WriteLine(" [x] Requesting fib({0})", n);
            var response = await rpcClient.CallAsync(n);
            Console.WriteLine(" [.] Got '{0}'", response);

            rpcClient.Close();

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}