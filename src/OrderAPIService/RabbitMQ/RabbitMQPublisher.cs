using RabbitMQ.Client;
using Shared.Models;
using System.Text;
using System.Text.Json;

namespace OrderAPIService.RabbitMQ;

public class RabbitMQPublisher
{
    private readonly RabbitMQConnection _rabbitMQConnection;

    public RabbitMQPublisher(RabbitMQConnection rabbitMQConnection)
    {
        _rabbitMQConnection = rabbitMQConnection;
    }

    public async Task PublishAsync<T>(T message)
    {
        Console.WriteLine("1. Creating connection...");
        Console.WriteLine("PublishAsync() called");
        // Create the connection 
        await using var connection = await _rabbitMQConnection.CreateConnectionAsync();

        Console.WriteLine("2. Creating channel...");
        // create the channel
        // if publisherConfirmationsEnabled false  then Publisher--> Send Message --> Publisher assumes success 🤞, If the network breaks at that exact moment, the publisher doesn't know.
        // if publisherConfirmationsEnabled true   then Publisher--> Send Message --> RabbitMQ --> ACK , Now the publisher knows RabbitMQ accepted the message. That's why Publisher Confirms are recommended for production systems.
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(
                                                                     publisherConfirmationsEnabled: true,
                                                                     publisherConfirmationTrackingEnabled: true)
                                                             );

        Console.WriteLine("3. Serializing message...");
        //serialize the JSON
        var json = JsonSerializer.Serialize(message);

        Console.WriteLine("4. Converting to bytes...");
        //Convert to BYTES
        var body = Encoding.UTF8.GetBytes(json);


        Console.WriteLine("5. Publishing message...");
        // Persistent = true?  This is a very important concept. 
        // Imagine this scenario: Publisher --> RabbitMQ receives message --> 💥 RabbitMQ Server crashes
        // If Persistent = false then The message may be lost.
        // If Persistent = true then RabbitMQ will store the message on disk (assuming the queue is durable), so after it restarts, the message can still be delivered.


        var properties = new BasicProperties
        {
            Persistent = true,
            Headers = new Dictionary<string, object?> { { "CustomerType", Random.Shared.Next(2) == 0 ? "Premium" : "Normal" } }
        };




        // mandatory: false, This means: "If no queue matches this routing key, don't return the message to me."
        //await channel.BasicPublishAsync(exchange: RabbitMQConstants.NotificationExchange, routingKey: RabbitMQConstants.EmailRoutingKey, mandatory: false, basicProperties: properties, body: body);

        // Direct Exchange
        //await channel.BasicPublishAsync(exchange: RabbitMQConstants.NotificationExchange, routingKey: RabbitMQConstants.SmsRoutingKey, mandatory: false, basicProperties: properties, body: body);

        //Fanout Exchange
        //await channel.BasicPublishAsync(exchange: RabbitMQConstants.NotificationExchange, routingKey: string.Empty, mandatory: false, basicProperties: properties, body: body);

        //Topic Exchange
        //await channel.BasicPublishAsync(exchange: RabbitMQConstants.NotificationExchange, routingKey: RabbitMQConstants.OrderCreatedRoutingemail, mandatory: false, basicProperties: properties, body: body);

        //Header Exchange
        await channel.BasicPublishAsync(exchange: RabbitMQConstants.NotificationHeaderExchange, routingKey: string.Empty, mandatory: false, basicProperties: properties, body: body);

        Console.WriteLine("6. Message published successfully.");
    }
}