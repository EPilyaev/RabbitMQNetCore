using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApiClient
{
    class Program
    {
        
        static HttpClient _client = new HttpClient(){Timeout = TimeSpan.FromSeconds(600)};
        
        static async Task Main(string[] args)
        { 
            Console.WriteLine("WebApi Client");
            string n = args.Length > 0 ? args[0] : "30";
            int requests = args.Length > 1 ? int.Parse(args[1]) : 40;
            
            var callTaskList = new List<Task>();
            
            var timer = new Stopwatch();
            timer.Start();
            
            for (var i = 0; i < requests; i++)
            {
                var callTask =  CallFibAsync(n);
                callTaskList.Add(callTask);
            }

            await Task.WhenAll(callTaskList);
            
            timer.Stop();
            Console.WriteLine($"Finished {requests} Tasks where n={n}\n" +
                              $"Elapsed: {timer.ElapsedMilliseconds:# ###} ms");
            

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        static async Task<int> CallFibAsync(string n)
        {
            Console.WriteLine($"Requesting fib({n})");

            var response = await _client.GetAsync($"https://localhost:5001/fib?n={n}");
            
            var result = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Got: {result}");
            
            return int.Parse(result);
        }
    }
}