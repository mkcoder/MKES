using System;
using System.Threading.Tasks;
using MKES.EventBus;
using MKES.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MKES.Interfaces
{
    public interface IEventBus
    {
        void Publish(Event @event);
        void Register<T>(ListenerInfo queueInfo, Func<T, Task> callback);
        void RegisterAsync(ListenerInfo queueInfo, AsyncEventHandler<BasicDeliverEventArgs> consumerOnReceived);
        Task<T> ReadAsync<T>(ListenerInfo queueInfo);
        void Send(Command command);
        IConnectionFactory GetConnectionFactory();
    }
}