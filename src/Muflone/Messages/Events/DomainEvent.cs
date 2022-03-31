using System;
using System.Collections.Generic;
using MassTransit;
using Muflone.Core;
using Muflone.CustomTypes;
using Muflone.Factories;

namespace Muflone.Messages.Events;

/// <summary>
/// Class DomainEvent
/// A domain-event is an indicator to interested parties that 'something has happened'. We expect zero to many receivers as it is one-to-many communication i.e. publish-subscribe
/// A domain-event is usually fire-and-forget, because we do not know it is received.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public DomainId AggregateId { get; }
    public EventHeaders Headers { get; set; }
    public Guid MessageId { get; set; }
    public Dictionary<string, object> UserProperties { get; set; }
    public int Version { get; set; }
    public Account Who => Headers.Who;
    public When When => Headers.When;

    protected DomainEvent(DomainId aggregateId)
    {
        Headers = new EventHeaders
        {
            CorrelationId = NewId.NextGuid(),
            AggregateType = GetType().Name,
            Who = new Account(NewId.NextGuid().ToString(), "Anonymous"),
            When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };
        MessageId = GuidExtension.GetNewGuid();
        AggregateId = aggregateId;
    }

    protected DomainEvent(DomainId aggregateId, Guid correlationId)
    {
        Headers = new EventHeaders
        {
            CorrelationId = correlationId,
            AggregateType = GetType().Name,
            Who = new Account(NewId.NextGuid().ToString(), "Anonymous"),
            When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };
        MessageId = GuidExtension.GetNewGuid();
        AggregateId = aggregateId;
    }

    protected DomainEvent(DomainId aggregateId, Account who)
    {
        Headers = new EventHeaders
        {
            CorrelationId = GuidExtension.GetNewGuid(),
            AggregateType = GetType().Name,
            Who = who,
            When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };
        MessageId = GuidExtension.GetNewGuid();
        AggregateId = aggregateId;
    }

    protected DomainEvent(DomainId aggregateId, Guid correlationId, Account who)
    {
        Headers = new EventHeaders
        {
            CorrelationId = correlationId,
            AggregateType = GetType().Name,
            Who = who,
            When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };
        MessageId = GuidExtension.GetNewGuid();
        AggregateId = aggregateId;
    }

    protected DomainEvent(DomainId aggregateId, Guid correlationId, Account who, When when)
    {
        Headers = new EventHeaders
        {
            CorrelationId = correlationId,
            AggregateType = GetType().Name,
            Who = who,
            When = when
        };
        MessageId = GuidExtension.GetNewGuid();
        AggregateId = aggregateId;
    }

    protected DomainEvent(DomainId aggregateId, Account who, When when)
        : this(aggregateId, aggregateId.Value, who, when)
    {
    }
}