using Fisdane.RabbitMQ.Extensions;
using MassTransit;
using System.Collections.Concurrent;

namespace Fisdane.RabbitMQ.Configuration;

public interface ISendEndPoint : ISendEndpointProvider
{
    /// <summary>
    /// Overrides GetSendEndpoint in ISendEndpointProvider
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    new Task<ISendEndpoint> GetSendEndpoint(Uri address);

    Task<ISendEndpoint> GetSendEndpoint(string queueName);
}

internal sealed class SendEndPoint(IBus bus) : ISendEndPoint
{
    private readonly ConcurrentDictionary<string, ISendEndpoint> _endpointCache = new();
    private readonly IBus _bus = bus;

    public async Task<ISendEndpoint> GetSendEndpoint(Uri address)
    {
        string key = address.AbsoluteUri;

        // Use GetOrAdd for cleaner atomic operation
        if (!_endpointCache.TryGetValue(key, out var endpoint))
        {
            endpoint = await _bus.GetSendEndpoint(address).ConfigureAwait(false);
            _endpointCache.TryAdd(key, endpoint);
        }

        return endpoint;
    }

    public Task<ISendEndpoint> GetSendEndpoint(string queueName)
    {
        return GetSendEndpoint(queueName.GetQueueUri());
    }

    public ConnectHandle ConnectSendObserver(ISendObserver observer)
    {
        throw new NotImplementedException();
    }
}