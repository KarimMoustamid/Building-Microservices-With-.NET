using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Repositories;
using Play.Catalog.Service.Settings;

var builder = WebApplication.CreateBuilder(args);

// Bind ServiceSettings from configuration and register in DI.
// This reads the "ServiceSettings" section from appsettings.json (if present).
var serviceSettings = builder.Configuration
    .GetSection(nameof(ServiceSettings))
    .Get<ServiceSettings>() ?? new ServiceSettings();
builder.Services.AddSingleton(serviceSettings);

// Add services to the container.

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