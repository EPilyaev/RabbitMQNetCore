using System;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace EmitLogTopic
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class EmitLogTopic
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            channel.ExchangeDeclare("topic_logs", ExchangeType.Topic);

            var routingKey = args.Length > 0 ? args[0] : "anonymous.info";
            var message = args.Length > 1
                ? string.Join(" ", args.Skip( 1 ).ToArray())
                : "Hello World!";
            var body = Encoding.UTF8.GetBytes(message);
            
            channel.BasicPublish("topic_logs", routingKey, null, body);
            Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, message);
        }
    }
}