using System;
using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;

namespace PublisherConfirms
{
    internal static class Individual
    {
        public static void PublishMessagesIndividually()
        {
            var factory = new ConnectionFactory {HostName = "localhost"};
            
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            var queueName = channel.QueueDeclare().QueueName;
            channel.ConfirmSelect();

            var timer = new Stopwatch();
            timer.Start();
            
            for (var i = 0; i < PublisherConfirms.MessageCount; i++)
            {
                var body = Encoding.UTF8.GetBytes(i.ToString());
                channel.BasicPublish("", queueName, 
                                    null, body);
                
                channel.WaitForConfirmsOrDie(new TimeSpan(0, 
                                                    0, 5));
            }

            timer.Stop();
            Console.WriteLine(
                $"Published {PublisherConfirms.MessageCount:N0} " +
                $"messages individually in {timer.ElapsedMilliseconds:N0} ms");
        }
    }
}