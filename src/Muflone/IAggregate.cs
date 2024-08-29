using Muflone.Core;
using System.Collections;

namespace Muflone;

public interface IAggregate
{
  IDomainId Id { get; }
  int Version { get; }

  void ApplyEvent(object @event);
  ICollection GetUncommittedEvents();
  void ClearUncommittedEvents();

  IMemento? GetSnapshot();
}