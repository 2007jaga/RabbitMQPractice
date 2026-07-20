// using RabbitMQ.Client;
// using System.Text;

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// var factory = new ConnectionFactory();
// factory.HostName = "localhost";
// factory.Port = 5672;
// factory.UserName = "guest";
// factory.Password = "guest";

// var connection = await factory.CreateConnectionAsync();
// var channel = await connection.CreateChannelAsync();
// await channel.ExchangeDeclareAsync(
//     exchange: "order.exchange", // This is simply the name of the Exchange.
//     type: ExchangeType.Direct, //"This Exchange should use exact Routing Key matching."
//     durable: true //"If durable true then If RabbitMQ restarts, keep this Exchange else disappear"
//     );


// await channel.QueueDeclareAsync(
//     queue: "email.queue", //This is the name of the queue. Later, our EmailService will read messages from: email.queue
//     durable: true, //"If durable true then If RabbitMQ restarts, keep this Queue  else disappear"
//     exclusive: false, // f this were true: Only this connection could use the queue.When the connection closes, RabbitMQ deletes the queue. 
//                       // We don't want that for a shared application queue.
//     autoDelete: false, //if it is true then the queue automatically delete when the last consumer disconnects."
//     arguments: null);

// await channel.QueueBindAsync(
//     queue: "email.queue",
//     exchange: "order.exchange",
//     routingKey: "email");


// var message = "Order #1001 Created";
// var body = Encoding.UTF8.GetBytes(message);


// await channel.BasicPublishAsync(
//     exchange: "order.exchange",
//     routingKey: "email",
//     body: body);

// var app = builder.Build();
// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast = Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast")
// .WithOpenApi();

// app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }




using OrderAPIService.RabbitMQ;
using OrderAPIService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Services
builder.Services.AddControllers();

builder.Services.Configure<RabbitMQSettings>(
    builder.Configuration.GetSection("RabbitMQSettings"));

// Register Application Services
//AddSingleton  Creates one instance only when someone asks for it through Dependency Injection.
//builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddSingleton<RabbitMQConnection>();
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddSingleton<RabbitMQTopologyManager>();
builder.Services.AddSingleton<RabbitMQTopologyManager_Fanout>();
builder.Services.AddSingleton<RabbitMQTopologyManager_Topic>();
builder.Services.AddSingleton<RabbitMQTopologyManager_Header>();

//AddHostedService, The Host itself starts it automatically during application startup., You never call it manually.
//builder.Services.AddHostedService<RabbitMQTopologyInitializer>(); 
//builder.Services.AddHostedService<RabbitMQTopologyInitializer_Fanout>();
//builder.Services.AddHostedService<RabbitMQTopologyInitializer_Topic>(); 
 builder.Services.AddHostedService<RabbitMQTopologyInitializer_Header>(); 
builder.Services.AddScoped<OrderService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.MapControllers();

// using (var scope = app.Services.CreateScope())
// {
//     var topologyManager = scope.ServiceProvider
//         .GetRequiredService<RabbitMQTopologyManager>();

//     await topologyManager.CreateTopologyAsync();
// }

app.Run();