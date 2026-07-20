using RabbitMQ.Client;
using Microsoft.Extensions.Options;


namespace OrderAPIService.RabbitMQ;

public class RabbitMQConnection
{
    private readonly ConnectionFactory _factory;

    public RabbitMQConnection(IOptions<RabbitMQSettings> options)
    {
        var settings = options.Value;

        _factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            UserName = settings.UserName,
            Password = settings.Password
        };
    }

    public async Task<IConnection> CreateConnectionAsync()
    {
        Console.WriteLine($"RabbitMQ Host: {_factory.HostName}");
        Console.WriteLine($"RabbitMQ User: {_factory.UserName}");
        return await _factory.CreateConnectionAsync();
    }
}