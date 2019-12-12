using System;
using Muflone.Core;
using Xunit;

namespace Muflone.Tests.Core
{
	public class AggregateRootTests
	{
		private class FakeAggregateRoot : AggregateRoot
		{
			public FakeAggregateRoot(IDomainId id)
			{
				Id = id;
			}
		}

		private class DifferentFakeAggregateRoot : AggregateRoot
		{
			public DifferentFakeAggregateRoot(IDomainId id)
			{
				Id = id;
			}
		}

		private class FakeDomainId : DomainId
		{
			public FakeDomainId(Guid id) : base(id)
			{
			}
		}

		[Fact]
		public void Equals_DifferentClassesSameIds_ReturnsFalse()
		{
			var id = new FakeDomainId(Guid.NewGuid());
			var fake1 = new FakeAggregateRoot(id);
			var fake2 = new DifferentFakeAggregateRoot(id);
			Assert.False(fake1 == fake2);
		}

		[Fact]
		public void Equals_SameClassesDifferentId_ReturnsFalse()
		{
			var fake1 = new FakeAggregateRoot(new FakeDomainId(Guid.NewGuid()));
			var fake2 = new FakeAggregateRoot(new FakeDomainId(Guid.NewGuid()));
			Assert.NotEqual(fake1, fake2);
		}

		[Fact]
		public void Equals_SameClassesSameIds_ReturnsTrue()
		{
			var id = new FakeDomainId(Guid.NewGuid());
			var fake1 = new FakeAggregateRoot(id);
			var fake2 = new FakeAggregateRoot(id);
			Assert.Equal(fake1, fake2);
		}
	}
}