using System;
using Muflone.Core;

namespace Muflone.Persistence
{
  public interface IConstructAggregates
  {
    IAggregate Build(Type type, IDomainId id, IMemento snapshot);
  }
}