using System;
using System.Collections.Generic;
using MassTransit;
using Muflone.Core;
using Muflone.CustomTypes;

namespace Muflone.Messages.Events;

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
		: this(aggregateId, Guid.Empty, who, new When(DateTime.UtcNow))
	{
	}


	protected Event(IDomainId aggregateId)
		: this(aggregateId, Guid.Empty, new Account(NewId.NextGuid().ToString(), "Anonymous"),
			new When(DateTime.UtcNow))
	{
	}

	protected Event(IDomainId aggregateId, Guid correlationId)
		: this(aggregateId, correlationId, new Account(NewId.NextGuid().ToString(), "Anonymous"),
			new When(DateTime.UtcNow))
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
		//TODO: Delete Headers. Replace with Userprops and move them in Envelope. away from here
		UserProperties = new Dictionary<string, object>();
		UserProperties.Add(HeadersNames.CorrelationId, correlationId);

		MessageId = NewId.NextGuid();
		AggregateId = aggregateId;
	}
}