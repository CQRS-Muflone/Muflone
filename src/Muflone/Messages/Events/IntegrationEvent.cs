using System;
using Muflone.Core;

namespace Muflone.Messages.Events
{
	public abstract class IntegrationEvent : Event, IIntegrationEvent
	{
		protected IntegrationEvent(IDomainId aggregateId, Guid correlationId, string who = "anonymous") : base(aggregateId, correlationId, who)
		{
		}

		protected IntegrationEvent(IDomainId aggregateId, string who = "anonymous") : base(aggregateId, who)
		{
		}
	}
}