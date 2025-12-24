using Muflone.Core;
using Muflone.CustomTypes;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using Muflone.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Muflone.Tests.Integration
{
    public class CommandEventFlowTests
    {
        #region Test Classes

        private class TestId : DomainId
        {
            public TestId(string value) : base(value)
            {
            }
        }

        private class CreateTestAggregate : Command
        {
            public readonly string Name;

            public CreateTestAggregate(TestId aggregateId, string name, Account who) : base(aggregateId, who)
            {
                Name = name;
            }
        }

        private class TestAggregateCreated : DomainEvent
        {
            public readonly string Name;

            public TestAggregateCreated(TestId aggregateId, string name, Account who) : base(aggregateId, who)
            {
                Name = name;
            }
        }

        private class ChangeTestAggregateName : Command
        {
            public readonly string NewName;

            public ChangeTestAggregateName(TestId aggregateId, string newName, Account who) : base(aggregateId, who)
            {
                NewName = newName;
            }
        }

        private class TestAggregateNameChanged : DomainEvent
        {
            public readonly string OldName;
            public readonly string NewName;

            public TestAggregateNameChanged(TestId aggregateId, string oldName, string newName, Account who)
                    : base(aggregateId, who)
            {
                OldName = oldName;
                NewName = newName;
            }
        }

        private class TestAggregate : AggregateRoot
        {
            public string Name { get; private set; } = string.Empty;

            public TestAggregate()
            {
                Register<TestAggregateCreated>(Apply);
                Register<TestAggregateNameChanged>(Apply);
            }

            private void Apply(TestAggregateCreated @event)
            {
                Id = (TestId)@event.AggregateId;
                Name = @event.Name;
            }

            private void Apply(TestAggregateNameChanged @event)
            {
                Name = @event.NewName;
            }

            public static TestAggregate Create(TestId id, string name, Account who)
            {
                var aggregate = new TestAggregate();
                aggregate.RaiseEvent(new TestAggregateCreated(id, name, who));
                return aggregate;
            }

            public void ChangeName(string newName, Account who)
            {
                if (Name == newName)
                    return;

                RaiseEvent(new TestAggregateNameChanged((TestId)Id, Name, newName, who));
            }
        }

        private class CreateTestAggregateHandler : ICommandHandlerAsync<CreateTestAggregate>, IDisposable
        {
            private readonly TestRepository repository;

            public CreateTestAggregateHandler(TestRepository repository)
            {
                this.repository = repository;
            }

            public async Task HandleAsync(CreateTestAggregate command, CancellationToken cancellationToken = default)
            {
                var aggregate = TestAggregate.Create((TestId)command.AggregateId, command.Name, command.Who);
                await repository.SaveAsync(aggregate, Guid.NewGuid(), cancellationToken);
            }

            public void Dispose()
            {
                // Nothing to dispose
            }
        }

        private class ChangeTestAggregateNameHandler : ICommandHandlerAsync<ChangeTestAggregateName>, IDisposable
        {
            private readonly TestRepository repository;

            public ChangeTestAggregateNameHandler(TestRepository repository)
            {
                this.repository = repository;
            }

            public async Task HandleAsync(ChangeTestAggregateName command, CancellationToken cancellationToken = default)
            {
                var aggregate = await repository.GetByIdAsync<TestAggregate>(command.AggregateId, cancellationToken);
                if (aggregate == null)
                    throw new AggregateNotFoundException(command.AggregateId, typeof(TestAggregate));

                aggregate.ChangeName(command.NewName, command.Who);
                await repository.SaveAsync(aggregate, Guid.NewGuid(), cancellationToken);
            }

            public void Dispose()
            {
                // Nothing to dispose
            }
        }

        private class TestDomainEventHandler : IDomainEventHandlerAsync<TestAggregateCreated>, IDomainEventHandlerAsync<TestAggregateNameChanged>, IDisposable
        {
            public List<IDomainEvent> HandledEvents { get; } = new List<IDomainEvent>();

            public Task HandleAsync(TestAggregateCreated @event, CancellationToken cancellationToken = default)
            {
                HandledEvents.Add(@event);
                return Task.CompletedTask;
            }

            public Task HandleAsync(TestAggregateNameChanged @event, CancellationToken cancellationToken = default)
            {
                HandledEvents.Add(@event);
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                // Nothing to dispose
            }
        }

        private class TestRepository : IRepository
        {
            private readonly Dictionary<string, TestAggregate> aggregates = new Dictionary<string, TestAggregate>();
            private readonly TestDomainEventHandler eventHandler;

            public TestRepository(TestDomainEventHandler eventHandler)
            {
                this.eventHandler = eventHandler;
            }

            public void Dispose()
            {
                // Nothing to dispose
            }

            public Task<TAggregate?> GetByIdAsync<TAggregate>(IDomainId id, CancellationToken cancellationToken = default) where TAggregate : class, IAggregate
            {
                if (!aggregates.TryGetValue(id.Value, out var aggregate))
                    return Task.FromResult<TAggregate?>(null);

                return Task.FromResult((TAggregate?)(object)aggregate);
            }

            public Task<TAggregate?> GetByIdAsync<TAggregate>(IDomainId id, long version, CancellationToken cancellationToken = default) where TAggregate : class, IAggregate
            {
                // Simplified for test - ignoring version
                return GetByIdAsync<TAggregate>(id, cancellationToken);
            }

            public async Task SaveAsync(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders, CancellationToken cancellationToken = default)
            {
                var events = aggregate.GetUncommittedEvents();
                foreach (IDomainEvent @event in events)
                {
                    if (@event is TestAggregateCreated createdEvent)
                        await eventHandler.HandleAsync(createdEvent, cancellationToken);
                    else if (@event is TestAggregateNameChanged nameChangedEvent)
                        await eventHandler.HandleAsync(nameChangedEvent, cancellationToken);
                }

                aggregates[aggregate.Id.Value] = (TestAggregate)aggregate;
                aggregate.ClearUncommittedEvents();
            }

            public Task SaveAsync(IAggregate aggregate, Guid commitId, CancellationToken cancellationToken = default)
            {
                return SaveAsync(aggregate, commitId, _ => { }, cancellationToken);
            }
        }

        #endregion

        [Fact]
        public async Task CommandHandler_Create_GeneratesDomainEvent()
        {
            // Arrange
            var eventHandler = new TestDomainEventHandler();
            var repository = new TestRepository(eventHandler);
            var commandHandler = new CreateTestAggregateHandler(repository);

            var aggregateId = new TestId(Guid.NewGuid().ToString());
            var who = new Account(Guid.NewGuid().ToString(), "Test User");
            var command = new CreateTestAggregate(aggregateId, "Test Aggregate", who);

            // Act
            await commandHandler.HandleAsync(command);

            // Assert
            Assert.Single(eventHandler.HandledEvents);
            var domainEvent = eventHandler.HandledEvents[0] as TestAggregateCreated;
            Assert.NotNull(domainEvent);
            Assert.Equal(aggregateId, domainEvent.AggregateId);
            Assert.Equal("Test Aggregate", domainEvent.Name);
            Assert.Equal(who.Id, domainEvent.Headers.Who.Id);

            // Verify aggregate was stored
            var aggregate = await repository.GetByIdAsync<TestAggregate>(aggregateId);
            Assert.NotNull(aggregate);
            Assert.Equal("Test Aggregate", aggregate.Name);
        }

        [Fact]
        public async Task CommandHandler_ChangeName_GeneratesDomainEvent()
        {
            // Arrange
            var eventHandler = new TestDomainEventHandler();
            var repository = new TestRepository(eventHandler);
            var createHandler = new CreateTestAggregateHandler(repository);
            var changeNameHandler = new ChangeTestAggregateNameHandler(repository);

            var aggregateId = new TestId(Guid.NewGuid().ToString());
            var who = new Account(Guid.NewGuid().ToString(), "Test User");

            // First create the aggregate
            await createHandler.HandleAsync(new CreateTestAggregate(aggregateId, "Test Aggregate", who));
            eventHandler.HandledEvents.Clear(); // Clear initial events

            // Prepare command
            var command = new ChangeTestAggregateName(aggregateId, "Updated Name", who);

            // Act
            await changeNameHandler.HandleAsync(command);

            // Assert
            Assert.Single(eventHandler.HandledEvents);
            var domainEvent = eventHandler.HandledEvents[0] as TestAggregateNameChanged;
            Assert.NotNull(domainEvent);
            Assert.Equal(aggregateId, domainEvent.AggregateId);
            Assert.Equal("Test Aggregate", domainEvent.OldName);
            Assert.Equal("Updated Name", domainEvent.NewName);
            Assert.Equal(who.Id, domainEvent.Headers.Who.Id);

            // Verify aggregate was updated
            var aggregate = await repository.GetByIdAsync<TestAggregate>(aggregateId);
            Assert.NotNull(aggregate);
            Assert.Equal("Updated Name", aggregate.Name);
        }

        [Fact]
        public async Task CommandHandler_ChangeName_ThrowsException_WhenAggregateNotFound()
        {
            // Arrange
            var eventHandler = new TestDomainEventHandler();
            var repository = new TestRepository(eventHandler);
            var changeNameHandler = new ChangeTestAggregateNameHandler(repository);

            var aggregateId = new TestId(Guid.NewGuid().ToString());
            var who = new Account(Guid.NewGuid().ToString(), "Test User");
            var command = new ChangeTestAggregateName(aggregateId, "Updated Name", who);

            // Act & Assert
            await Assert.ThrowsAsync<AggregateNotFoundException>(() =>
                    changeNameHandler.HandleAsync(command));

            Assert.Empty(eventHandler.HandledEvents);
        }
    }
}