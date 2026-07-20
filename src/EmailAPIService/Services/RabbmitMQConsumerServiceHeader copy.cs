


using EmailAPIService.Consumer;
using EmailAPIService.Data;
using EmailAPIService.Exceptions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Models;
using System.Formats.Tar;
using System.Text;
using System.Text.Json;

namespace EmailAPIService.Services;

public class RabbitMQConsumerServiceHeaderCopy : BackgroundService
{
    private int _processedMessageCount = 0;
    private readonly EmailService _emailService;
    private readonly ConsumerSettings _consumerSettings;
 
    public RabbitMQConsumerServiceHeaderCopy(EmailService emailService, IOptions<ConsumerSettings> consumerOptions )
    {
        _emailService = emailService;
        _consumerSettings = consumerOptions.Value; 
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            // HostName = "localhost", 
            //  UserName = "guest",
            // Password = "guest"           
            HostName = "rabbitmq",
            UserName = "admin",
            Password = "admin123"

        };

        // var connection = await factory.CreateConnectionAsync();

        IConnection connection = null!;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine("Connecting to RabbitMQ...");
                connection = await factory.CreateConnectionAsync();
                Console.WriteLine("Connected to RabbitMQ successfully.");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ is not ready. Retrying in 5 seconds...");
                Console.WriteLine(ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        var channel = await connection.CreateChannelAsync();
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            OrderCreatedEvent? orderEvent = null;
            try
            {
                Console.WriteLine($"{_consumerSettings.ConsumerName} Delay = {_consumerSettings.TimeDelay} second(s)");
                await Task.Delay(TimeSpan.FromSeconds(_consumerSettings.TimeDelay));
                var body = eventArgs.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                Console.WriteLine();
                Console.WriteLine($"{_consumerSettings.ConsumerName} - Message Received From RabbitMQ {_processedMessageCount}");
                Console.WriteLine(json);
                orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(json);
                if (orderEvent != null)
                {
                    await _emailService.SendEmailAsync(orderEvent);
                }

                // Success -> Acknowledge the message
                await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
                Console.WriteLine("Message Acknowledged (ACK)");
                _processedMessageCount++;

            }
            catch (TemporaryFailureException ex)
            {
                Console.WriteLine($"Temporary Error: {ex.Message}");

                int retryCount = 0;

                // Read our custom retry count header
                if (eventArgs.BasicProperties.Headers != null &&
                    eventArgs.BasicProperties.Headers.TryGetValue("x-retry-count", out var retryHeader))
                {
                    retryCount = Convert.ToInt32(retryHeader);
                }

                Console.WriteLine($"Retry Count = {retryCount}");

                if (retryCount >= 3)
                {
                    Console.WriteLine("Maximum retry count reached. Sending to DLQ.");

                    await channel.BasicRejectAsync(deliveryTag: eventArgs.DeliveryTag, requeue: false);
                    return;
                }

                // Increment retry count
                retryCount++;

                string retryExchange = orderEvent?.CustomerType == "Premium" ? "premium.retry.exchange" : "normal.retry.exchange";

                string retryRoutingKey = orderEvent?.CustomerType == "Premium" ? "premium.retry" : "normal.retry";


                Console.WriteLine($"Publishing to {retryExchange}...");

                var properties = new BasicProperties
                {
                    Persistent = true,
                    Headers = new Dictionary<string, object?>()
                };

                // Copy all existing headers except our retry header
                if (eventArgs.BasicProperties.Headers != null)
                {
                    foreach (var header in eventArgs.BasicProperties.Headers)
                    {
                        if (header.Key != "x-retry-count")
                        {
                            properties.Headers[header.Key] = header.Value;
                        }
                    }
                }

                // Add updated retry count
                properties.Headers["x-retry-count"] = retryCount;

                await channel.BasicPublishAsync(
                    exchange: retryExchange,
                    routingKey: retryRoutingKey,
                    mandatory: false,
                    basicProperties: properties,
                    body: eventArgs.Body);

                Console.WriteLine($"Republished with Retry Count = {retryCount}");

                await channel.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false);

                Console.WriteLine("Original message ACKed.");
            }
        };



        //await channel.BasicConsumeAsync(queue: "premium.email.queue", autoAck: false, consumer: consumer);
        await channel.BasicConsumeAsync(queue: "premium.email.queue", autoAck: false, consumer: consumer);
        await channel.BasicConsumeAsync(queue: "normal.email.queue", autoAck: false, consumer: consumer);

        Console.WriteLine("Listening on Premium and Normal queues...");

        await Task.Delay(Timeout.Infinite, stoppingToken);

    }
}


