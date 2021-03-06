using System;
using Muflone.Core;
using Xunit;

namespace Muflone.Tests.Core
{
	public class EntityTests
	{
		private class FakeEntity : Entity
		{
			public FakeEntity(DomainId id) : base(id)
			{
			}
		}

		private class DifferentFakeEntity : Entity
		{
			public DifferentFakeEntity(DomainId id) : base(id)
			{
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
			var fake1 = new FakeEntity(id);
			var fake2 = new DifferentFakeEntity(id);
			Assert.False(fake1 == fake2);
		}

		[Fact]
		public void Equals_SameClassesDifferentId_ReturnsFalse()
		{
			var fake1 = new FakeEntity(new FakeDomainId(Guid.NewGuid()));
			var fake2 = new FakeEntity(new FakeDomainId(Guid.NewGuid()));
			Assert.NotEqual(fake1, fake2);
		}

		[Fact]
		public void Equals_SameClassesSameIds_ReturnsTrue()
		{
			var id = new FakeDomainId(Guid.NewGuid());
			var fake1 = new FakeEntity(id);
			var fake2 = new FakeEntity(id);
			Assert.Equal(fake1, fake2);
		}
	}
}