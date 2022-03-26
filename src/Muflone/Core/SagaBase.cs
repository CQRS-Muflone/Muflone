using System;
using System.Collections;
using System.Collections.Generic;

namespace Muflone.Core;

public class SagaBase<TMessage> : ISaga, IEquatable<ISaga> where TMessage : class
{
    private readonly IDictionary<Type, Action<TMessage>> _handlers = new Dictionary<Type, Action<TMessage>>();

    private readonly ICollection<TMessage> _uncommitted = new LinkedList<TMessage>();

    private readonly ICollection<TMessage> _undispatched = new LinkedList<TMessage>();

    public virtual bool Equals(ISaga other)
    {
        return null != other && other.Id == Id;
    }

    public string Id { get; protected set; }

    public int Version { get; private set; }

    public void Transition(object message)
    {
        _handlers[message.GetType()](message as TMessage);
        _uncommitted.Add(message as TMessage);
        Version++;
    }

    ICollection ISaga.GetUncommittedEvents()
    {
        return _uncommitted as ICollection;
    }

    void ISaga.ClearUncommittedEvents()
    {
        _uncommitted.Clear();
    }

    ICollection ISaga.GetUndispatchedMessages()
    {
        return _undispatched as ICollection;
    }

    void ISaga.ClearUndispatchedMessages()
    {
        _undispatched.Clear();
    }

    protected void Register<TRegisteredMessage>(Action<TRegisteredMessage> handler)
        where TRegisteredMessage : class, TMessage
    {
        _handlers[typeof(TRegisteredMessage)] = message => handler(message as TRegisteredMessage);
    }

    protected void Dispatch(TMessage message)
    {
        _undispatched.Add(message);
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