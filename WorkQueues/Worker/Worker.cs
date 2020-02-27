using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Worker
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class Worker
    {
        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            channel.QueueDeclare("task_queue", true, false,
                false, null);

            channel.BasicQos(0, 1, false);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += OnReceived;
            channel.BasicConsume("task_queue", false, consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
            
            
            void OnReceived(object model, BasicDeliverEventArgs ea)
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                
                Console.WriteLine(" [x] Received {0}", message);
                DoHardWork(message);
                Console.WriteLine(" [x] Done");

                channel.BasicAck(ea.DeliveryTag, false);
            }
        }

        private static void DoHardWork(string message)
        {
            int dots = message.Split('.').Length - 1;
            Thread.Sleep(dots * 1000);
        }
    }
}