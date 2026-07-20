using Microsoft.Extensions.Hosting;

namespace OrderAPIService.RabbitMQ;

public class RabbitMQTopologyInitializer_Header : IHostedService
{
    private readonly RabbitMQTopologyManager_Header _topologyManagerHeader;
    private readonly ILogger<RabbitMQTopologyInitializer_Header> _logger;

    public RabbitMQTopologyInitializer_Header(
        RabbitMQTopologyManager_Header topologyManagerHeader,
        ILogger<RabbitMQTopologyInitializer_Header> logger)
    {
        _topologyManagerHeader = topologyManagerHeader;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ Header Topology Initializer Started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _topologyManagerHeader.CreateTopologyAsync();

                _logger.LogInformation("RabbitMQ Header Topology Created.");

                break; // Exit the loop once topology creation succeeds
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "RabbitMQ is not ready. Retrying in 5 seconds...");

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}