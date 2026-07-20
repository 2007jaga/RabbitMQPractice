using RabbitMQ.Client;

namespace OrderAPIService.RabbitMQ;

public class RabbitMQTopologyManager
{
    private readonly RabbitMQConnection _rabbitMQConnection;


    public RabbitMQTopologyManager(RabbitMQConnection rabbitMQConnection)
    {
        _rabbitMQConnection = rabbitMQConnection;
    }

    /// <summary>
    /// If the exchange doesn't exist, RabbitMQ creates it.
    /// If it already exists with the same configuration, RabbitMQ does nothing.
    /// So it's safe to call every time the application starts.
    /// This is a common production practice.
    /// </summary>
    /// <returns> If the exchange doesn't exist, RabbitMQ creates it.</returns>
    public async Task CreateTopologyAsync()
    {
        await using var connection = await _rabbitMQConnection.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: RabbitMQConstants.NotificationExchange, type: ExchangeType.Direct, durable: true, autoDelete: false);
        Console.WriteLine("notification.exchange created successfully.");

        var queueArguments = new Dictionary<string, object?>
        {

            { "x-dead-letter-exchange", "email.dlx" },
            { "x-dead-letter-routing-key", "email.deadletter" }
        };

        await channel.QueueDeclareAsync(queue: RabbitMQConstants.EmailQueue, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments);
        Console.WriteLine("email.queue created successfully.");

        await channel.QueueDeclareAsync(queue: RabbitMQConstants.SmsQueue, durable: true, exclusive: false, autoDelete: false);
        Console.WriteLine("sms.queue created successfully.");

        var retryQueueArguments = new Dictionary<string, object?>
        {
            { "x-message-ttl", 10000 },
            { "x-dead-letter-exchange", RabbitMQConstants.NotificationExchange },
            { "x-dead-letter-routing-key", RabbitMQConstants.EmailRoutingKey }
        };

        await channel.QueueDeclareAsync(queue: RabbitMQConstants.RetryQueue, durable: true, exclusive: false, autoDelete: false, arguments: retryQueueArguments);
        Console.WriteLine("retry.queue created successfully.");



        await channel.QueueBindAsync(queue: RabbitMQConstants.EmailQueue, exchange: RabbitMQConstants.NotificationExchange, routingKey: RabbitMQConstants.EmailRoutingKey);
        Console.WriteLine("email.queue bound to notification.exchange.");

        await channel.QueueBindAsync(queue: RabbitMQConstants.SmsQueue, exchange: RabbitMQConstants.NotificationExchange, routingKey: RabbitMQConstants.SmsRoutingKey);
        Console.WriteLine("sms.queue bound to notification.exchange.");
    }
}