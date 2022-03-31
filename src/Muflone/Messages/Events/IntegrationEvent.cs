using System;
using System.Collections.Generic;
using MassTransit;
using Muflone.CustomTypes;
using Muflone.Factories;

namespace Muflone.Messages.Events;

public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid AggregateId { get; }
    public EventHeaders Headers { get; set; }
    public int Version { get; set; }
    public Guid MessageId { get; set; }
    public Dictionary<string, object> UserProperties { get; set; }

    protected IntegrationEvent(Guid aggregateId, Guid correlationId)
    {
        Headers = new EventHeaders
        {
            Who = new Account(NewId.NextGuid().ToString(), "Anonymous"),
            CorrelationId = correlationId,
            When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            AggregateType = GetType().Name
        };
        MessageId = GuidExtension.GetNewGuid();
        AggregateId = aggregateId;
    }

    protected IntegrationEvent(Guid aggregateId)
        : this(aggregateId, aggregateId)
    {
    }
}