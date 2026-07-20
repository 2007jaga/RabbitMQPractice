using Microsoft.Extensions.Hosting;

namespace OrderAPIService.RabbitMQ;

public class RabbitMQTopologyInitializer_Topic : IHostedService
{
    private readonly RabbitMQTopologyManager_Topic _topologyManagerTopic;
    private readonly ILogger<RabbitMQTopologyInitializer_Topic> _logger;
    public RabbitMQTopologyInitializer_Topic(RabbitMQTopologyManager_Topic topologyManagerTopic, ILogger<RabbitMQTopologyInitializer_Topic> logger)
    {
        _topologyManagerTopic = topologyManagerTopic;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ Topic Topology Initializer Started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _topologyManagerTopic.CreateTopologyAsync();

                _logger.LogInformation("RabbitMQ Topic Topology Created.");

                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}