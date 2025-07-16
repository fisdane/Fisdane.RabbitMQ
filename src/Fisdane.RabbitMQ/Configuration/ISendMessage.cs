using Fisdane.RabbitMQ.Extensions;
using MassTransit;

namespace Fisdane.RabbitMQ.Configuration;

public interface ISendMessage
{
    Task SendMessageAsync<T>(string queue, T message, int timeoutSeconds = 0);
}

internal sealed class SendMessage(ISendEndpointProvider sendEndpointProvider) : ISendMessage
{
    public async Task SendMessageAsync<T>(string queue, T message, int timeoutSeconds = 0)
    {
        Uri address = queue.GetQueueUri();
        ISendEndpoint endpoint = await sendEndpointProvider.GetSendEndpoint(address).ConfigureAwait(false);

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
}