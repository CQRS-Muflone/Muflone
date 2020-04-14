using System;
using System.Collections.Generic;

namespace Muflone.Persistence
{
  public interface ISagaRepository
  {
    TSaga GetById<TSaga>(string sagaId) where TSaga : class, ISaga;

    void Save(ISaga saga, Guid commitId, Action<IDictionary<string, object>> updateHeaders);
  }
}