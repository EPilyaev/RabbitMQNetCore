using System;
using System.Text;
using RabbitMQ.Client;

namespace EmitLog
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class EmitLog
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare("logs", ExchangeType.Fanout);

            var message = GetMessage(args);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish("logs", "", null, body);
            Console.WriteLine(" [x] Sent {0}", message);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static string GetMessage(string[] args) =>
            args.Length > 0 ? string.Join(" ", args) : "info: Hello World!";
    }
}