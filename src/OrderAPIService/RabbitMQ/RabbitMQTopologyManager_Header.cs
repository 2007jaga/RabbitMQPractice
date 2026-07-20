using RabbitMQ.Client;

namespace OrderAPIService.RabbitMQ;

public class RabbitMQTopologyManager_Header
{
    private readonly RabbitMQConnection _rabbitMQConnection;

    public RabbitMQTopologyManager_Header(RabbitMQConnection rabbitMQConnection)
    {
        _rabbitMQConnection = rabbitMQConnection;
    }

    public async Task CreateTopologyAsync()
    {
        await using var connection = await _rabbitMQConnection.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        // ------------------------------------------------------------------
        // Headers Exchange
        // ------------------------------------------------------------------
        await channel.ExchangeDeclareAsync(exchange: RabbitMQConstants.NotificationHeaderExchange, type: ExchangeType.Headers, durable: true, autoDelete: false);
        Console.WriteLine("notification.header.exchange created successfully.");


        // Dead Letter Exchange
        await channel.ExchangeDeclareAsync(exchange: "email.dlx", type: ExchangeType.Direct, durable: true, autoDelete: false);
        Console.WriteLine("email.dlx created.");


        // ------------------------------------------------------------------
        // Premium Retry Exchange
        // ------------------------------------------------------------------
        await channel.ExchangeDeclareAsync(
            exchange: RabbitMQConstants.PremiumRetryExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        Console.WriteLine("premium.retry.exchange created.");


        // ------------------------------------------------------------------
        // Normal Retry Exchange
        // ------------------------------------------------------------------
        await channel.ExchangeDeclareAsync(
            exchange: RabbitMQConstants.NormalRetryExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        Console.WriteLine("normal.retry.exchange created.");



        // ------------------------------------------------------------------
        // Dead Letter Queue
        // ------------------------------------------------------------------
        await channel.QueueDeclareAsync(queue: "email.deadletter.queue", durable: true, exclusive: false, autoDelete: false);
        Console.WriteLine("email.deadletter.queue created successfully.");

        await channel.QueueBindAsync(queue: "email.deadletter.queue", exchange: "email.dlx", routingKey: "premium.dead");
        await channel.QueueBindAsync(queue: "email.deadletter.queue", exchange: "email.dlx", routingKey: "normal.dead");
        Console.WriteLine("email.deadletter.queue bound successfully.");


        // ------------------------------------------------------------------
        // Premium Retry Queue
        // ------------------------------------------------------------------
        var premiumRetryQueueArguments = new Dictionary<string, object>
{
    { "x-message-ttl", 10000 },
    { "x-dead-letter-exchange", "" },
    { "x-dead-letter-routing-key", RabbitMQConstants.PremiumEmailQueue }
};

        await channel.QueueDeclareAsync(
            queue: RabbitMQConstants.PremiumRetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: premiumRetryQueueArguments);

        await channel.QueueBindAsync(
            queue: RabbitMQConstants.PremiumRetryQueue,
            exchange: RabbitMQConstants.PremiumRetryExchange,
            routingKey: RabbitMQConstants.PremiumRetryRoutingKey);

        Console.WriteLine("premium.retry.queue created successfully.");

        // ------------------------------------------------------------------
        // Normal Retry Queue
        // ------------------------------------------------------------------
        var normalRetryQueueArguments = new Dictionary<string, object>
{
    { "x-message-ttl", 10000 },
    { "x-dead-letter-exchange", "" },
    { "x-dead-letter-routing-key", RabbitMQConstants.NormalEmailQueue }
};

        await channel.QueueDeclareAsync(
            queue: RabbitMQConstants.NormalRetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: normalRetryQueueArguments);

        await channel.QueueBindAsync(
            queue: RabbitMQConstants.NormalRetryQueue,
            exchange: RabbitMQConstants.NormalRetryExchange,
            routingKey: RabbitMQConstants.NormalRetryRoutingKey);

        Console.WriteLine("normal.retry.queue created successfully.");

        

        var premiumQueueArguments = new Dictionary<string, object> { { "x-dead-letter-exchange", "email.dlx" }, { "x-dead-letter-routing-key", "premium.dead" } };
        // ------------------------------------------------------------------
        // Premium Queue
        // ------------------------------------------------------------------
        await channel.QueueDeclareAsync(queue: RabbitMQConstants.PremiumEmailQueue, durable: true, exclusive: false, autoDelete: false, arguments: premiumQueueArguments);
        Console.WriteLine("premium.email.queue created successfully.");


        var normalQueueArguments = new Dictionary<string, object> { { "x-dead-letter-exchange", "email.dlx" }, { "x-dead-letter-routing-key", "normal.dead" } };
        // ------------------------------------------------------------------
        // Normal Queue
        // ------------------------------------------------------------------
        await channel.QueueDeclareAsync(queue: RabbitMQConstants.NormalEmailQueue, durable: true, exclusive: false, autoDelete: false, arguments: normalQueueArguments!);
        Console.WriteLine("normal.email.queue created successfully.");

        // ------------------------------------------------------------------
        // Premium Queue Arguments
        // ------------------------------------------------------------------
        // var premiumQueueArguments = new Dictionary<string, object> { { "x-dead-letter-exchange", "email.dlx" }, { "x-dead-letter-routing-key", "premium.dead" } };

        // await channel.QueueBindAsync(queue: RabbitMQConstants.PremiumEmailQueue, exchange: RabbitMQConstants.NotificationHeaderExchange, routingKey: string.Empty, arguments: premiumQueueArguments!);
        // Console.WriteLine("premium.email.queue bound successfully.");

        var premiumHeaders = new Dictionary<string, object?> { { "x-match", "all" }, { "CustomerType", "Premium" } };
        await channel.QueueBindAsync(queue: RabbitMQConstants.PremiumEmailQueue, exchange: RabbitMQConstants.NotificationHeaderExchange, routingKey: string.Empty, arguments: premiumHeaders);

        Console.WriteLine("premium.email.queue bound successfully.");

        // ------------------------------------------------------------------
        // Normal Queue Arguments
        // ------------------------------------------------------------------
        // var normalQueueArguments = new Dictionary<string, object> { { "x-dead-letter-exchange", "email.dlx" }, { "x-dead-letter-routing-key", "normal.dead" } };

        // await channel.QueueBindAsync(queue: RabbitMQConstants.NormalEmailQueue, exchange: RabbitMQConstants.NotificationHeaderExchange, routingKey: string.Empty, arguments: normalQueueArguments!);
        // Console.WriteLine("normal.email.queue bound successfully.");


        var normalHeaders = new Dictionary<string, object?> { { "x-match", "all" }, { "CustomerType", "Normal" } };

        await channel.QueueBindAsync(queue: RabbitMQConstants.NormalEmailQueue, exchange: RabbitMQConstants.NotificationHeaderExchange, routingKey: string.Empty, arguments: normalHeaders);
        Console.WriteLine("normal.email.queue bound successfully.");
    }
}