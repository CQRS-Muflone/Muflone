using Muflone.Core;
using System.Linq;
using Xunit;

namespace Muflone.Tests.Core
{
    public class AggregateRootTests
    {
        private class TestDomainId : DomainId
        {
            public TestDomainId(string value) : base(value)
            {
            }
        }

        private class TestEvent
        {
            public string Value { get; }

            public TestEvent(string value)
            {
                Value = value;
            }
        }

        private class TestAggregate : AggregateRoot
        {
            public string Value { get; private set; } = string.Empty;

            public TestAggregate() : base()
            {
                Register<TestEvent>(OnTestEvent);
            }

            public TestAggregate(IDomainId id) : this()
            {
                Id = id;
            }

            private void OnTestEvent(TestEvent @event)
            {
                Value = @event.Value;
            }

            public void ChangeValue(string newValue)
            {
                RaiseEvent(new TestEvent(newValue));
            }
        }

        [Fact]
        public void AggregateRoot_RaiseEvent_AddsToUncommittedEvents()
        {
            // Arrange
            var aggregate = new TestAggregate(new TestDomainId("123"));

            // Act
            aggregate.ChangeValue("New Value");
            var uncommittedEvents = ((IAggregate)aggregate).GetUncommittedEvents().Cast<TestEvent>().ToList();

            // Assert
            Assert.Single(uncommittedEvents);
            Assert.Equal("New Value", uncommittedEvents.First().Value);
        }

        [Fact]
        public void AggregateRoot_RaiseEvent_UpdatesAggregateState()
        {
            // Arrange
            var aggregate = new TestAggregate(new TestDomainId("123"));

            // Act
            aggregate.ChangeValue("New Value");

            // Assert
            Assert.Equal("New Value", aggregate.Value);
        }

        [Fact]
        public void AggregateRoot_RaiseEvent_IncrementsVersion()
        {
            // Arrange
            var aggregate = new TestAggregate(new TestDomainId("123"));
            int initialVersion = aggregate.Version;

            // Act
            aggregate.ChangeValue("New Value");

            // Assert
            Assert.Equal(initialVersion + 1, aggregate.Version);
        }

        [Fact]
        public void AggregateRoot_ClearUncommittedEvents_RemovesAllEvents()
        {
            // Arrange
            var aggregate = new TestAggregate(new TestDomainId("123"));
            aggregate.ChangeValue("New Value");

            // Act
            ((IAggregate)aggregate).ClearUncommittedEvents();
            var uncommittedEvents = ((IAggregate)aggregate).GetUncommittedEvents();

            // Assert
            Assert.Empty(uncommittedEvents);
        }

        [Fact]
        public void AggregateRoot_Equals_ReturnsTrue()
        {
            // Arrange
            var id = new TestDomainId("123");
            var aggregate1 = new TestAggregate(id);
            var aggregate2 = new TestAggregate(id);

            // Act & Assert
            Assert.True(aggregate1.Equals(aggregate2));
        }

        [Fact]
        public void AggregateRoot_Equals_ReturnsFalse_WhenIdsDiffer()
        {
            // Arrange
            var aggregate1 = new TestAggregate(new TestDomainId("123"));
            var aggregate2 = new TestAggregate(new TestDomainId("456"));

            // Act & Assert
            Assert.False(aggregate1.Equals(aggregate2));
        }

        [Fact]
        public void AggregateRoot_EqualityOperator_ReturnsTrue()
        {
            // Arrange
            var id = new TestDomainId("123");
            var aggregate1 = new TestAggregate(id);
            var aggregate2 = new TestAggregate(id);

            // Act & Assert
            Assert.True(aggregate1 == aggregate2);
        }

        [Fact]
        public void AggregateRoot_EqualityOperator_ReturnsFalse_WhenIdsDiffer()
        {
            // Arrange
            var aggregate1 = new TestAggregate(new TestDomainId("123"));
            var aggregate2 = new TestAggregate(new TestDomainId("456"));

            // Act & Assert
            Assert.False(aggregate1 == aggregate2);
        }

        [Fact]
        public void AggregateRoot_InequalityOperator_ReturnsFalse()
        {
            // Arrange
            var id = new TestDomainId("123");
            var aggregate1 = new TestAggregate(id);
            var aggregate2 = new TestAggregate(id);

            // Act & Assert
            Assert.False(aggregate1 != aggregate2);
        }

        [Fact]
        public void AggregateRoot_InequalityOperator_ReturnsTrue_WhenIdsDiffer()
        {
            // Arrange
            var aggregate1 = new TestAggregate(new TestDomainId("123"));
            var aggregate2 = new TestAggregate(new TestDomainId("456"));

            // Act & Assert
            Assert.True(aggregate1 != aggregate2);
        }
    }
}