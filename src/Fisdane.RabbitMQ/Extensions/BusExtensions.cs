namespace Fisdane.RabbitMQ.Extensions;

public static class BusExtensions
{
    /// <summary>
    /// Returns a send endpoint for the [queueName] exchange, which would be bound to the [queueName] queue.
    /// </summary>
    /// <param name="queueName"></param>
    /// <returns></returns>
    public static Uri GetQueueUri(this string queueName)
        => new($"queue:{queueName}");

    public static Uri GetExchangeUri(this string exchangeName)
        => new($"exchange:{exchangeName}");
}