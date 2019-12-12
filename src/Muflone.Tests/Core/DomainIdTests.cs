using System;
using Muflone.Core;
using Xunit;

namespace Muflone.Tests.Core
{
	public class DomainIdTests
	{
		private class FakeDomainId : DomainId
		{
			public FakeDomainId(Guid id) : base(id)
			{
			}
		}

		private class DifferentFakeDomainId : DomainId
		{
			public DifferentFakeDomainId(Guid id) : base(id)
			{
			}
		}


		[Fact]
		public void Equals_DifferentClassesSameIds_ReturnsFalse()
		{
			var id = Guid.NewGuid();
			var fake1 = new FakeDomainId(id);
			var fake2 = new DifferentFakeDomainId(id);
			Assert.False(fake1 == fake2);
		}

		[Fact]
		public void Equals_SameClassesDifferentId_ReturnsFalse()
		{
			var fake1 = new FakeDomainId(Guid.NewGuid());
			var fake2 = new FakeDomainId(Guid.NewGuid());
			Assert.NotEqual(fake1, fake2);
		}

		[Fact]
		public void Equals_SameClassesSameIds_ReturnsTrue()
		{
			var id = Guid.NewGuid();
			var fake1 = new FakeDomainId(id);
			var fake2 = new FakeDomainId(id);
			Assert.Equal(fake1, fake2);
		}
	}
}