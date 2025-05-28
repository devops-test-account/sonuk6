using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace TaskAssignmentService.Services
{
    public class MessagePublisher
    {
        private readonly string _hostname;
        private readonly string _queueName;
        private readonly IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;

        public MessagePublisher(IConfiguration configuration)
        {
            _hostname = configuration["RabbitMQ:HostName"];
            _queueName = configuration["RabbitMQ:QueueName"];

            var factory = new ConnectionFactory() { HostName = _hostname };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _queueName,
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        public void Publish(object message)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: "",
                                  routingKey: _queueName,
                                  basicProperties: null,
                                  body: body);
        }
    }
}
