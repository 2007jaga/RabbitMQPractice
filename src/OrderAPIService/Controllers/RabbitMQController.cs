using Microsoft.AspNetCore.Mvc;
using OrderAPIService.RabbitMQ;

namespace OrderAPIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RabbitMQController : ControllerBase
{
    private readonly RabbitMQTopologyManager _topologyManager;

    public RabbitMQController(RabbitMQTopologyManager topologyManager)
    {
        _topologyManager = topologyManager;
    }

    [HttpPost("setup")]
    public async Task<IActionResult> Setup()
    {
        await _topologyManager.CreateTopologyAsync();

        return Ok("RabbitMQ topology created successfully.");
    }
}