using Microsoft.Extensions.Hosting;

namespace OrderAPIService.RabbitMQ;

public class RabbitMQTopologyInitializer_Fanout : BackgroundService
{ 
    private readonly RabbitMQTopologyManager_Fanout _topologyManagerFanout;
    private readonly ILogger<RabbitMQTopologyInitializer_Fanout> _logger;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMQ Topology Initializer Fanout Started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _topologyManagerFanout.CreateTopologyAsync();
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    public RabbitMQTopologyInitializer_Fanout(RabbitMQTopologyManager_Fanout topologyManagerFanout , ILogger<RabbitMQTopologyInitializer_Fanout> logger)
    {
        _topologyManagerFanout = topologyManagerFanout;
        _logger = logger; 
    }
}