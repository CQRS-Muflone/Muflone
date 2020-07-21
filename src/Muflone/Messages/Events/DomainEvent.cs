using System;
using Muflone.Core;

namespace Muflone.Messages.Events
{
	public abstract class DomainEvent : Event, IDomainEvent
	{
		protected DomainEvent(IDomainId aggregateId, Guid correlationId, string who = "anonymous") : base(aggregateId, correlationId, who)
		{
		}

		protected DomainEvent(IDomainId aggregateId, string who = "anonymous") : base(aggregateId, who)
		{
		}
	}
}