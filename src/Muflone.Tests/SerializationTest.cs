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
		var accountId = Guid.NewGuid().ToString();
		var stateUpdated = new StateUpdated(new StateId(Guid.NewGuid()), new Account(accountId, "Name"));
		
		Assert.False(stateUpdated.Headers.When.Value > DateTimeOffset.UtcNow);
		Assert.Equal(accountId, stateUpdated.Headers.Who.Id);
	}
}

public class StateUpdated : DomainEvent
{
	public readonly StateId StateId;

	public StateUpdated(StateId aggregateId, Account who) : base(aggregateId, who)
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