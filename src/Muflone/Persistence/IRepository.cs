using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Muflone.Core;

namespace Muflone.Persistence
{
  public interface IRepository : IDisposable
  {
    Task<TAggregate> GetById<TAggregate>(IDomainId id) where TAggregate : class, IAggregate;
    Task<TAggregate> GetById<TAggregate>(IDomainId id, int version) where TAggregate : class, IAggregate;
    Task Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders);
  }
}