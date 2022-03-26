using System;
using System.Threading;
using Muflone.Core;
using Muflone.CustomTypes;
using Muflone.Messages.Events;
using Xunit;

namespace Muflone.Tests;

public class SerializationTest
{
    [Fact]
    public void Can_SerializeAndDeserialize_Event()
    {
        var accountId = new AccountId(Guid.NewGuid());

        var stateUpdated = new StateUpdated(new StateId(Guid.NewGuid()),
            new AccountInfo(accountId, new AccountName("Name")));

        Thread.Sleep(200);

        Assert.False(stateUpdated.When.Value > DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        Assert.Equal(accountId, stateUpdated.Who.AccountId);
    }
}

public class StateUpdated : DomainEvent
{
    public readonly StateId StateId;

    public StateUpdated(StateId aggregateId, AccountInfo who) : base(aggregateId, who,
        new When(DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
    {
        StateId = aggregateId;
    }
}

public class StateId : DomainId
{
    public StateId(Guid value) : base(value)
    {
    }
}