using System;
using System.Collections.Generic;
using MassTransit;
using Muflone.Core;
using Muflone.CustomTypes;
using Muflone.Factories;

namespace Muflone.Messages.Commands;

/// <summary>
/// A command is an imperative instruction to do something. We expect only one receiver of a command because it is point-to-point
/// </summary>
public class Command : ICommand
{
    public DomainId AggregateId { get; set; }
    public Guid MessageId { get; set; }
    public Dictionary<string, object> UserProperties { get; set;  }
    public AccountInfo Who { get; }
    public When When { get; }

    protected Command(DomainId aggregateId)
    {
        AggregateId = aggregateId;
        MessageId = GuidExtension.GetNewGuid();
        UserProperties = new Dictionary<string, object>();
        Who = new AccountInfo(new AccountId(NewId.NextGuid()), new AccountName("Anonymous"));
        When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    protected Command(DomainId aggregateId, Guid commitId)
    {
        AggregateId = aggregateId;
        MessageId = commitId;
        UserProperties = new Dictionary<string, object>();
        Who = new AccountInfo(new AccountId(NewId.NextGuid()), new AccountName("Anonymous"));
        When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    protected Command(DomainId aggregateId, AccountInfo who)
    {
        AggregateId = aggregateId;
        MessageId = GuidExtension.GetNewGuid();
        UserProperties = new Dictionary<string, object>();
        Who = who;
        When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    protected Command(DomainId aggregateId, AccountInfo who, When when)
    {
        AggregateId = aggregateId;
        MessageId = GuidExtension.GetNewGuid();
        UserProperties = new Dictionary<string, object>();
        Who = who;
        When = when;
    }

    protected Command(DomainId aggregateId, Guid commitId, AccountInfo who)
    {
        AggregateId = aggregateId;
        MessageId = commitId;
        UserProperties = new Dictionary<string, object>();
        Who = who;
        When = new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    protected Command(DomainId aggregateId, Guid commitId, AccountInfo who, When when)
    {
        AggregateId = aggregateId;
        MessageId = commitId;
        UserProperties = new Dictionary<string, object>();
        Who = who;
        When = when;
    }
}