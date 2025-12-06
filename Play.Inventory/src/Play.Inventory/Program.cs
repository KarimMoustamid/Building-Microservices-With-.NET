using Play.Common.MongoDB;
using Play.Common.Settings;
using Play.Inventory.Client;
var builder = WebApplication.CreateBuilder(args);

ServiceSettings serviceSettings = builder.Configuration.GetSection("ServiceSettings").Get<ServiceSettings>();

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
});

builder.Services.AddSingleton<string>(serviceSettings.ServiceName);
builder.Services.AddScoped(typeof(Play.Common.Repositories.IRepository<>), typeof(Play.Common.MongoDB.MongoRepository<>));


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