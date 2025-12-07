using Play.Common.MongoDB;
using Play.Common.Repositories;
using Play.Common.Settings;
using Play.Inventory.Client;
using Polly;
using Polly.Timeout;
using Polly.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

ServiceSettings serviceSettings = builder.Configuration.GetSection("ServiceSettings").Get<ServiceSettings>()
    ?? throw new InvalidOperationException("Missing required configuration: ServiceSettings section");

Random jitterer = new Random();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register controllers
builder.Services.AddControllers();
// Register mongo client and IMongoDatabase using extension
builder.Services.AddMongo(builder.Configuration, serviceSettings.ServiceName );
builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:5001");
}).AddTransientHttpErrorPolicy(
        builder => builder.
            Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(5, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                onRetry: (outcome, timespan, retryAttempt) =>
                {
                    if (outcome.Exception is not null)
                    {
                        Console.WriteLine($"Retry {retryAttempt} due to {outcome.Exception.GetType().Name}: {outcome.Exception.Message}. Waiting {timespan.TotalSeconds} seconds...");
                    }
                    else
                    {
                        Console.WriteLine($"Retry {retryAttempt} of {outcome.Result?.StatusCode} after {timespan.TotalSeconds} seconds. Waiting...");
                    }
                }))
.AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
    3,
    TimeSpan.FromSeconds(15),
    onBreak: (outcome, timespan) =>
    {
        if (outcome.Exception is not null)
        {
            Console.WriteLine($"Opening the circuit for {timespan.TotalSeconds} seconds due to {outcome.Exception.GetType().Name}: {outcome.Exception.Message}");
        }
        else
        {
            Console.WriteLine($"Opening the circuit for {timespan.TotalSeconds} seconds. Status: {outcome.Result?.StatusCode}");
        }
    },
    onReset: () =>
    {
        Console.WriteLine($"Closing the circuit...");
    }
))
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(1)));

builder.Services.AddSingleton<string>(serviceSettings.ServiceName);
builder.Services.AddScoped(typeof(IRepository<>), typeof(Play.Common.MongoDB.MongoRepository<>));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map controller routes
app.MapControllers();

app.Run();