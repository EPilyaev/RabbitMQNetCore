using System;
using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;

namespace PublisherConfirms
{
    internal static class Batch
    {
        public static void PublishMessagesInBatch()
        {
            var factory = new ConnectionFactory {HostName = "localhost"};
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var queueName = channel.QueueDeclare().QueueName;
            channel.ConfirmSelect();

            const int batchSize = 100;
            var outstandingMessageCount = 0;
            var timer = new Stopwatch();
            timer.Start();
            
            for (var i = 0; i < PublisherConfirms.MessageCount; i++)
            {
                var body = Encoding.UTF8.GetBytes(i.ToString());
                channel.BasicPublish("", queueName,
                                       null, body);
                outstandingMessageCount++;

                if (outstandingMessageCount != batchSize) continue;
                
                channel.WaitForConfirmsOrDie(new TimeSpan(0, 
                                                    0, 5));
                outstandingMessageCount = 0;
            }

            if (outstandingMessageCount > 0)
                channel.WaitForConfirmsOrDie(
                    new TimeSpan(0, 0, 5));

            timer.Stop();
            Console.WriteLine(
                $"Published {PublisherConfirms.MessageCount:N0} messages " +
                $"in batch in {timer.ElapsedMilliseconds:N0} ms");
        }
    }
}