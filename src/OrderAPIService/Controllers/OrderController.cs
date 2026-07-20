using Microsoft.AspNetCore.Mvc;
using OrderAPIService.Services;

namespace OrderAPIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        Console.WriteLine("OrderAPI Version 1.0.1 - CreateOrder API called");
        await _orderService.CreateOrderAsync();

        return Ok("Order Created Successfully.");
    }
}