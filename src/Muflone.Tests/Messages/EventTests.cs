using Muflone.Core;
using Muflone.CustomTypes;
using Muflone.Messages.Events;
using System;
using Xunit;

namespace Muflone.Tests.Messages
{
    public class EventTests
    {
        private class TestDomainId : DomainId
        {
            public TestDomainId(string value) : base(value)
            {
            }
        }

        private class TestEvent : Event
        {
            public TestEvent(IDomainId aggregateId) : base(aggregateId)
            {
            }

            public TestEvent(IDomainId aggregateId, Account who) : base(aggregateId, who)
            {
            }

            public TestEvent(IDomainId aggregateId, Guid correlationId) : base(aggregateId, correlationId)
            {
            }

            public TestEvent(IDomainId aggregateId, Guid correlationId, Account who) : base(aggregateId, correlationId, who)
            {
            }

            public TestEvent(IDomainId aggregateId, Guid correlationId, Account who, When when) : base(aggregateId, correlationId, who, when)
            {
            }
        }

        [Fact]
        public void Event_Constructor_SetsAggregateId()
        {
            // Arrange
            var aggregateId = new TestDomainId("123");

            // Act
            var @event = new TestEvent(aggregateId);

            // Assert
            Assert.Equal(aggregateId, @event.AggregateId);
        }

        [Fact]
        public void Event_Constructor_SetsDefaultWho_WhenNotProvided()
        {
            // Arrange
            var aggregateId = new TestDomainId("123");

            // Act
            var @event = new TestEvent(aggregateId);

            // Assert
            Assert.Equal(Guid.Empty.ToString(), @event.Headers.Who.Id);
            Assert.Equal("Anonymous", @event.Headers.Who.Name);
        }

        [Fact]
        public void Event_Constructor_SetsWho_WhenProvided()
        {
            // Arrange
            var aggregateId = new TestDomainId("123");
            var account = new Account("user-123", "Test User");

            // Act
            var @event = new TestEvent(aggregateId, account);

            // Assert
            Assert.Equal("user-123", @event.Headers.Who.Id);
            Assert.Equal("Test User", @event.Headers.Who.Name);
        }

        [Fact]
        public void Event_Constructor_SetsCorrelationId()
        {
            // Arrange
            var aggregateId = new TestDomainId("123");
            var correlationId = Guid.NewGuid();

            // Act
            var @event = new TestEvent(aggregateId, correlationId);

            // Assert
            Assert.Equal(correlationId, @event.Headers.CorrelationId);
            Assert.Equal(correlationId, @event.UserProperties["CorrelationId"]);
        }

        [Fact]
        public void Event_Constructor_SetsMessageId()
        {
            // Arrange
            var aggregateId = new TestDomainId("123");

            // Act
            var @event = new TestEvent(aggregateId);

            // Assert
            Assert.NotEqual(Guid.Empty, @event.MessageId);
        }

        [Fact]
        public void Event_Constructor_SetsAggregateType()
        {
            // Arrange
            var aggregateId = new TestDomainId("123");

            // Act
            var @event = new TestEvent(aggregateId);

            // Assert
            Assert.Equal("TestEvent", @event.Headers.AggregateType);
        }

        [Fact]
        public void Event_Constructor_SetsTimestamp()
        {
            // Arrange
            var aggregateId = new TestDomainId("123");
            var beforeTest = DateTime.UtcNow.AddSeconds(-1);

            // Act
            var @event = new TestEvent(aggregateId);
            var afterTest = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.True(@event.Headers.When.Value > beforeTest);
            Assert.True(@event.Headers.When.Value < afterTest);
        }

        [Fact]
        public void Event_Constructor_SetsCustomTimestamp_WhenProvided()
        {
            // Arrange
            var aggregateId = new TestDomainId("123");
            var correlationId = Guid.NewGuid();
            var account = new Account("user-123", "Test User");
            var when = new When(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            // Act
            var @event = new TestEvent(aggregateId, correlationId, account, when);

            // Assert
            Assert.Equal(when.Value, @event.Headers.When.Value);
        }
    }
}