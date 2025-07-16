using Fisdane.RabbitMQ.Configuration;
using Fisdane.RabbitMQ.Options;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Fisdane.RabbitMQ;

public static class ServiceInjection
{
    public static IServiceCollection ValidateRabbitMQOptions(this IServiceCollection services) =>
        services.AddSingleton<IValidateOptions<RabbitMQOption>, RabbitMQOptionWithValidation>();

    public static IServiceCollection RegisterPublisher(this IServiceCollection services)
    {
        return services
                .AddMassTransit(x =>
                {
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        RabbitMQOption rabbitmqOptions = context.GetRequiredService<IOptions<RabbitMQOption>>().Value;

                        cfg.Host(host: rabbitmqOptions.Host, port: (ushort)rabbitmqOptions.Port, virtualHost: rabbitmqOptions.VirtualHost,
                            h =>
                            {
                                h.Username(rabbitmqOptions.Username);
                                h.Password(rabbitmqOptions.Password);
                            }
                        );
                    });
                })
                .AddScoped<ISendMessage, SendMessage>()
                .AddScoped<IPublishEvent, PublishEvent>();
    }

    public static IServiceCollection RegisterConsumers(this IServiceCollection services, IConfiguration configuration, ConsumerMappingDelegate consumerMapping)
    {
        var rabbitmqOptions = configuration.GetSection("RabbitMQ").Get<RabbitMQOption>();
        if (rabbitmqOptions == null)
            throw new ArgumentNullException(nameof(rabbitmqOptions), "RabbitMQ options are not configured.");

        // get enabled consumers from configuration
        var enabled = GetEnabledConsumers(rabbitmqOptions.EnabledConsumers, consumerMapping);

        services
            .AddMassTransit(busConfig =>
            {
                busConfig.AddDelayedMessageScheduler();

                // Use reflection to call the generic AddConsumer method
                var addConsumerMethod = typeof(RegistrationExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .FirstOrDefault(m => m.Name == "AddConsumer" && m.IsGenericMethodDefinition &&
                                m.GetParameters().Length == 2 &&
                                m.GetGenericArguments().Length == 2 &&
                                m.GetParameters()[0].ParameterType == typeof(IRegistrationConfigurator));

                foreach (var (consumerType, consumerDefinitionType) in enabled)
                {
                    if (addConsumerMethod != null)
                    {
                        var genericMethod = addConsumerMethod.MakeGenericMethod(consumerType, consumerDefinitionType);
                        genericMethod.Invoke(null, new object[] { (IRegistrationConfigurator)busConfig, null });
                    }
                }

                if (rabbitmqOptions.EndpointNameFormatter == EndpointNameFormatter.SnakeCase)
                    busConfig.SetSnakeCaseEndpointNameFormatter();

                if (rabbitmqOptions.EndpointNameFormatter == EndpointNameFormatter.KhebabCase)
                    busConfig.SetKebabCaseEndpointNameFormatter();

                busConfig.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(host: rabbitmqOptions.Host, port: (ushort)rabbitmqOptions.Port, virtualHost: rabbitmqOptions.VirtualHost,
                        h =>
                        {
                            h.Username(rabbitmqOptions.Username);
                            h.Password(rabbitmqOptions.Password);
                        }
                    );

                    //add transport-independent settings, applies to all receive endpoints
                    cfg.Durable = true;
                    cfg.UseDelayedMessageScheduler();

                    if (rabbitmqOptions.GeneralPrefetchCount > 0)
                        cfg.PrefetchCount = rabbitmqOptions.GeneralPrefetchCount;

                    if (rabbitmqOptions.GeneralConcurrentMessageLimit > 0)
                        cfg.ConcurrentMessageLimit = rabbitmqOptions.GeneralConcurrentMessageLimit;

                    cfg.ConfigureEndpoints(context);
                });
            });

        return services;
    }

    private static List<(Type ConsumerType, Type ConsumerDefinitionType)> GetEnabledConsumers(string[] enabledConsumers, ConsumerMappingDelegate consumerMapping)
    {
        var consumerMappings = consumerMapping();

        if (consumerMappings.Count == 0)
            throw new ArgumentNullException(nameof(consumerMapping),
                "No consumers are configured. Please ensure that the consumer mappings are correctly defined.");

        var configuredConsumers = enabledConsumers.Where(name => consumerMappings.ContainsKey(name))
           .Select(name => consumerMappings[name]).ToList();

        return configuredConsumers;
    }
}