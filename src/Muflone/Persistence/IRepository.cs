using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Persistence;

public interface IRepository : IDisposable
{
	Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, CancellationToken cancellationToken = default) where TAggregate : class, IAggregate;
	Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, long version, CancellationToken cancellationToken = default) where TAggregate : class, IAggregate;
	Task SaveAsync(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders, CancellationToken cancellationToken = default);
	Task SaveAsync(IAggregate aggregate, Guid commitId, CancellationToken cancellationToken = default);
}