using Fisdane.RabbitMQ.Extensions;
using MassTransit;
using System.Collections.Concurrent;

namespace Fisdane.RabbitMQ.Configuration;

public interface ISendMessage
{
    Task SendMessageAsync<T>(string queue, T message, int timeoutSeconds = 0);
}

internal sealed class SendMessage(ISendEndpointProvider sendEndpointProvider) : ISendMessage
{
    private readonly ISendEndpointProvider _sendEndpointProvider = sendEndpointProvider;
    private readonly ConcurrentDictionary<string, ISendEndpoint> _endpointCache = new();

    public async Task SendMessageAsync<T>(string queue, T message, int timeoutSeconds = 0)
    {
        var endpoint = await GetSendEndpoint(queue).ConfigureAwait(false);

        if (timeoutSeconds > 0)
        {
            using CancellationTokenSource source = new(TimeSpan.FromSeconds(timeoutSeconds));
            await endpoint.Send(message, source.Token).ConfigureAwait(false);
        }
        else
        {
            await endpoint.Send(message).ConfigureAwait(false);
        }
    }

    private async Task<ISendEndpoint> GetSendEndpoint(string queueName)
    {
        Uri address = queueName.GetQueueUri();
        string key = address.AbsoluteUri;

        // Use GetOrAdd for cleaner atomic operation
        if (!_endpointCache.TryGetValue(key, out var endpoint))
        {
            endpoint = await _sendEndpointProvider.GetSendEndpoint(address).ConfigureAwait(false);
            _endpointCache.TryAdd(key, endpoint);
        }

        return endpoint;
    }
}