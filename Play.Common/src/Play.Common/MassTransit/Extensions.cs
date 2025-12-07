namespace Play.Common.Repositories.MassTransit
{
    using System.Reflection;
    using global::MassTransit;
    using global::MassTransit.Definition;
    using global::MassTransit.MultiBus;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Settings;

    public static class Extensions
    {
        public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services)
        {
            services.AddMassTransit<IBus>(configure =>
            {
                // Register consumers
                configure.AddConsumers(Assembly.GetEntryAssembly());
                // Register bus definitions
                configure.UsingRabbitMq((context, cfg) =>
                {
                    #region Setup
                    var configuration = context.GetRequiredService<IConfiguration>();

                    var serviceSettings = configuration
                        .GetSection(nameof(ServiceSettings))
                        .Get<ServiceSettings>() ?? new ServiceSettings();
                    var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>() ?? new RabbitMQSettings();
                    #endregion

                    cfg.Host(rabbitMQSettings.Host);
                    cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
                });
            });
            return services;
        }
    }
}