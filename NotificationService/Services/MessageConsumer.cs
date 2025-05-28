using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore.Metadata;
using NotificationService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Services
{
    public class MessageConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private RabbitMQ.Client.IModel _channel;

        public MessageConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _configuration["RabbitMQ:QueueName"],
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var taskAssignment = JsonSerializer.Deserialize<TaskAssignmentMessage>(json);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<NotificationManagementService>();

                    var notification = new NotificationManagementEntity
                    {
                        UserId = taskAssignment.UserId,
                        Message = $"Task {taskAssignment.TaskId} assigned.",
                        SentDate = DateTime.UtcNow
                    };

                    notificationService.SendNotification(notification);
                }
            };

            _channel.BasicConsume(queue: _configuration["RabbitMQ:QueueName"],
                                  autoAck: true,
                                  consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }

    public class TaskAssignmentMessage
    {
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}
