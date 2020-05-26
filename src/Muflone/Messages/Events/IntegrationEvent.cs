using System;
using Muflone.Core;

namespace Muflone.Messages.Events
{
	public abstract class IntegrationEvent : IIntegrationEvent
	{
		public IDomainId AggregateId { get; }
		public EventHeaders Headers { get; set; }
		public int Version { get; set; }
		public Guid MessageId { get; set; }

		protected IntegrationEvent(IDomainId aggregateId, Guid correlationId, string who = "anonymous")
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

		protected IntegrationEvent(IDomainId aggregateId, string who = "anonymous")
			: this(aggregateId, Guid.NewGuid(), who)
		{

		}

		public string Who => Headers.Who;
	}
}