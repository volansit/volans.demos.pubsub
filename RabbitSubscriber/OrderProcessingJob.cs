using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using volans.demos.pubsub.Common;

namespace volans.demos.pubsub.RabbitSubscriber
{
    public class OrderProcessingJob : BackgroundService
    {
        private readonly ILogger<OrderProcessingJob> _logger;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _messageConnection;
        private IModel? _messageChannel;

        public OrderProcessingJob(ILogger<OrderProcessingJob> logger, IConfiguration config, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = config;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _messageConnection = _serviceProvider.GetService<IConnection>();

            _messageChannel = _messageConnection!.CreateModel();


            _messageChannel.ExchangeDeclare(exchange: "pubsub", type: ExchangeType.Fanout);

            var queueName = _messageChannel.QueueDeclare().QueueName;

            _messageChannel.QueueBind(queue: queueName, exchange: "pubsub", routingKey: "");

            var consumer = new EventingBasicConsumer(_messageChannel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Consumer - Recieved new message: {message}");
            };

            _messageChannel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            Console.WriteLine("Consuming");

            //Console.ReadKey();


            //old code
            /*
            const string configKeyName = "RabbitMQ:QueueName";
            string queueName = _config[configKeyName] ?? "orders";
            _messageConnection = _serviceProvider.GetService<IConnection>();

            _messageChannel = _messageConnection.CreateModel();
            _messageChannel.QueueDeclare(queueName, exclusive: false);

            var consumer = new EventingBasicConsumer(_messageChannel);
            consumer.Received += ProcessMessageAsync;

            _messageChannel.BasicConsume(queue: queueName,
                                         autoAck: true,
                                         consumer: consumer);
            */
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            _messageChannel?.Dispose();
        }
    }
}
