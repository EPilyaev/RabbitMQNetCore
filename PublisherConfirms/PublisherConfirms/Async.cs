using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using RabbitMQ.Client;

namespace PublisherConfirms
{
    internal static class Async
    {
        public static void HandlePublishConfirmsAsynchronously()
        {
            using var connection = new ConnectionFactory {HostName = "localhost"}.CreateConnection();
            using var channel = connection.CreateModel();
            var queueName = channel.QueueDeclare().QueueName;
            channel.ConfirmSelect();

            var outstandingConfirms = new ConcurrentDictionary<ulong, string>();

            void CleanOutstandingConfirms(ulong sequenceNumber, bool multiple)
            {
                if (multiple)
                {
                    var confirmed = outstandingConfirms.Where(k => k.Key <= sequenceNumber);
                    foreach (var entry in confirmed)
                        outstandingConfirms.TryRemove(entry.Key, out _);
                }
                else
                    outstandingConfirms.TryRemove(sequenceNumber, out _);
            }

            channel.BasicAcks += (sender, ea) => CleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
            channel.BasicNacks += (sender, ea) =>
            {
                outstandingConfirms.TryGetValue(ea.DeliveryTag, out string body);
                Console.WriteLine(
                    $"Message with body {body} has been nack-ed. Sequence number: {ea.DeliveryTag}, multiple: {ea.Multiple}");
                CleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
            };

            var timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < PublisherConfirms.MessageCount; i++)
            {
                var body = i.ToString();
                outstandingConfirms.TryAdd(channel.NextPublishSeqNo, i.ToString());
                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null,
                    body: Encoding.UTF8.GetBytes(body));
            }

            if (!WaitUntil(60, () => outstandingConfirms.IsEmpty))
                throw new Exception("All messages could not be confirmed in 60 seconds");

            timer.Stop();
            Console.WriteLine(
                $"Published {PublisherConfirms.MessageCount:N0} messages and handled confirm asynchronously {timer.ElapsedMilliseconds:N0} ms");
        }

        private static bool WaitUntil(int numberOfSeconds, Func<bool> condition)
        {
            int waited = 0;
            while (!condition() && waited < numberOfSeconds * 1000)
            {
                Thread.Sleep(100);
                waited += 100;
            }

            return condition();
        }
    }
}