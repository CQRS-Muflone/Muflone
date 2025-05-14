using System;
using System.Collections;
using System.Collections.Generic;

namespace Muflone.Core;

public abstract class AggregateRoot : IAggregate, IEquatable<IAggregate>
{
	private readonly ICollection<object> _uncommittedEvents = new LinkedList<object>();

	private IRouteEvents _registeredRoutes = default!;

	protected AggregateRoot()
		: this(null)
	{
	}

	protected AggregateRoot(IRouteEvents? handler)
	{
		if (handler is null)
			return;

		RegisteredRoutes = handler;
		RegisteredRoutes.Register(this);
	}

	protected IRouteEvents RegisteredRoutes
	{
		get => _registeredRoutes ??= new ConventionEventRouter(true, this);
		set => _registeredRoutes =
			value ?? throw new InvalidOperationException("AggregateRoot must have an event router to function");
	}

	public IDomainId Id { get; protected set; } = default!;
	public int Version { get; protected set; }

	void IAggregate.ApplyEvent(object @event)
	{
		RegisteredRoutes.Dispatch(@event);
		Version++;
	}

	ICollection IAggregate.GetUncommittedEvents() => (ICollection)_uncommittedEvents;

	void IAggregate.ClearUncommittedEvents() => _uncommittedEvents.Clear();

	IMemento? IAggregate.GetSnapshot()
	{
		var snapshot = GetSnapshot();

		if (snapshot is null)
			return null;

		snapshot.Id = Id.Value;
		snapshot.Version = Version;
		return snapshot;
	}

	public virtual bool Equals(IAggregate? other)
	{
		return null != other && GetType() == other.GetType() && other.Id.GetType() == Id.GetType() &&
					 other.Id.Value == Id.Value;
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

	protected virtual IMemento? GetSnapshot() => null;

	public override int GetHashCode() => Id.Value.GetHashCode();

	public override bool Equals(object? obj) => Equals(obj as IAggregate);

	public static bool operator ==(AggregateRoot? entity1, AggregateRoot? entity2)
	{
		if (entity1 is null && entity2 is null)
			return true;

		if (entity1 is null || entity2 is null)
			return false;

		return entity1.GetType() == entity2.GetType() && entity1.Id.GetType() == entity2.Id.GetType() && entity1.Id.Value == entity2.Id.Value;
	}

	public static bool operator !=(AggregateRoot? entity1, AggregateRoot? entity2) => !(entity1 == entity2);
}