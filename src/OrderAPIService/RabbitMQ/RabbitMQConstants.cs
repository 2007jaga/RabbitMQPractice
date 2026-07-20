namespace OrderAPIService.RabbitMQ;

public static class RabbitMQConstants
{
    public const string NotificationExchange = "notification.exchange";

    public const string EmailQueue = "email.queue";
    public const string SmsQueue = "sms.queue";

    public const string EmailRoutingKey = "email";
    public const string SmsRoutingKey = "sms";
    public const string PushRoutingKey = "push";

    public const string OrderCreatedRoutingKey = "order.created";
    public const string PaymentSuccessRoutingKey = "payment.success";
    public const string RetryQueue = "retry.queue";

    public const string OrderCreatedRoutingemail = "order.created.email";
    public const string OrderCreatedRoutingStar = "order.created.*";


    public const string NotificationHeaderExchange = "notification.header.exchange";
    public const string PremiumRetryExchange = "premium.retry.exchange";
    public const string PremiumEmailQueue = "premium.email.queue";
    public const string NormalEmailQueue = "normal.email.queue";


    public const string NormalRetryExchange = "normal.retry.exchange";

    public const string PremiumRetryQueue = "premium.retry.queue";
    public const string NormalRetryQueue = "normal.retry.queue";

    public const string PremiumRetryRoutingKey = "premium.retry";
    public const string NormalRetryRoutingKey = "normal.retry";


    public const string DeadLetterExchange = "email.dlx";
public const string PremiumDeadRoutingKey = "premium.dead";
public const string NormalDeadRoutingKey = "normal.dead";
}