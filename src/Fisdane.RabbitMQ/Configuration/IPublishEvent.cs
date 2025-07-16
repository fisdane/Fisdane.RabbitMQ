using MassTransit;

namespace Fisdane.RabbitMQ.Configuration;

public interface IPublishEvent
{
    Task Publish<T>(object _event, CancellationToken cancellationToken = default);

    Task Publish<T>(object _event, Type eventType, CancellationToken cancellationToken = default);
}

internal sealed class PublishEvent(IPublishEndpoint publishEndpoint) : IPublishEvent
{
    public async Task Publish<T>(object _event, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(_event, cancellationToken: cancellationToken);
    }

    public async Task Publish<T>(object _event, Type eventType, CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(_event, eventType, cancellationToken: cancellationToken);
    }
}