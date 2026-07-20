using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderAPIService.RabbitMQ;

public class RabbitMQPublisherCopy
{
    private readonly ConnectionFactory _factory;

    public RabbitMQPublisherCopy()
    {
        _factory = new ConnectionFactory
        {
            // HostName = "localhost", 
            //  UserName = "guest",
            // Password = "guest"           
            HostName = "rabbitmq",
            UserName = "admin",
            Password = "admin123"

        };
    }

    public async Task PublishAsync<T>(T message)
    {
        await using var connection = await _factory.CreateConnectionAsync();

        await using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true,
                                    publisherConfirmationTrackingEnabled: true));

        // Queue arguments for Dead Letter Queue
        var queueArguments = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", "email.dlx" },
            { "x-dead-letter-routing-key", "email.deadletter" }
        };

        // Declare the main queue
        await channel.QueueDeclareAsync(queue: "email.queue", durable: true, exclusive: false, autoDelete: false, arguments: queueArguments);

        // Convert object to JSON
        var json = JsonSerializer.Serialize(message);

        // Convert JSON to bytes
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true
        };

        // await channel.BasicPublishAsync(
        //     exchange: string.Empty,
        //     routingKey: "email.queue",
        //     body: body);try
        try
        {
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "email.queue", mandatory: false, basicProperties: properties, body: body);

            Console.WriteLine("Publisher Confirm Received");
            Console.WriteLine($"Published: {json}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Publisher Confirm Failed");
            Console.WriteLine(ex.Message);
        }


    }
}

// Publisher -->email.queue --> Consumer receives the message --> Email sent successfully ✅
// After successfully sending the email, what do you think the consumer should tell RabbitMQ?
// "I have successfully processed this message. You can remove it from the queue."

// await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);




// Publisher --> RabbitMQ --> Consumer --> Send Email --> ❌ SMTP Server Down

// Because the email was not sent. If we send an ACK now then RabbitMQ deletes the message from the  QUEUE, 
// so that The message will lost forever. That is a serious production bug.
// instead of that RabbitMQ now has options if the Processing Failed 1. Retry, 2.Send to DLQ  3.Discard .  This is where NACK and REJECT come in.

// Every production system first asks --> 
//   Did it fail?
//       │
//       ▼
// Is the failure temporary?
//       │
//       ├── Yes → Retry     BasicNackAsync()  
//       │
//       └── No → Dead Letter Queue / Discard  BasicRejectAsync()



// Do we need to implement RetryCount ourselves?  For our current architecture: No.
// Why? RabbitMQ automatically adds a special header called: x-death
// This header records: How many times the message has been dead-lettered. Which queue it came from. Why it was dead-lettered.

// What is the x-death Header?
// Imagine this flow  Publisher--> mail.queue--> Consumer--> ❌ Temporary Failure--> Retry Queue (10 sec TTL)--> email.queue
// RabbitMQ internally thinks: "This message has already died once."  So it adds a header called: x-death

// Second failure  Again the flow  Publisher--> mail.queue--> Consumer--> ❌ Temporary Failure--> Retry Queue (10 sec TTL)--> email.queue
// RabbitMQ updates the same header. Now it becomes: x-death.count = 2




// Step 1: Why do we need a Retry Queue?

// Let's first understand why we need another queue.
// Imagine your consumer fails because the SMTP server is temporarily unavailable.

// If you do this:  await channel.BasicNackAsync(    deliveryTag: ea.DeliveryTag,    multiple: false,    requeue: true);
// The message goes back immediately.  email.queue--> Consumer-->SMTP Down ❌-->Requeue--> email.queue --> Consumer again
// This entire cycle might happen in milliseconds. The SMTP server is still down, so it fails again. This creates a tight loop.


// What do we really want? Instead of retrying immediately, we want to wait.

// For example: Attempt 1 --> Fail --> Wait 10 seconds--> Attempt 2
// Now the SMTP server has time to recover.

