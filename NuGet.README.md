# Fisdane.RabbitMQ

Wrapper around MassTransit.RabbitMQ making it easier to setup with your background services without complexity

- First class testing support
- Write once, then deploy using RabbitMQ
- Fully-supported, widely-adopted, a complete end-to-end solution

## Documentation

Get started by [reading through the documentation](https://abc.com/).


## Getting started
Fisdane.RabbitMQ is installed from NuGet.


```
dotnet add package Fisdane.RabbitMQ
```

The simplest way to set up is using the **RegisterConsumers** extension method to setup background workers and connect to rabbitmq or, using **RegisterPublisher** method to connect a service to the broker as a producer of events or messages  

Sample Program.cs:

```
using Fisdane.RabbitMQ;
using Fisdane.RabbitMQ.Options;
using Test.Worker;

var builder = Host.CreateApplicationBuilder(args);

var env = builder.Environment.EnvironmentName;

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

if (args != null)
    builder.Configuration.AddCommandLine(args);

builder.Services
    .Configure<RabbitMQOption>(builder.Configuration.GetSection("RabbitMQ"))
    .ValidateRabbitMQOptions();

// GetConsumerMappings method passed as a parameter here
// GetConsumerMappings should be created by YOU!
builder.Services.RegisterConsumers(builder.Configuration, Utils.GetConsumerMappings);

var host = builder.Build();
host.Run();
```

Your service **must** set rabbitmq configruation values as follows

Sample appsettings.json:
```
{
  ...
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "admin",
    "Password": "admin",
    "VirtualHost": "/",
    "GeneralConcurrentMessageLimit": 2,
    "GeneralPrefetchCount": 2,
    "EndpointNameFormatter": "SnakeCase",
    "EnabledConsumers": [
      "SubmitOrder",
      "ShipmentCreated"
      ...
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
  ...
}
```
EndpointNameFormatter have three possible values _[KhebabCase, SnakeCase & CamelCase]_ deciding how you want your queue/endpoint names to look like

In the program.cs file, GetConsumerMappings is called to fetch this data. Very important piece of this library
Your project should have a method that contains all the consumers available with their respect consumer definitions. Find sample files below

Sample util.cs
##### _Dictionary keys should match the names of consumers set as EnabledConsumers in the appsettings.json_
```
internal static class Utils
{
    public static Dictionary<string, (Type, Type)> GetConsumerMappings()
    {
        return new Dictionary<string, (Type, Type)>
        {
            ["SubmitOrder"] = (typeof(SubmitOrderConsumer), typeof(SubmitOrderConsumerDefinition)),
            ["ShipmentCreated"] = (typeof(ShipmentCreatedConsumer), typeof(ShipmentCreatedConsumerDefinition))
            ...
        };
    }
}
```

Sample ShipmentCreatedConsumer.cs
```
internal class ShipmentCreatedConsumer : IConsumer<ShipmentCreatedEvent>
{
    public async Task Consume(ConsumeContext<ShipmentCreatedEvent> context)
    {
        var shipment = context.Message;
        // Process shipment creation logic
        Console.WriteLine($"Processing shipment: {shipment.ShipmentId} for order: {shipment.OrderId}");
        await Task.CompletedTask;
    }
}

internal class ShipmentCreatedConsumerDefinition : ConsumerDefinition<ShipmentCreatedConsumer>
{
    public ShipmentCreatedConsumerDefinition()
    {
        // override the default endpoint name
        EndpointName = "ShipmentCreated";

        // limit the number of messages consumed concurrently
        // this applies to the consumer only, not the endpoint
        ConcurrentMessageLimit = 2;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ShipmentCreatedConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Interval(4, TimeSpan.FromSeconds(7)));
        endpointConfigurator.PrefetchCount = 8;
    }
}
```


## Getting help

## Contributing