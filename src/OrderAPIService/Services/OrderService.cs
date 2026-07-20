// using OrderAPIService.RabbitMQ;

// namespace OrderAPIService.Services;

// public class OrderService
// {
//     private readonly RabbitMQPublisher _rabbitMQPublisher;

//     public OrderService(RabbitMQPublisher rabbitMQPublisher)
//     {
//         _rabbitMQPublisher = rabbitMQPublisher;
//     }

//     public async Task CreateOrderAsync()
//     {
//         // Business Logic
//         Console.WriteLine("Order Created Successfully.");

//         // Publish Event
//         await _rabbitMQPublisher.PublishAsync("Order #1001 Created");
//     }
// }


using OrderAPIService.RabbitMQ;
using Shared.Models;

namespace OrderAPIService.Services;

public class OrderService
{
    private readonly RabbitMQPublisher _rabbitMQPublisher;

    public OrderService(RabbitMQPublisher rabbitMQPublisher)
    {
        _rabbitMQPublisher = rabbitMQPublisher;
    }

    public async Task CreateOrderAsync()
    {

        string randomCustomerType = new Random().Next(1, 10) % 2 == 0? "Premium":"Normal";

        // Simulate creating an order
        var orderEvent = new OrderCreatedEvent
        {
            OrderId = 1001,
            CustomerName = "Fail",
            Email = "jagannath@example.com",
            TotalAmount = 2500,
            OrderDate = DateTime.UtcNow
            //,CustomerType = "Premium"//randomCustomerType
        };

        Console.WriteLine($"Order {orderEvent.OrderId} Created Successfully.");

        await _rabbitMQPublisher.PublishAsync(orderEvent);
    }
}