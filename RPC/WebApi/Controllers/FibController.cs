using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FibController : ControllerBase
    {
        [HttpGet]
        public int Calculate(int n = 30)
        {
            return Fib(n);
        }
        
        /// <summary>
        /// Assumes only valid positive integer input.
        /// Don't expect this one to work for big numbers, and it's probably the slowest recursive implementation possible.
        /// Input more than 45 is nonsensical
        /// </summary>
        private static int Fib(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }

            return Fib(n - 1) + Fib(n - 2);
        }
    }
}