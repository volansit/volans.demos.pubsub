using Aspire.Hosting;
using Projects;

//docs/ examples:
// https://learn.microsoft.com/en-us/dotnet/aspire/messaging/rabbitmq-integration?tabs=dotnet-cli
// 
var builder = DistributedApplication.CreateBuilder(args);

var rabbitmqPassword = builder.AddParameter("rabbitmq-password", secret: true);

/*
 * add a RabbitMQ server resource using builder.AddRabbitMQ("resourceName"). 
 * The "resourceName" will be used as the connection string name when referencing it in other projects. * 
 */ 
var rabbitMq = builder.AddRabbitMQ("eventbus", password: rabbitmqPassword)
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

/*
 * WithReference:
 * Injects a connection string as an environment variable from the source resource
 * into the destination resource, using the source resource's name as the connection
 * string name (if not overridden). The format of the environment variable will
 * be "ConnectionStrings__{sourceResourceName}={connectionString}
 */
var apiSerivce = builder.AddProject<PubSub_ApiService>("apiservice")
    .WithReference(rabbitMq);

/*
 * Waits for the dependency reeource to enter the "Running" state, before starting the resource
 */
builder.AddProject<RabbitConsumer>("RabbitMQConsumer")
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WaitFor(apiSerivce);


builder.AddProject<RabbitSubscriber>("RabbitSubscriber1")
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WaitFor(apiSerivce);

builder.AddProject<RabbitSubscriber>("RabbitSubscriber2")
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WaitFor(apiSerivce);
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
