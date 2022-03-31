using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Muflone.Persistence;

public interface IRepository : IDisposable
{
	Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : class, IAggregate;
	Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, int version) where TAggregate : class, IAggregate;
	Task SaveAsync(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders);
	Task SaveAsync(IAggregate aggregate, Guid commitId);
}