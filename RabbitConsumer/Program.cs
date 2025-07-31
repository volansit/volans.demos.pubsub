// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using volans.demos.pubsub.RabbitConsumer;

Console.WriteLine("Hello, World!");
var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddRabbitMQClient("eventbus");
builder.Services.AddHostedService<OrderProcessingJob>();
builder.Build().Run();
