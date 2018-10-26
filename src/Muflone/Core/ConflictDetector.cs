using System;
using System.Collections.Generic;
using System.Linq;

namespace Muflone.Core
{
  /// <summary>
  ///   The conflict detector is used to determine if the events to be committed represent
  ///   a true business conflict as compared to events that have already been committed, thus
  ///   allowing reconciliation of optimistic concurrency problems.
  /// </summary>
  /// <remarks>
  ///   The implementation contains some internal lambda "magic" which allows casting between
  ///   TCommitted, TUncommitted, and System.Object and in a completely type-safe way.
  /// </remarks>
  public class ConflictDetector : IDetectConflicts
  {
    //declaring different delegate so that ConflictDelegate(object, object) can be marked obsolete
    private delegate bool ConflictPredicate(object uncommitted, object committed);

    private readonly IDictionary<Type, IDictionary<Type, ConflictPredicate>> actions =
      new Dictionary<Type, IDictionary<Type, ConflictPredicate>>();

    public void Register<TUncommitted, TCommitted>(ConflictDelegate<TUncommitted, TCommitted> handler)
      where TUncommitted : class
      where TCommitted : class
    {
      if (!actions.TryGetValue(typeof(TUncommitted), out var inner))
        actions[typeof(TUncommitted)] = inner = new Dictionary<Type, ConflictPredicate>();

      inner[typeof(TCommitted)] = (uncommitted, committed) => handler(uncommitted as TUncommitted, committed as TCommitted);
    }

    public bool ConflictsWith(IEnumerable<object> uncommittedEvents, IEnumerable<object> committedEvents)
    {
      return (from object uncommitted in uncommittedEvents
              from object committed in committedEvents
              where Conflicts(uncommitted, committed)
              select uncommittedEvents).Any();
    }

    private bool Conflicts(object uncommitted, object committed)
    {
      if (!actions.TryGetValue(uncommitted.GetType(), out var registration))
        return uncommitted.GetType() == committed.GetType(); // no reg, only conflict if the events are the same time

      return !registration.TryGetValue(committed.GetType(), out var callback) || callback(uncommitted, committed);
    }
  }
}