// How do we make RabbitMQ wait?
// RabbitMQ queues support something called: TTL (Time-To-Live)

// Example: retry.queue--> TTL = 10 seconds

// This means:  Every message entering this queue must stay there for 10 seconds.

// After 10 seconds:  retry.queue--> TTL Expired--> RabbitMQ automatically forwards it, No consumer is needed for the retry queue.RabbitMQ itself handles the waiting.

/*
Visual Architecture

After we build it, the flow will look like this:

                notification.exchange
                         │
                         ▼
                   email.queue
                         │
                         ▼
                    Email Consumer
                         │
          ┌──────────────┴──────────────┐
          │                             │
      Success ✅                    Temporary Fail ❌
          │                             │
         ACK                            ▼
                                 retry.queue
                               (TTL 10 seconds)
                                      │
                                      ▼
                                email.queue

*/



/*

Property	                Value	                Why?
Queue Name	                retry.queue	            Queue that temporarily stores failed messages
Durable	                    true	                Survives RabbitMQ restart
Exclusive	                false	                Shared by all consumers
AutoDelete	                false	                Don't delete automatically
TTL	                        10000 ms	            Hold the message for 10 seconds
Dead Letter Exchange	    notification.exchange	After 10 seconds, send the message back
Dead Letter Routing Key	    email	                Route it back to email.queue


Why notification.exchange?

This is a very important point.

When the TTL expires, RabbitMQ asks:

"Where should I send this message?"

The answer is:

notification.exchange

Then the exchange uses the routing key:

email

to route the message back to:

email.queue

So the complete retry path becomes:

retry.queue
      │
TTL expires (10 sec)
      │
      ▼
notification.exchange
      │
Routing Key = email
      │
      ▼
email.queue

This is a beautiful RabbitMQ design because the exchange handles the routing, just like it did when the publisher originally sent the message.
*/


// We created: retry.queue  But...  Who is going to put messages into it?
// Think about it. Currently the publisher does: Publisher--> notification.exchange--> email.queue
// The publisher does not know nothing about retry.queue. 

// The Consumer makes the decision ,Imagine this: Consumer receives message--> Try Send Email-->SMTP Server Down 
// Now the consumer thinks: "This is temporary."  So instead of saying:  BasicNack(requeue:true)  
// the consumer will say: "I'm going to publish this message into retry.queue." Then RabbitMQ waits 10 seconds. 
// Then RabbitMQ automatically sends it back to:  notification.exchange--> email.queue

// requeue = true → Put it back into the same queue
// requeue = false → Send it to the Dead Letter Exchange (DLX) if one is configured
// That's all BasicNackAsync() can do.

// So how do we send a message to retry.queue?
// We publish it like a brand-new message.
// Consumer--> BasicPublishAsync()--> retry.queue


// Why didn't we use BasicNackAsync(requeue:true)?
// Because it causes this: Fail --> Immediate Retry --> Fail--> Immediate Retry --> Fail
// No waiting.
// No backoff.
// No control.




//Why do we need an Exchange?

// Imagine you want to send a courier.

// Without RabbitMQ Exchange:

// Publisher
//     │
//     ▼
// email.queue

// The publisher must know the exact queue name.

// If tomorrow you rename the queue:

// email.queue
//         ↓
// customer-email.queue

// You have to change the publisher code.

// That creates tight coupling.

// With an Exchange
// Publisher
//      │
//      ▼
// notification.exchange
//      │
//      ▼
// email.queue

// Now the publisher knows only:

// Exchange Name
// Routing Key

// It doesn't know (or care) which queue receives the message.




// Why is this useful?

// Suppose tomorrow you add another service.

// Today:

// Publisher
//     │
// notification.exchange
//     │
//     ▼
// email.queue

// Tomorrow:

//                  email.queue
//                 /
// Publisher
//     │
// notification.exchange
//                 \
//                  sms.queue

// Did the publisher change?

// No.

// The exchange handles the routing.

// That's the biggest benefit of using exchanges.