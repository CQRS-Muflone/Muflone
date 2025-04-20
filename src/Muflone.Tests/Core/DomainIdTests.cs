using Muflone.Core;
using Xunit;

namespace Muflone.Tests.Core
{
	public class DomainIdTests
	{
		private class TestDomainId : DomainId
		{
			public TestDomainId(string value) : base(value)
			{
			}
		}

		[Fact]
		public void DomainId_Equals_ReturnsTrue()
		{
			var id1 = new TestDomainId("123");
			var id2 = new TestDomainId("123");

			Assert.True(id1.Equals(id2));
		}

		[Fact]
		public void DomainId_Equals_ReturnsFalse_WhenValuesDiffer()
		{
			var id1 = new TestDomainId("123");
			var id2 = new TestDomainId("456");

			Assert.False(id1.Equals(id2));
		}

		[Fact]
		public void DomainId_EqualityOperator_ReturnsTrue()
		{
			var id1 = new TestDomainId("123");
			var id2 = new TestDomainId("123");

			Assert.True(id1 == id2);
		}

		[Fact]
		public void DomainId_EqualityOperator_ReturnsFalse_WhenValuesDiffer()
		{
			var id1 = new TestDomainId("123");
			var id2 = new TestDomainId("456");

			Assert.False(id1 == id2);
		}

		[Fact]
		public void DomainId_InequalityOperator_ReturnsFalse()
		{
			var id1 = new TestDomainId("123");
			var id2 = new TestDomainId("123");

			Assert.False(id1 != id2);
		}

		[Fact]
		public void DomainId_InequalityOperator_ReturnsTrue_WhenValuesDiffer()
		{
			var id1 = new TestDomainId("123");
			var id2 = new TestDomainId("456");

			Assert.True(id1 != id2);
		}

		[Fact]
		public void DomainId_BothNull_ReturnsTrue()
		{
			TestDomainId? id1 = null;
			TestDomainId? id2 = null;

			Assert.True(id1 == id2);
		}

		[Fact]
		public void DomainId_OneIsNull_ReturnsFalse()
		{
			TestDomainId? id1 = null;
			var id2 = new TestDomainId("123");

			Assert.False(id1 == id2);
		}

		[Fact]
		public void DomainId_GetHashCode_ReturnsValueHashCode()
		{
			string value = "123";
			var id = new TestDomainId(value);

			Assert.Equal(value.GetHashCode(), id.GetHashCode());
		}

		[Fact]
		public void DomainId_ToString_ReturnsValue()
		{
			string value = "123";
			var id = new TestDomainId(value);

			Assert.Equal(value, id.ToString());
		}
	}
}