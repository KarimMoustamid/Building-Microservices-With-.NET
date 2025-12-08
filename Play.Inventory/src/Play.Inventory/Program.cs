using System;
using Play.Common.MongoDB;
using Play.Common.Repositories;
using Play.Common.Settings;
using Play.Inventory.Client;
using Polly;
using Polly.Timeout;
using Polly.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Repositories.MassTransit;
using Play.Inventory.Entities;
var builder = WebApplication.CreateBuilder(args);

// Read required service settings from the configuration. This throws if the section is missing.
ServiceSettings serviceSettings = builder.Configuration.GetSection("ServiceSettings").Get<ServiceSettings>()
    ?? throw new InvalidOperationException("Missing required configuration: ServiceSettings section");

// Random jitter used to add a small random delay to retry backoffs to avoid thundering herd.
Random jitterer = new Random();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register MVC controllers.
builder.Services.AddControllers();

// Register MongoDB client and IMongoDatabase using a project extension.
builder.Services.AddMongo(builder.Configuration, serviceSettings.ServiceName)
    .AddMongoRepository<InventoryItem>("inventoryitems")
    .AddMongoRepository<CatalogItem>("catalogitems")
    .AddMassTransitWithRabbitMq();

// Register a typed HttpClient for CatalogClient with resilience policies.
// The policies applied are:
//  - Retry (WaitAndRetryAsync) for transient HTTP errors and timeouts with exponential backoff + jitter.
//  - Circuit Breaker to open the circuit after consecutive failures and fail fast for a short period.
//  - Timeout to cancel any single HTTP call that exceeds 1 second.
// ConfigureHttpClientPolicies(builder, jitterer);

builder.Services.AddSingleton<string>(serviceSettings.ServiceName);

// Register a scoped MongoDB-backed generic repository implementation.
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


// Register the service name for other components to consume.
void ConfigureHttpClientPolicies(WebApplicationBuilder webApplicationBuilder, Random random)
{
    webApplicationBuilder.Services.AddHttpClient<CatalogClient>(client =>
        {
            // Base address for the Catalog microservice.
            client.BaseAddress = new Uri("https://localhost:5001");
        })
        .AddTransientHttpErrorPolicy(
            // Retry policy: handles transient HTTP errors (5xx, 408, HttpRequestException) and timeouts.
            policyBuilder => policyBuilder
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(
                    5, // retry up to 5 times
                    retryAttempt =>
                        // exponential backoff (2^attempt seconds) plus random jitter (0-999 ms)
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(random.Next(0, 1000)),
                    onRetry: (outcome, timespan, retryAttempt) =>
                    {
                        // Console-based logging for retries. We check whether an exception caused the retry
                        // or whether we have an HTTP result to report the status code.
                        if (outcome.Exception is not null)
                        {
                            Console.WriteLine($"Retry {retryAttempt} due to {outcome.Exception.GetType().Name}: {outcome.Exception.Message}. Waiting {timespan.TotalSeconds} seconds...");
                        }
                        else
                        {
                            Console.WriteLine($"Retry {retryAttempt} of {outcome.Result?.StatusCode} after {timespan.TotalSeconds} seconds. Waiting...");
                        }
                    }))
        .AddTransientHttpErrorPolicy(
            // Circuit-breaker: after 3 consecutive handled failures, open the circuit for 15 seconds.
            // While open, calls fail fast. onBreak/onReset are used for notifications (console here).
            policyBuilder => policyBuilder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3, // exceptions allowed before breaking
                TimeSpan.FromSeconds(15), // duration of break
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
                    // Circuit closed again — resume attempts.
                    Console.WriteLine($"Closing the circuit...");
                }
            ))
// Timeout policy: cancels any HTTP call that takes longer than 1 second. TimeoutRejectedException
// will be thrown and treated by the retry/circuit-breaker policies above (because of Or<TimeoutRejectedException>()).
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(1)));
}