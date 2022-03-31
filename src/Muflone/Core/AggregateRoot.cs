using System;
using System.Collections;
using System.Collections.Generic;

namespace Muflone.Core;

public abstract class AggregateRoot : IAggregate, IEquatable<IAggregate>
{
    private readonly ICollection<object> _uncommittedEvents = new LinkedList<object>();

    private IRouteEvents _registeredRoutes;

    protected AggregateRoot()
        : this(null)
    { }

    protected AggregateRoot(IRouteEvents handler)
    {
        if (handler == null)
        {
            return;
        }

        RegisteredRoutes = handler;
        RegisteredRoutes.Register(this);
    }

    protected IRouteEvents RegisteredRoutes
    {
        get => _registeredRoutes ??= new ConventionEventRouter(true, this);
        set => _registeredRoutes = value ?? throw new InvalidOperationException("AggregateBase must have an event router to function");
    }

    public Guid Id { get; protected set; }

    public int Version { get; protected set; }

    void IAggregate.ApplyEvent(object @event)
    {
        RegisteredRoutes.Dispatch(@event);
        Version++;
    }

    ICollection IAggregate.GetUncommittedEvents()
    {
        return (ICollection)_uncommittedEvents;
    }

    void IAggregate.ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }

    IMemento IAggregate.GetSnapshot()
    {
        IMemento snapshot = GetSnapshot();
        snapshot.Id = Id;
        snapshot.Version = Version;
        return snapshot;
    }

    public virtual bool Equals(IAggregate other)
    {
        return null != other && other.Id == Id;
    }

    protected void Register<T>(Action<T> route)
    {
        RegisteredRoutes.Register(route);
    }

    protected void RaiseEvent(object @event)
    {
        ((IAggregate)this).ApplyEvent(@event);
        _uncommittedEvents.Add(@event);
    }

    protected virtual IMemento GetSnapshot()
    {
        return null;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as IAggregate);
    }
}