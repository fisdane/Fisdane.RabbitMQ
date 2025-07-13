using System.ComponentModel;

namespace Fisdane.RabbitMQ.Configuration;

public enum EndpointNameFormatter
{
    [Description("CamelCaseNameFormatter")]
    CamelCase = 0,

    [Description("snake_case_name_formatter")]
    SnakeCase,

    [Description("snake-case-name-formatter")]
    KhebabCase,
}