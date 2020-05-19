using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Muflone.Core;
using Muflone.Messages.Events;
using Muflone.Persistence;

namespace Muflone.Tests.Persistence
{
	public class InMemoryEventRepository : IRepository
	{
		public IEnumerable<DomainEvent> Events { get; private set; }

		private static TAggregate ConstructAggregate<TAggregate>()
		{
			return (TAggregate)Activator.CreateInstance(typeof(TAggregate), true);
		}

		public void Dispose()
		{
			// nothing to do or apply
		}

		public async Task<TAggregate> GetById<TAggregate>(IDomainId id) where TAggregate : class, IAggregate
		{
			return await GetById<TAggregate>(id, 0);
		}

		public Task<TAggregate> GetById<TAggregate>(IDomainId id, int version) where TAggregate : class, IAggregate
		{
			var aggregate = ConstructAggregate<TAggregate>();
			Events.ForEach(aggregate.ApplyEvent);
			return Task.FromResult(aggregate);
		}

		public Task Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
		{
			Events = aggregate.GetUncommittedEvents().Cast<DomainEvent>();

			var originalVersion = aggregate.Version - Events.Count();
			var expectedVersion = originalVersion + 1;

			if (aggregate.Version == expectedVersion)
				return Task.CompletedTask;

			throw new Exception($"Aggregate has a wrong Version. Expected: {expectedVersion} - Current: {aggregate.Version}");
		}
	}

	public static class Helpers
	{
		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			if (items == null)
				return;
			foreach (var obj in items)
				action(obj);
		}
	}

	public static class ExpectedVersion
	{
		public const long Any = -2;
		public const long NoStream = 0;
		public const long EmptyStream = -1;
		public const long StreamExists = -4;
	}
}