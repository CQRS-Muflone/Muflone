using Muflone.Core;
using Xunit;

namespace Muflone.Tests.Core
{
	public class ExceptionsTests
	{
		private class TestDomainId : DomainId
		{
			public TestDomainId(string value) : base(value)
			{
			}
		}

		[Fact]
		public void AggregateDeletedException_Constructor_SetsProperties()
		{
			// Arrange
			var id = new TestDomainId("123");
			var aggregateType = typeof(TestDomainId);

			// Act
			var exception = new AggregateDeletedException(id, aggregateType);

			// Assert
			Assert.Equal(id, exception.Id);
			Assert.Equal(aggregateType, exception.Type);
			Assert.Contains("was deleted", exception.Message);
			Assert.Contains(id.Value, exception.Message);
			Assert.Contains(aggregateType.Name, exception.Message);
		}

		[Fact]
		public void AggregateNotFoundException_Constructor_SetsProperties()
		{
			// Arrange
			var id = new TestDomainId("123");
			var aggregateType = typeof(TestDomainId);

			// Act
			var exception = new AggregateNotFoundException(id, aggregateType);

			// Assert
			Assert.Equal(id, exception.Id);
			Assert.Equal(aggregateType, exception.Type);
			Assert.Contains("was not found", exception.Message);
			Assert.Contains(id.Value, exception.Message);
			Assert.Contains(aggregateType.Name, exception.Message);
		}

		[Fact]
		public void AggregateVersionException_Constructor_SetsProperties()
		{
			// Arrange
			var id = new TestDomainId("123");
			var aggregateType = typeof(TestDomainId);
			const long aggregateVersion = 5;
			const long requestedVersion = 10;

			// Act
			var exception = new AggregateVersionException(id, aggregateType, aggregateVersion, requestedVersion);

			// Assert
			Assert.Equal(id, exception.Id);
			Assert.Equal(aggregateType, exception.Type);
			Assert.Equal(aggregateVersion, exception.AggregateVersion);
			Assert.Equal(requestedVersion, exception.RequestedVersion);
			Assert.Contains("Requested version", exception.Message);
			Assert.Contains(requestedVersion.ToString(), exception.Message);
			Assert.Contains(aggregateVersion.ToString(), exception.Message);
			Assert.Contains(id.Value, exception.Message);
			Assert.Contains(aggregateType.Name, exception.Message);
		}

		[Fact]
		public void HandlerForDomainEventNotFoundException_Constructor_SetsProperties()
		{
			// Arrange
			var eventTypeName = typeof(TestDomainId).FullName;

			// Act
			var exception = new HandlerForDomainEventNotFoundException($"Handler not found for event type {eventTypeName}");

			// Assert
			Assert.Contains("Handler not found", exception.Message);
			Assert.Contains(eventTypeName, exception.Message);
		}
	}
}