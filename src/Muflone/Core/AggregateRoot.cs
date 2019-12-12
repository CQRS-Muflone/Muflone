using System;
using System.Collections;
using System.Collections.Generic;

namespace Muflone.Core
{
	public abstract class AggregateRoot : IAggregate, IEquatable<IAggregate>
	{
		private readonly ICollection<object> uncommittedEvents = new LinkedList<object>();

		private IRouteEvents registeredRoutes;

		protected AggregateRoot()
			: this(null)
		{

		}

		protected AggregateRoot(IRouteEvents handler)
		{
			if (handler == null)
				return;

			RegisteredRoutes = handler;
			RegisteredRoutes.Register(this);
		}

		protected IRouteEvents RegisteredRoutes
		{
			get => registeredRoutes ?? (registeredRoutes = new ConventionEventRouter(true, this));
			set => registeredRoutes = value ?? throw new InvalidOperationException("AggregateRoot must have an event router to function");
		}

		public IDomainId Id { get; protected set; }
		public int Version { get; protected set; }

		void IAggregate.ApplyEvent(object @event)
		{
			RegisteredRoutes.Dispatch(@event);
			Version++;
		}

		ICollection IAggregate.GetUncommittedEvents()
		{
			return (ICollection)uncommittedEvents;
		}

		void IAggregate.ClearUncommittedEvents()
		{
			uncommittedEvents.Clear();
		}

		IMemento IAggregate.GetSnapshot()
		{
			var snapshot = GetSnapshot();
			snapshot.Id = Id.Value;
			snapshot.Version = Version;
			return snapshot;
		}

		public virtual bool Equals(IAggregate other)
		{
			return null != other && GetType() == other.GetType() && other.Id.GetType() == Id.GetType() && other.Id.Value == Id.Value;
		}

		protected void Register<T>(Action<T> route)
		{
			RegisteredRoutes.Register(route);
		}

		protected void RaiseEvent(object @event)
		{
			((IAggregate)this).ApplyEvent(@event);
			uncommittedEvents.Add(@event);
		}

		protected virtual IMemento GetSnapshot()
		{
			return null;
		}

		public override int GetHashCode()
		{
			return Id.Value.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as IAggregate);
		}

		public static bool operator ==(AggregateRoot entity1, AggregateRoot entity2)
		{
			if ((object)entity1 == null && (object)entity2 == null)
				return true;

			if ((object)entity1 == null || (object)entity2 == null)
				return false;

			return entity1.GetType() == entity2.GetType() && entity1.Id.GetType() == entity2.Id.GetType() && entity1.Id.Value==entity2.Id.Value;
		}

		public static bool operator !=(AggregateRoot entity1, AggregateRoot entity2)
		{
			return !(entity1 == entity2);
		}
	}
}