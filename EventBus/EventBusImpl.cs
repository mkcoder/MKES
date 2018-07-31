using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MKES.Attributes;
using MKES.Interfaces;
using MKES.Model;
using MKES.Extensions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MKES.EventBus
{
    public class EventBusImpl : IEventBus
    {
        public ConnectionFactory Factory { get; } = new ConnectionFactory() { HostName = "localhost", DispatchConsumersAsync = true};
        private readonly IConnection connection;
        private readonly IModel channel;
        private Dictionary<Type, QueueDeclareOk> queueDeclare = new Dictionary<Type, QueueDeclareOk>();

        public EventBusImpl()
        {
            connection = Factory.CreateConnection();
            channel = connection.CreateModel();
        }
        
        public void Send(Command command)
        {
            var queueInfo = ((CommandInfo)command.GetType().GetCustomAttribute(typeof(CommandInfo)));
            if (queueInfo == null) throw new Exception("A Command must have an CommandInfo attribute");
            var queueName = queueInfo.Name;
            var routingKey = queueInfo.RoutingKey;

            Publish(JsonConvert.SerializeObject(command), queueName, routingKey, command.GetType());
        }

        public IConnectionFactory GetConnectionFactory() => Factory;
        

        public void Publish(Event @event)
        {
            var queueInfo = ((QueueInfo)@event.GetType().GetCustomAttribute(typeof(QueueInfo)));
            if (queueInfo == null) throw new Exception("A Event must have an QueueInfo attribute");
            var queueName = queueInfo.Name;
            var routingKey = queueInfo.RoutingKey;

            Publish(JsonConvert.SerializeObject(@event), queueName, routingKey, @event.GetType());
        }

        public void RegisterAsync(ListenerInfo queueInfo, AsyncEventHandler<BasicDeliverEventArgs> consumerOnReceived)
        {

            string jsonResult = String.Empty;
            var ouOk = channel.QueueDeclare(queue: queueInfo.RoutingKey, durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            // DEBUG: Console.WriteLine($"Read QN: {ouOk.QueueName} CC: {ouOk.ConsumerCount} MC: {ouOk.MessageCount}");
            consumer.Received += consumerOnReceived;
            channel.BasicConsume(queue: queueInfo.RoutingKey, autoAck: true, consumer: consumer);
        }

        public void Register<T>(ListenerInfo queueInfo, Func<T, Task> callback)
        {
            var connection = Factory.CreateConnection();
            var channel = connection.CreateModel();
            
            string jsonResult = String.Empty;
            var ouOk = channel.QueueDeclare(queue: queueInfo.QueueName, durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            Console.WriteLine($"Read QN: {ouOk.QueueName} CC: {ouOk.ConsumerCount} MC: {ouOk.MessageCount}");

            consumer.Received += async (sender, @event) =>
            {
                jsonResult = Encoding.UTF8.GetString(@event.Body);
                T t = JsonConvert.DeserializeObject<T>(jsonResult);
                await callback.Invoke(t);
            };
            
            
            channel.BasicConsume(queue: queueInfo.RoutingKey, autoAck: true, consumer: consumer);
        }

        public async Task<T> ReadAsync<T>(ListenerInfo queueInfo)
        {
            using (var connection = Factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                string jsonResult = String.Empty;
                var ouOk = channel.QueueDeclare(queue: queueInfo.QueueName, durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new AsyncEventingBasicConsumer(channel);
                Console.WriteLine($"Read QN: {ouOk.QueueName} CC: {ouOk.ConsumerCount} MC: {ouOk.MessageCount}");

                var data = await Task.Run(() => channel.BasicGet(queueInfo.RoutingKey, true)).ConfigureAwait(false);
                channel.BasicConsume(queue: queueInfo.RoutingKey, autoAck: true, consumer: consumer);
                if(data!=null)
                    return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data.Body));                
            }

            return default(T);
        }

        private void Publish(string message, string queueName, string routingKey, Type type)
        {
            using (var connection = Factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var ouOk = GetQueueDeclareOkFromType(type, queueName, channel);
                var body = Encoding.UTF8.GetBytes(message);
                //Console.WriteLine($"Publishing....{ouOk} {body} {message} {channel} {connection}");

                channel.BasicPublish(exchange: "", routingKey: routingKey, basicProperties: null, body: body);
            }
        }

        private QueueDeclareOk GetQueueDeclareOkFromType(Type type, string queueName, IModel channel)
        {
            if(queueDeclare.ContainsKey(type).Not())
            {
                queueDeclare[type] =
                    channel.QueueDeclare(queue: queueName, durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
            }

            return queueDeclare[type];
        }
    }
}
