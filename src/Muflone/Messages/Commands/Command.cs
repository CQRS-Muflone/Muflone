using MassTransit;
using Muflone.Core;
using Muflone.CustomTypes;
using System;
using System.Collections.Generic;

namespace Muflone.Messages.Commands;

/// <summary>
/// A command is an imperative instruction to do something. We expect only one receiver of a command because it is point-to-point
/// </summary>
public abstract class Command(IDomainId aggregateId, Guid commitId, Account who, When when)
    : ICommand
{
    public IDomainId AggregateId { get; set; } = aggregateId;
    public Guid MessageId { get; set; } = commitId;
    public Dictionary<string, object> UserProperties { get; set; } = new();
    public Account Who { get; } = who;
    public When When { get; } = when;

    protected Command(IDomainId aggregateId)
        : this(aggregateId, NewId.NextGuid(), new Account(NewId.NextGuid().ToString(), "Anonymous"),
            new When(DateTime.UtcNow))
    {
    }

    protected Command(IDomainId aggregateId, Guid commitId)
        : this(aggregateId, commitId, new Account(NewId.NextGuid().ToString(), "Anonymous"),
            new When(DateTime.UtcNow))
    {
    }

    protected Command(IDomainId aggregateId, Account who)
        : this(aggregateId, NewId.NextGuid(), who, new When(DateTime.UtcNow))
    {
    }

    protected Command(IDomainId aggregateId, Account who, When when)
        : this(aggregateId, NewId.NextGuid(), who, when)
    {
    }

    protected Command(IDomainId aggregateId, Guid commitId, Account who)
        : this(aggregateId, commitId, who, new When(DateTime.UtcNow))
    {
    }
}