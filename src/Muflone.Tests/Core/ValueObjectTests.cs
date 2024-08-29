using Muflone.Core;
using System.Collections.Generic;
using Xunit;

namespace Muflone.Tests.Core
{
	public class ValueObjectTests
	{
		private class MyValueObject : ValueObject
		{
			public string Field1 { get; }
			public string Field2 { get; }

			public MyValueObject(string field1, string field2)
			{
				Field1 = field1;
				Field2 = field2;
			}

			protected override IEnumerable<object?> GetEqualityComponents()
			{
				yield return Field1;
				yield return Field2;
			}
		}


		[Fact]
		public void ValueObject_Equals_ReturnsTrue()
		{
			var vo1 = new MyValueObject("Field1", "Field2");
			var vo2 = new MyValueObject("Field1", "Field2");

			Assert.True(vo1.Equals(vo2));
		}

		[Fact]
		public void ValueObject_OneIsNull_ReturnsFalse()
		{
			MyValueObject? vo1 = null;
			var vo2 = new MyValueObject("Field1", "Field2");

			Assert.False(vo1 == vo2);
		}

		[Fact]
		public void ValueObject_BothNull_ReturnsTrue()
		{
			MyValueObject? vo1 = null;
			MyValueObject? vo2 = null;

			Assert.True(vo1 == vo2);
		}
	}
}
