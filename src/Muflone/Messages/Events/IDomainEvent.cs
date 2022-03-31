using Muflone.CustomTypes;

namespace Muflone.Messages.Events;

public interface IDomainEvent : IEvent
{
    Account Who { get; }
    When When { get; }
    int Version { get; }
}