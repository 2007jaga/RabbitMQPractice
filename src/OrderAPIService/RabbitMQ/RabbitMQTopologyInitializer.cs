using Microsoft.Extensions.Hosting;

namespace OrderAPIService.RabbitMQ;

public class RabbitMQTopologyInitializer : BackgroundService
{
    private readonly RabbitMQTopologyManager _topologyManager; 
    private readonly ILogger<RabbitMQTopologyInitializer> _logger;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMQ Topology Initializer Started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _topologyManager.CreateTopologyAsync();
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    public RabbitMQTopologyInitializer(RabbitMQTopologyManager topologyManager,  ILogger<RabbitMQTopologyInitializer> logger)
    {
        _topologyManager = topologyManager;
        _logger = logger;
        
    }
}