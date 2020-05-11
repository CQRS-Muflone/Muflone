using System;
using System.Threading.Tasks;
using Muflone.Core;
using Muflone.Messages.Events;
using Xunit;

namespace Muflone.Persistence.Test
{
    public class PersistenceTest
    {
        private readonly IRepository _repository;

        public PersistenceTest()
        {
            this._repository = new InMemoryEventRepository();
        }

        [Fact]
        public async Task Can_Save_Aggregate_Within_Repository()
        {
            var fakeId = Guid.NewGuid();
            var fakeDomainId = new FakeDomainId(fakeId);

            var fakeAggregate = FakeAggregateRoot.CreateAggregateRoot(fakeDomainId);
            await this._repository.Save(fakeAggregate, Guid.NewGuid());

            var aggregateFromRepository = await this._repository.GetById<FakeAggregateRoot>(fakeDomainId);
            aggregateFromRepository.DoSomething();
            await this._repository.Save(fakeAggregate, Guid.NewGuid());

            aggregateFromRepository = await this._repository.GetById<FakeAggregateRoot>(fakeDomainId);
            Assert.Equal(fakeAggregate, aggregateFromRepository);
        }

        [Fact]
        public async Task Cannot_Commit_Entity_With_Same_Revision()
        {
            var fakeId = Guid.NewGuid();
            var fakeDomainId = new FakeDomainId(fakeId);

            var fakeAggregate = FakeAggregateRoot.CreateAggregateRoot(fakeDomainId);
            await this._repository.Save(fakeAggregate, Guid.NewGuid());

            var aggregateFromRepository = await this._repository.GetById<FakeAggregateRoot>(fakeDomainId);

            var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
                this._repository.Save(aggregateFromRepository, Guid.NewGuid()));

            Assert.Equal(
                "Aggregate has a wrong Version. Expected: 2 - Current: 1",
                exception.Message);
        }

        #region Mock Objects
        public class FakeDomainId : DomainId
        {
            public FakeDomainId(Guid id) : base(id)
            {
            }
        }

        private class FakeAggregateRoot : AggregateRoot
        {
            private string _message;

            protected FakeAggregateRoot()
            {}

            internal static FakeAggregateRoot CreateAggregateRoot (IDomainId domainId)
            {
                return new FakeAggregateRoot(domainId);
            }

            private FakeAggregateRoot(IDomainId id)
            {
                this.RaiseEvent(new FakeAggregateCreated(id));
            }

            private void Apply(FakeAggregateCreated @event)
            {
                this.Id = @event.AggregateId;
            }

            public void DoSomething()
            {
                this.RaiseEvent(new SomethingDone(this.Id, "Raise FakeEvent"));
            }

            private void Apply(SomethingDone @event)
            {
                this.Id = @event.AggregateId;
                this._message = @event.Message;
            }
        }

        public class FakeAggregateCreated : DomainEvent
        {
            public FakeAggregateCreated(IDomainId aggregateId, string who = "anonymous") : base(aggregateId, who)
            {
            }
        }
        public class SomethingDone : DomainEvent
        {
            public readonly string Message;

            public SomethingDone(IDomainId aggregateId, string message, string who = "anonymous") : base(aggregateId, who)
            {
                this.Message = message;
            }
        }
        #endregion
    }
}
