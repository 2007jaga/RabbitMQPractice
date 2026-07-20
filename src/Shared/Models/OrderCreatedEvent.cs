namespace Shared.Models;

public class OrderCreatedEvent
{
    public int OrderId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime OrderDate { get; set; }
    public string CustomerType { get; set; } = ""; // this is only for Header Exchange
}