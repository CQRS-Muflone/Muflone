using Muflone.Persistence;

namespace Muflone;

public class HandlerConfiguration
{
    public string? InstanceId { get; set; }
    public string? QueueName { get; set; }
    public string? RoutingKey { get; set; }
    public ISerializer? CustomMessageSerializer { get; set; }
}