using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MKES.Attributes;
using MKES.EventBus;
using MKES.Model;
using Newtonsoft.Json;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MKES.EventStore.Tests
{
    internal class EventBusTest
    {
        private EventBusImpl _eventBusImpl = new EventBusImpl();

        [CommandInfo(name: "Test-Queue", routingKey: "Test-Queue")]
        public class TestCommand : Command
        {
            public string Message { get; set; }
        }

        [QueueInfo(name: "Test-Queue", routingKey: "Test-Queue")]
        public class TestEvent : Event
        {
            public string Message { get; set; }
        }

        [Test]
        public void Send_Command_Works()
        {
            _eventBusImpl.Send(new TestCommand() {AggregateId = Guid.NewGuid(), Version = 1, Message = "This is a test command"});
        }

        private static Task Arg(object sender, ConsumerEventArgs @event)
        {
            Console.WriteLine("sender");
            return Task.Delay(1999);
        }

        [Test]
        public void Publish_Event_Works()
        {
            _eventBusImpl.Publish(new TestEvent() { AggregateId = Guid.NewGuid(), Version = 1, Message = "This is a test command" });
        }

        [Test]
        public void Read_Commands_Back()
        {
            _eventBusImpl.Register<TestCommand>(ListenerInfo.FromCommand(typeof(TestCommand)), Callback);
            for (int j = 0; j < 3; j++)
            {
                var message = "Message Id: " + j + " " + RandomMessageGenerator();
                _eventBusImpl.Send(new TestCommand() {AggregateId = Guid.NewGuid(), Version = 1, Message = message});
            }

            Thread.Sleep(5000);
        }

        private async Task Callback(TestCommand testCommand)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"Testcommand [{testCommand.Version} {testCommand.AggregateId}]. " +
                                  $"{testCommand.Message.Substring(0, 100)}...");

                Task.Delay(1000);
            });
        }

        [Test]
        public async Task ReadAsync_Commands_Back()
        {
            for (int j = 0; j < 3; j++)
            {
                var message = "Message Id: " + j + " " + RandomMessageGenerator();
                _eventBusImpl.Send(new TestCommand() { AggregateId = Guid.NewGuid(), Version = j, Message = message });
                TestCommand testCommand = await _eventBusImpl.ReadAsync<TestCommand>(ListenerInfo.FromCommand(typeof(TestCommand)));
                Console.WriteLine($"Testcommand [{testCommand.Version} {testCommand.AggregateId}]. " +
                                  $"{testCommand.Message.Substring(0, 100)}...");
            }            
            Thread.Sleep(5000);
        }

        [Test]
        public async Task ReadAsyncLong_Commands_Back()
        {
            var factory = _eventBusImpl.Factory;
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var queueInfo = ListenerInfo.FromCommand(typeof(TestCommand));

            string jsonResult = String.Empty;
            var ouOk = channel.QueueDeclare(queue: queueInfo.QueueName, durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            Console.WriteLine($"Read QN: {ouOk.QueueName} CC: {ouOk.ConsumerCount} MC: {ouOk.MessageCount}");

            consumer.Received += ConsumerOnReceived;
            
            for (int j = 0; j < 3; j++)
            {
                var message = "Message Id: " + j + " " + RandomMessageGenerator();
                _eventBusImpl.Send(new TestCommand() { AggregateId = Guid.NewGuid(), Version = j, Message = message });
                channel.BasicConsume(queue: queueInfo.RoutingKey, autoAck: true, consumer: consumer);
            }
            Thread.Sleep(5000);
        }

        private async Task ConsumerOnReceived(object sender, BasicDeliverEventArgs @event)
        {
            var testCommand = JsonConvert.DeserializeObject<TestCommand>(Encoding.UTF8.GetString(@event.Body));
            Console.WriteLine($"Testcommand [{testCommand.Version} {testCommand.AggregateId}]. " +
                              $"{testCommand.Message.Substring(0, 100)}...");
            await Task.Run(() => testCommand);
        }

        private string RandomMessageGenerator()
        {
            string[] words =
                @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut vitae varius diam. Vestibulum dolor elit, ornare nec lectus at, mattis sagittis erat. Ut ornare faucibus mi consectetur tempus. Cras eu elit id libero varius pulvinar. Sed malesuada non est vitae dignissim. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Quisque id sem venenatis, molestie urna vitae, pulvinar sapien. Aliquam tempus ultricies nisl sit amet elementum. Suspendisse viverra viverra arcu non tempor.

Aliquam a ligula sit amet dui vulputate pellentesque. Donec egestas diam ex, fermentum pharetra enim pharetra sed. Suspendisse fringilla egestas enim. Sed fringilla, massa et porttitor congue, quam ante convallis risus, vitae tempus ipsum nibh sit amet dui. Vestibulum laoreet at elit non posuere. Quisque felis elit, blandit id posuere nec, scelerisque et augue. Donec id aliquam nisi, vel hendrerit nisl. Nam maximus a tellus id imperdiet. Suspendisse commodo, ipsum eget gravida ultrices, leo orci efficitur dui, ac laoreet ex elit vel ligula. Sed quis tempor ligula, a ornare quam. Phasellus bibendum semper ante, imperdiet tempus augue aliquet vel.

Aenean quis felis vulputate, posuere felis facilisis, malesuada nunc. Donec rhoncus tortor nec ultrices efficitur. Curabitur imperdiet dolor non feugiat aliquam. Proin euismod accumsan augue eu vestibulum. Etiam commodo nec dolor in scelerisque. Vestibulum pellentesque arcu sit amet arcu tincidunt egestas eu efficitur metus. Duis pretium massa eu lacinia eleifend. Suspendisse vel lacus elit. Suspendisse potenti. Donec ex sem, tempus volutpat pharetra vel, sollicitudin ac enim. Donec non condimentum purus, in rutrum nulla. Aenean maximus, ex vel convallis pulvinar, lacus justo pulvinar nulla, consequat euismod tortor ante eu erat. Pellentesque augue dui, faucibus vitae magna vel, ultrices placerat augue.

Praesent ullamcorper lacus ipsum, eu auctor sem ullamcorper vitae. Ut quis risus dui. Donec posuere a lorem commodo mattis. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Duis eget elementum turpis, quis ullamcorper libero. Aenean elementum ex turpis, ut aliquet velit rutrum eu. Cras varius justo at enim bibendum, vitae dapibus neque volutpat. In felis nisi, blandit sed nulla in, feugiat ullamcorper diam. Mauris hendrerit sagittis dolor sit amet iaculis. In non bibendum nulla, eget tristique erat.

Nunc ex urna, venenatis sed convallis eu, commodo eu urna. Quisque cursus sit amet velit imperdiet ultrices. Nullam ac ligula in tellus eleifend tempor. Pellentesque rutrum vulputate lectus, ac scelerisque elit aliquam ac. Phasellus sem nulla, commodo id urna et, commodo tempor enim. Ut.".Split(" ");
            int sentenceLength = new Random().Next(10, words.Length-1);
            return String.Join(" ", words.Take(sentenceLength));
        }
    }
}