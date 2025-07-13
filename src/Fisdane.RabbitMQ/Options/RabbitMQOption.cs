using Fisdane.RabbitMQ.Configuration;
using Microsoft.Extensions.Options;

namespace Fisdane.RabbitMQ.Options;

public class RabbitMQOption
{
    public required string Host { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public int Port { get; init; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public int GeneralConcurrentMessageLimit { get; set; } = 0;
    public int GeneralPrefetchCount { get; set; } = 0;
    public EndpointNameFormatter EndpointNameFormatter { get; set; } = 0;
    public required string[] EnabledConsumers { get; set; } = [];
}

public sealed class RabbitMQOptionWithValidation : IValidateOptions<RabbitMQOption>
{
    public ValidateOptionsResult Validate(string? name, RabbitMQOption options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
            failures.Add("RabbitMQ Host is required");

        if (string.IsNullOrWhiteSpace(options.Username))
            failures.Add("RabbitMQ Username is required");

        if (string.IsNullOrWhiteSpace(options.Password))
            failures.Add("RabbitMQ Password is required");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}