namespace Play.Common.MongoDB
{
    using global::MongoDB.Bson;
    using global::MongoDB.Bson.Serialization;
    using global::MongoDB.Bson.Serialization.Serializers;
    using global::MongoDB.Driver;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Repositories;
    using Settings;

    public static class Extensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration, string serviceName)
        {
            // Note : Register a Guid and DateTimeOff serializer with explicit representation before creating MongoClient
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>() ?? new MongoDbSettings();

            var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);

            services.AddSingleton<IMongoClient>(mongoClient);
            services.AddSingleton(sp => mongoClient.GetDatabase(serviceName));

            return services;
        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName) where T : IEntity
        {
            services.AddSingleton<IRepository<T>>(sp => new MongoRepository<T>(sp.GetRequiredService<IMongoDatabase>(), collectionName));

            return services;
        }
    }
}