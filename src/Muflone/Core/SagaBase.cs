using System;
using System.Collections;
using System.Collections.Generic;

namespace Muflone.Core
{
  public class SagaBase<TMessage> : ISaga, IEquatable<ISaga>
  where TMessage : class
  {
    private readonly IDictionary<Type, Action<TMessage>> handlers = new Dictionary<Type, Action<TMessage>>();

    private readonly ICollection<TMessage> uncommitted = new LinkedList<TMessage>();

    private readonly ICollection<TMessage> undispatched = new LinkedList<TMessage>();

    public virtual bool Equals(ISaga other)
    {
      return null != other && other.Id == Id;
    }

    public string Id { get; protected set; }

    public int Version { get; private set; }

    public void Transition(object message)
    {
      handlers[message.GetType()](message as TMessage);
      uncommitted.Add(message as TMessage);
      Version++;
    }

    ICollection ISaga.GetUncommittedEvents()
    {
      return uncommitted as ICollection;
    }

    void ISaga.ClearUncommittedEvents()
    {
      uncommitted.Clear();
    }

    ICollection ISaga.GetUndispatchedMessages()
    {
      return undispatched as ICollection;
    }

    void ISaga.ClearUndispatchedMessages()
    {
      undispatched.Clear();
    }

    protected void Register<TRegisteredMessage>(Action<TRegisteredMessage> handler)
      where TRegisteredMessage : class, TMessage
    {
      handlers[typeof(TRegisteredMessage)] = message => handler(message as TRegisteredMessage);
    }

    protected void Dispatch(TMessage message)
    {
      undispatched.Add(message);
    }

    public override int GetHashCode()
    {
      return Id.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as ISaga);
    }
  }
}