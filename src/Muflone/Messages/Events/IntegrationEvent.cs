﻿using Muflone.Core;
using Muflone.CustomTypes;
using System;

namespace Muflone.Messages.Events;

public abstract class IntegrationEvent : Event, IIntegrationEvent
{
	protected IntegrationEvent(IDomainId aggregateId, Guid correlationId, Account who) : base(aggregateId, correlationId, who)
	{
	}

	protected IntegrationEvent(IDomainId aggregateId, Account who) : base(aggregateId, who)
	{
	}

	protected IntegrationEvent(IDomainId aggregateId) : base(aggregateId)
	{
	}

	protected IntegrationEvent(IDomainId aggregateId, Guid correlationId) : base(aggregateId, correlationId)
	{
	}

	protected IntegrationEvent(IDomainId aggregateId, Guid correlationId, Account who, When when) : base(aggregateId, correlationId, who, when)
	{
	}
}