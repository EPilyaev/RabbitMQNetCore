using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ReceiveLogsDirect
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class ReceiveLogsDirect
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare("direct_logs", ExchangeType.Direct);
            var queueName = channel.QueueDeclare().QueueName;

            if (CheckInvalidArgsCount(args)) return;

            foreach(var severity in args)
            {
                channel.QueueBind(queueName, "direct_logs", severity); 
            }

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += OnConsumerReceived;
            channel.BasicConsume(queueName, true, consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static void OnConsumerReceived(object? model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;
            Console.WriteLine(" [x] Received '{0}':'{1}'", routingKey, message);
        }

        private static bool CheckInvalidArgsCount(string[] args)
        {
            if (args.Length >= 1) return false;
            
            Console.Error.WriteLine("Usage: {0} [info] [warning] [error]",
                Environment.GetCommandLineArgs()[0]);
            
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
            Environment.ExitCode = 1;
            return true;
        }
    }
}