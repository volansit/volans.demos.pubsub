using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using volans.demos.pubsub.Common;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.AddRabbitMQClient("eventbus");
// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/send-message", (IConnection messageConnection, IConfiguration configuration) =>
{
    const string configKeyName = "RabbitMQ:QueueName";
    string? queueName = configuration[configKeyName];

    using var channel = messageConnection.CreateModel();
    channel.QueueDeclare(queueName, exclusive: false);
    channel.BasicPublish(
        exchange: "",
        routingKey: queueName,
        basicProperties: null,
        body: JsonSerializer.SerializeToUtf8Bytes(new OrderModel
        {
            Name = $"Message from API: {Guid.NewGuid()}",
            Amount = 1000
        }));

    return Results.Ok("message sent");
});

app.MapGet("/publish-message", (IConnection messageConnection, IConfiguration configuration) =>
{
    using var channel = messageConnection.CreateModel();

    channel.ExchangeDeclare(exchange: "pubsub", type: ExchangeType.Fanout);

    var message = "Hello I want to broadcast this message";

    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange: "pubsub", "", null, body);

    Console.WriteLine($"Send message: {message}");

    return Results.Ok("message published");
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
