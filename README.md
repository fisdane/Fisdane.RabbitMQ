
# Fisdane.RabbitMQ


**Fisdane.RabbitMQ** is a lightweight wrapper around `MassTransit.RabbitMQ`, designed to simplify integration with background services—removing boilerplate and reducing setup complexity.

---

## ✨ Features

- First-class testing support
- Easily integrate RabbitMQ into .NET background services
- Write once, deploy anywhere
- Fully supported and production-ready
- Clean abstraction with minimal overhead

---

## 📚 Documentation

Get started by [reading the full documentation](https://fisdane.github.io/Fisdane.RabbitMQ/).

---

## 📦 Installation

Install via NuGet:

```bash
dotnet add package Fisdane.RabbitMQ
```

---


## 🚀 Getting Started

The easiest way to use **Fisdane.RabbitMQ** is via the `RegisterConsumers` extension method to set up background workers that consume from RabbitMQ, or `RegisterPublisher` to configure services as producers of events or messages.

##### Sample `Program.cs`

```csharp
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

// Replace 'GetConsumerMappings' with your own consumer mapping method
builder.Services.RegisterConsumers(builder.Configuration, Utils.GetConsumerMappings);

var host = builder.Build();
host.Run();
```



### ⚙️ Configuration

Your service **must** provide RabbitMQ configuration via `appsettings.json`.

##### Sample `appsettings.json`

```json
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
}
```
The `EndpointNameFormatter` can take one of the following values:

- `KebabCase`
- `SnakeCase`
- `CamelCase`

This controls the format of queue and endpoint names.

---


### 🧠 Consumer Mapping

The method `GetConsumerMappings` is essential—it maps enabled consumer names (from config) to their respective consumer and definition types. Method name can be anything you see fit but must return a `Dictionary<string, (Type, Type)>`

##### Sample `Utils.cs`

```csharp
internal static class Utils
{
    public static Dictionary<string, (Type, Type)> GetConsumerMappings()
    {
        return new Dictionary<string, (Type, Type)>
        {
            ["SubmitOrder"] = (typeof(SubmitOrderConsumer), typeof(SubmitOrderConsumerDefinition)),
            ["ShipmentCreated"] = (typeof(ShipmentCreatedConsumer), typeof(ShipmentCreatedConsumerDefinition))
        };
    }
}
```

> 🔑 **Note:** Keys in this dictionary **must** match the names listed under `EnabledConsumers` in `appsettings.json`.



### 📨 Sample Consumer Implementation

##### `ShipmentCreatedConsumer.cs`

```csharp
internal class ShipmentCreatedConsumer : IConsumer<ShipmentCreatedEvent>
{
    public async Task Consume(ConsumeContext<ShipmentCreatedEvent> context)
    {
        var shipment = context.Message;
        Console.WriteLine($"Processing shipment: {shipment.ShipmentId} for order: {shipment.OrderId}");
        await Task.CompletedTask;
    }
}

internal class ShipmentCreatedConsumerDefinition : ConsumerDefinition<ShipmentCreatedConsumer>
{
    public ShipmentCreatedConsumerDefinition()
    {
        EndpointName = "ShipmentCreated";
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

---


## ✅ Build Status

| Branch | Status |
|--------|--------|
| `master` | [![Fisdane.RabbitMQ](https://github.com/fisdane/Fisdane.RabbitMQ/actions/workflows/build.yml/badge.svg)](https://github.com/fisdane/Fisdane.RabbitMQ/actions/workflows/build.yml) |

---

## 🙋 Getting Help

For questions, bug reports, or feature requests, feel free to [open an issue](https://github.com/fisdane/Fisdane.RabbitMQ/issues).

---

## 🤝 Contributing

We welcome contributions! Please see the [contributing guidelines](CONTRIBUTING.md) for more details.

---

<!-- ## 📄 License

This project is licensed under the [MIT License](LICENSE). -->
<!-- --- -->

## 💬 Support

If you find this project helpful, consider giving it a ⭐ on GitHub or sharing it with others.