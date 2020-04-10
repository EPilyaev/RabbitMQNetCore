using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RPCServer
{
    static class RpcServer
    {
        private static IModel _channel;

        public static void Main()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                //Following is required to use AsyncEventingBasicConsumer
                DispatchConsumersAsync = true
            };

            using var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            _channel.QueueDeclare("rpc_queue", false, false, 
                                            false, null);
            _channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += OnReceived;

            _channel.BasicConsume("rpc_queue", false, consumer);

            Console.WriteLine(" [x] Awaiting RPC requests");
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static async Task OnReceived(object model, BasicDeliverEventArgs ea)
        {
            string response = null;

            var body = ea.Body;
            var props = ea.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            try
            {
                var message = Encoding.UTF8.GetString(body);
                var n = int.Parse(message);
                Console.WriteLine(" [.] fib({0})", message);
                response = await Task.Run(() => Fib(n).ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(" [.] " + e.Message);
                response = "";
            }
            finally
            {
                var responseBytes = Encoding.UTF8.GetBytes(response);
                
                _channel.BasicPublish("", props.ReplyTo,
                                            replyProps, responseBytes);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
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