using System.Collections;
using Muflone.Core;

namespace Muflone
{
  public interface IAggregate
  {
    IDomainId Id { get; }
    int Version { get; }
    void ApplyEvent(object @event);
    ICollection GetUncommittedEvents();
    void ClearUncommittedEvents();
    IMemento GetSnapshot();
  }
}