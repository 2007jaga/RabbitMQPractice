
namespace EmailAPIService.Models
{
    public class FailedEmailMessage
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        // Why message failed
        public string FailureReason { get; set; } = string.Empty;
        // Store original RabbitMQ JSON
        public string OriginalMessage { get; set; } = string.Empty;
        // Number of retry attempts
        public int RetryCount { get; set; }
        // Failed / Retrying / Resolved
        public string Status { get; set; } = "Failed";

        public DateTime FailedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RetriedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        // When DLQ received message
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}