using System;

namespace Muflone.Messages.Events
{
  public abstract class IntegrationEvent : IIntegrationEvent
  {
    public Guid AggregateId { get; }
    public EventHeaders Headers { get; set; }
    public int Version { get; set; }
    public Guid MessageId { get; set; }

    protected IntegrationEvent(Guid aggregateId, Guid correlationId, string who = "anonymous")
    {
      Headers = new EventHeaders()
      {
        Who = who,
        CorrelationId = correlationId,
        When = DateTime.UtcNow,
        AggregateType = GetType().Name
      };
      MessageId = Guid.NewGuid();
      AggregateId = aggregateId;
    }

    protected IntegrationEvent(Guid aggregateId, string who = "anonymous")
      : this(aggregateId, aggregateId, who)
    {

    }

    public string Who => Headers.Who;
  }
}