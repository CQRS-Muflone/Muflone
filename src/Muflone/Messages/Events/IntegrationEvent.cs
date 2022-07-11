using System;
using Muflone.Core;
using Muflone.CustomTypes;

namespace Muflone.Messages.Events;

public abstract class IntegrationEvent : Event, IIntegrationEvent
{
	protected IntegrationEvent(IDomainId aggregateId, Guid correlationId, Account who = default) : base(aggregateId,
		correlationId, who)
	{
	}

	protected IntegrationEvent(IDomainId aggregateId, Account who = default) : base(aggregateId, who)
	{
	}

	protected IntegrationEvent(IDomainId aggregateId) : base(aggregateId)
	{
	}

	protected IntegrationEvent(IDomainId aggregateId, Guid correlationId) : base(aggregateId, correlationId)
	{
	}

	protected IntegrationEvent(IDomainId aggregateId, Guid correlationId, Account who, When when) : base(aggregateId,
		correlationId, who, when)
	{
	}
}