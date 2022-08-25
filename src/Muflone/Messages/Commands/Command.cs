using System;
using System.Collections.Generic;
using MassTransit;
using Muflone.Core;
using Muflone.CustomTypes;

namespace Muflone.Messages.Commands;

/// <summary>
/// A command is an imperative instruction to do something. We expect only one receiver of a command because it is point-to-point
/// </summary>
public abstract class Command : ICommand
{
	public IDomainId AggregateId { get; set; }
	public Guid MessageId { get; set; }
	public Dictionary<string, object> UserProperties { get; set; }
	public Account Who { get; }
	public When When { get; }

	protected Command(IDomainId aggregateId)
		: this(aggregateId, Guid.NewGuid(), new Account(NewId.NextGuid().ToString(), "Anonymous"),
			new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
	{
	}

	protected Command(IDomainId aggregateId, Guid commitId)
		: this(aggregateId, commitId, new Account(NewId.NextGuid().ToString(), "Anonymous"),
			new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
	{
	}

	protected Command(IDomainId aggregateId, Account who)
		: this(aggregateId, Guid.NewGuid(), who, new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
	{
	}

	protected Command(IDomainId aggregateId, Account who, When when)
		: this(aggregateId, Guid.NewGuid(), who, when)
	{
	}

	protected Command(IDomainId aggregateId, Guid commitId, Account who)
		: this(aggregateId, commitId, who, new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
	{
	}

	protected Command(IDomainId aggregateId, Guid commitId, Account who, When when)
	{
		AggregateId = aggregateId;
		MessageId = commitId;
		UserProperties = new Dictionary<string, object>();
		Who = who;
		When = when;
	}
}