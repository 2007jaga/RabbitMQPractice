// using RabbitMQ.Client;
// using RabbitMQ.Client.Events;
// using System.Text;

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
// var factory = new ConnectionFactory
// {
//     HostName = "localhost",
//     Port = 5672,
//     UserName = "guest",
//     Password = "guest"
// };


// var connection = await factory.CreateConnectionAsync();
// var channel = await connection.CreateChannelAsync();

// //If the queue doesn't exist → RabbitMQ creates it. 
// // If the queue already exists with the same configuration → RabbitMQ does nothing. 
// // This makes every service independent.
// await channel.QueueDeclareAsync(
//     queue: "email.queue",
//     durable: true,
//     exclusive: false,
//     autoDelete: false,
//     arguments: null);


// var consumer = new AsyncEventingBasicConsumer(channel);

// consumer.ReceivedAsync += async (sender, eventArgs) =>
// {
//     var body = eventArgs.Body.ToArray();
//     var message = Encoding.UTF8.GetString(body);

//     Console.WriteLine($"Email received: {message}");

//     await channel.BasicAckAsync(
//         deliveryTag: eventArgs.DeliveryTag,
//         multiple: false);
// };


// await channel.BasicConsumeAsync(
//     queue: "email.queue",
//     autoAck: false,
//     consumer: consumer);

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();



// app.Run();



using EmailAPIService.Consumer;
using EmailAPIService.Services;

using EmailAPIService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

// Register Application Services
builder.Services.AddSingleton<EmailService>();


// Consumer setting
builder.Services.Configure<ConsumerSettings>(
    builder.Configuration.GetSection("ConsumerSettings"));

// Register RabbitMQ Background Service
//builder.Services.AddHostedService<RabbitMQConsumerService>();


// Register RabbitMQ Background Service

//builder.Services.AddHostedService<RabbitMQConsumerService>();
//builder.Services.AddHostedService<RabbitMQConsumerServiceTopic>();
//builder.Services.AddHostedService<RabbitMQConsumerServiceFanout>();
builder.Services.AddHostedService<RabbitMQConsumerServiceHeader>();

//DLQ COnsumer
builder.Services.AddHostedService<DLQConsumerService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();