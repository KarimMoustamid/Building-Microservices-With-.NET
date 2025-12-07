// csharp
using MassTransit;
using MassTransit.Definition;
using MassTransit.MultiBus;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Settings;
using Play.Common.MongoDB;
using Play.Common.Settings;


var builder = WebApplication.CreateBuilder(args);

// Bind Settings from configuration and register in DI.
var serviceSettings = builder.Configuration
    .GetSection(nameof(ServiceSettings))
    .Get<ServiceSettings>() ?? new ServiceSettings();
builder.Services.AddSingleton(serviceSettings);

// Load RabbitMQ settings; fall back to a default instance to avoid null dereference
var rabbitMQSettings = builder.Configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>() ?? new RabbitMQSettings();

// Add services to the container.

// Register MassTransit
builder.Services.AddMassTransit<IBus>(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMQSettings.Host);
        cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
    });
});

// Register the hosted service for MassTransit (use the standard overload)
builder.Services.AddMassTransitHostedService();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register mongo client and IMongoDatabase using extension
builder.Services.AddMongo(builder.Configuration, serviceSettings.ServiceName);

// Register a repository for Item using the generic MongoRepository<T>
builder.Services.AddMongoRepository<Item>("items");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();