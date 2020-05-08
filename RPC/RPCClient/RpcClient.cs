using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RPCClient
{
    public class RpcClient
    {
        private const string QueueName = "rpc_queue";

        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> 
            _callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

        public RpcClient()
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _replyQueueName = _channel.QueueDeclare().QueueName;

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += OnReceived;
        }

        public Task<string> CallAsync(string message,
                        CancellationToken cancellationToken = default)
        {
            Console.WriteLine(" [x] Requesting fib({0})", message);
            
            var correlationId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<string>();
            _callbackMapper.TryAdd(correlationId, tcs);
            
            var props = _channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = _replyQueueName;
            
            var messageBytes = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish("", QueueName, props, messageBytes);
            _channel.BasicConsume(consumer: _consumer, queue: _replyQueueName, autoAck: true);

            cancellationToken.Register(() => 
                _callbackMapper.TryRemove(correlationId, out _));
            return tcs.Task;
        }

        public void Close() => _connection.Close();

        private void OnReceived(object model, BasicDeliverEventArgs ea)
        {
            var suchTaskExists = _callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, 
                                                                    out var tcs);
            
            if (!suchTaskExists) return;
            
            var body = ea.Body;
            var response = Encoding.UTF8.GetString(body);

            tcs.TrySetResult(response);
            
            Console.WriteLine(" [.] Got '{0}'", response);
        }
    }
}