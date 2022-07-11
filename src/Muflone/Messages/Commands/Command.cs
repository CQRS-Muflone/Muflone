using System;
using System.Collections.Generic;
using MassTransit;
using Muflone.Core;
using Muflone.CustomTypes;

namespace Muflone.Messages.Commands;

/// <summary>
/// A command is an imperative instruction to do something. We expect only one receiver of a command because it is point-to-point
/// </summary>
public class Command : ICommand
{
	public DomainId AggregateId { get; set; }
	public Guid MessageId { get; set; }
	public Dictionary<string, object> UserProperties { get; set; }
	public Account Who { get; }
	public When When { get; }

	protected Command(DomainId aggregateId)
	{
		AggregateId = aggregateId;
		MessageId = Guid.NewGuid();
		UserProperties = new Dictionary<string, object>();
		Who = new Account(NewId.NextGuid().ToString(), "Anonymous");
		When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
	}

	protected Command(DomainId aggregateId, Guid commitId)
	{
		AggregateId = aggregateId;
		MessageId = commitId;
		UserProperties = new Dictionary<string, object>();
		Who = new Account(NewId.NextGuid().ToString(), "Anonymous");
		When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
	}

	protected Command(DomainId aggregateId, Account who)
	{
		AggregateId = aggregateId;
		MessageId = Guid.NewGuid();
		UserProperties = new Dictionary<string, object>();
		Who = who;
		When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
	}

	protected Command(DomainId aggregateId, Account who, When when)
	{
		AggregateId = aggregateId;
		MessageId = Guid.NewGuid();
		UserProperties = new Dictionary<string, object>();
		Who = who;
		When = when;
	}

	protected Command(DomainId aggregateId, Guid commitId, Account who)
	{
		AggregateId = aggregateId;
		MessageId = commitId;
		UserProperties = new Dictionary<string, object>();
		Who = who;
		When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
	}

	protected Command(DomainId aggregateId, Guid commitId, Account who, When when)
	{
		AggregateId = aggregateId;
		MessageId = commitId;
		UserProperties = new Dictionary<string, object>();
		Who = who;
		When = when;
	}
}