using EmailAPIService.Exceptions;
using Shared.Models;

namespace EmailAPIService.Services;

public class EmailService
{
    public Task SendEmailAsync(OrderCreatedEvent orderEvent)
    {

        // Simulate a temporary failure
        if (orderEvent.CustomerName == "Fail")
        {
           throw new TemporaryFailureException("SMTP Server is temporarily unavailable.");
        }

        Console.WriteLine("----------------------------------------");
        Console.WriteLine("EMAIL SERVICE");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"Order Id      : {orderEvent.OrderId}");
        Console.WriteLine($"Customer Name : {orderEvent.CustomerName}");
        Console.WriteLine($"Email         : {orderEvent.Email}");
        Console.WriteLine($"Amount        : {orderEvent.TotalAmount}");
        Console.WriteLine($"Order Date    : {orderEvent.OrderDate}");
        Console.WriteLine("----------------------------------------");
        Console.WriteLine("Email Sent Successfully.");
        Console.WriteLine("----------------------------------------");

        return Task.CompletedTask;
    }
}