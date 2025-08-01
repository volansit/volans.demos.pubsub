using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitmqPassword = builder.AddParameter("rabbitmq-password", secret: true);

var rabbitMq = builder.AddRabbitMQ("eventbus", password: rabbitmqPassword)
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

var apiService = builder.AddProject<Projects.PubSub_ApiService>("apiservice")
    .WithReference(rabbitMq);

builder.AddProject<RabbitConsumer>("RabbitMQConsumer")
    .WithReference(rabbitMq).WaitFor(rabbitMq);

builder.AddProject<RabbitSubscriber>("RabbitSubscriber1")
    .WithReference(rabbitMq).WaitFor(rabbitMq);

builder.AddProject<RabbitSubscriber>("RabbitSubscriber2")
    .WithReference(rabbitMq).WaitFor(rabbitMq);


/*
 * removed pre added references
var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.PubSub_ApiService>("apiservice");

builder.AddProject<Projects.PubSub_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);
*/
builder.Build().Run();
