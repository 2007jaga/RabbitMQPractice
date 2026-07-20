namespace EmailAPIService.Models;

public class FailedEmailMessageDto
{
    public int OrderId { get; set; }

    public string CustomerName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Subject { get; set; } = "";

    public string Body { get; set; } = "";

    public string FailureReason { get; set; } = "";
}