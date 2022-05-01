using System;
using System.Collections.Generic;
using MassTransit;
using Muflone.Core;
using Muflone.CustomTypes;

namespace Muflone.Messages.Events
{
	public class Event : IEvent
	{
		public IDomainId AggregateId { get; init; }
		public EventHeaders Headers { get; set; }

		public int Version { get; set; }
		public Guid MessageId { get; set; }
		public Dictionary<string, object> UserProperties { get; set; }

		protected Event(IDomainId aggregateId, Guid correlationId, Account who = default)
			: this(aggregateId, correlationId, who, new When(DateTime.UtcNow))
		{
		}

		protected Event(IDomainId aggregateId, Account who = default)
			: this(aggregateId, Guid.NewGuid(), who, new When(DateTime.UtcNow))
		{
		}


		protected Event(IDomainId aggregateId)
			: this(aggregateId, Guid.NewGuid(), new Account(NewId.NextGuid().ToString(), "Anonymous"), new When(DateTime.UtcNow))
		{
		}

		protected Event(IDomainId aggregateId, Guid correlationId)
			: this(aggregateId, correlationId, new Account(NewId.NextGuid().ToString(), "Anonymous"), new When(DateTime.UtcNow))
		{
		}

		protected Event(IDomainId aggregateId, Guid correlationId, Account who, When when)
		{
			Headers = new EventHeaders
			{
				CorrelationId = correlationId,
				AggregateType = GetType().Name,
				Who = who,
				When = when
			};
			MessageId = Guid.NewGuid();
			AggregateId = aggregateId;
		}
	}
}