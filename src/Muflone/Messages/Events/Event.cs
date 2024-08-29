using MassTransit;
using Muflone.Core;
using Muflone.CustomTypes;
using System;
using System.Collections.Generic;

namespace Muflone.Messages.Events;

public class Event : IEvent
{
	public IDomainId AggregateId { get; init; }
	public EventHeaders Headers { get; set; }

	public int Version { get; set; }
	public Guid MessageId { get; set; }
	public Dictionary<string, object> UserProperties { get; set; }

	protected Event(IDomainId aggregateId, Guid correlationId, Account who)
		: this(aggregateId, correlationId, who, new When(DateTime.UtcNow))
	{
	}

	protected Event(IDomainId aggregateId, Account who)
		: this(aggregateId, Guid.Empty, who, new When(DateTime.UtcNow))
	{
	}


	protected Event(IDomainId aggregateId)
		: this(aggregateId, Guid.Empty, new Account(Guid.Empty.ToString(), "Anonymous"), new When(DateTime.UtcNow))
	{
	}

	protected Event(IDomainId aggregateId, Guid correlationId)
		: this(aggregateId, correlationId, new Account(Guid.Empty.ToString(), "Anonymous"), new When(DateTime.UtcNow))
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
		//TODO: Delete Headers. Replace with Userprops and move them in Envelope away from here
		UserProperties = new Dictionary<string, object>
		{
			{ HeadersNames.CorrelationId, correlationId }
		};

		MessageId = NewId.NextGuid();
		AggregateId = aggregateId;
	}
}