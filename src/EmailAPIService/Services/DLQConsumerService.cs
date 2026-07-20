using System.Text;
using System.Text.Json;
using EmailAPIService.Data;
using EmailAPIService.Models;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmailAPIService.Consumer
{
    //No controller calls it. No one manually creates it.ASP.NET Core automatically starts it in the background. That's exactly what we want for a RabbitMQ consumer because it needs to listen continuously.
    public class DLQConsumerService : BackgroundService
    {
        private IConnection? _connection;
        private IChannel? _channel;

        private readonly ApplicationDbContext _context;
        public DLQConsumerService(ApplicationDbContext context)
        {
            _context = context;
        }


        //Think of it as the entry point of the background service.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            Console.WriteLine("Starting DLQ Consumer...");

            var factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                UserName = "admin",
                Password = "admin123"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            Console.WriteLine("Connected to RabbitMQ.");
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);
            
            
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                Console.WriteLine("DLQ Message Received");

                // We will process the message in the next step.
                try
                {
                    #region 

                    // 1. Read message body
                    var body = eventArgs.Body.ToArray();
                    var jsonMessage = Encoding.UTF8.GetString(body);

                    // 2. Deserialize JSON 
                    var message = JsonSerializer.Deserialize<FailedEmailMessageDto>(jsonMessage);

                    if (message is null)
                        throw new Exception("Invalid DLQ message");


                    // 3. Create database entity
                    var failedEmail = new FailedEmailMessage
                    {
                        OrderId = message.OrderId,
                        CustomerName = message.CustomerName,
                        Email = message.Email,
                        Subject = message.Subject,
                        Body = message.Body,
                        FailureReason = message.FailureReason,
                        OriginalMessage = jsonMessage,
                        RetryCount = 0,
                        Status = "Failed",
                        CreatedDate = DateTime.UtcNow
                    };


                    // 4. Save into SQL Server
                    _context.FailedEmailMessages.Add(failedEmail);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Saved failed email for OrderId: {message.OrderId}");

                    // 5. Acknowledge RabbitMQ message
                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);

                    #endregion


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(queue: "email.deadletter.queue", autoAck: false, consumer: consumer);
            //Application Starts--> ExecuteAsync()-->Read RabbitMQ-->Read RabbitMQ-->Read RabbitMQ-->Read RabbitMQ
            // The loop stops only when: -->the application shuts down, --> the container stops, -->or the service is cancelled.-->This is exactly how production message consumers work.
            while (!stoppingToken.IsCancellationRequested)
            {
                // We will write the DLQ logic here

                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null)
                await _channel.DisposeAsync();

            if (_connection != null)
                await _connection.DisposeAsync();

            await base.StopAsync(cancellationToken);
        }

    }
}