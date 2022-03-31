using System;
using Muflone.Core;
using Muflone.CustomTypes;

namespace Muflone.Messages.Events;

/// <summary>
/// Class DomainEvent
/// A domain-event is an indicator to interested parties that 'something has happened'. We expect zero to many receivers as it is one-to-many communication i.e. publish-subscribe
/// A domain-event is usually fire-and-forget, because we do not know it is received.
/// </summary>
public abstract class DomainEvent : Event, IDomainEvent
{
	protected DomainEvent(IDomainId aggregateId, Guid correlationId, Account who = default) : base(aggregateId,
		correlationId, who)
	{
	}

	protected DomainEvent(IDomainId aggregateId, Account who = default) : base(aggregateId, who)
	{
	}

	protected DomainEvent(IDomainId aggregateId) : base(aggregateId)
	{
	}

	protected DomainEvent(IDomainId aggregateId, Guid correlationId) : base(aggregateId, correlationId)
	{
	}

	protected DomainEvent(IDomainId aggregateId, Guid correlationId, Account who, When when) : base(aggregateId,
		correlationId, who, when)
	{
	}
}