using MassTransit;

namespace Test.Worker;

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