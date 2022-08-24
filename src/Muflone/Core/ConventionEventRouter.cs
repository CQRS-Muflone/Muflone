using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Muflone.Core;

public sealed class ConventionEventRouter : IRouteEvents
{
	private readonly IDictionary<Type, Action<object>> _handlers = new Dictionary<Type, Action<object>>();

	private readonly bool _throwOnApplyNotFound;

	private IAggregate _registered;

	public ConventionEventRouter()
		: this(true)
	{
	}

	public ConventionEventRouter(bool throwOnApplyNotFound)
	{
		this._throwOnApplyNotFound = throwOnApplyNotFound;
	}

	public ConventionEventRouter(bool throwOnApplyNotFound, IAggregate aggregate)
		: this(throwOnApplyNotFound)
	{
		Register(aggregate);
	}

	public void Register<T>(Action<T> handler)
	{
		if (handler == null)
			throw new ArgumentNullException(nameof(handler));

		Register(typeof(T), @event => handler((T)@event));
	}

	public void Register(IAggregate aggregate)
	{
		_registered = aggregate ?? throw new ArgumentNullException("aggregate");

		// Get instance methods named Apply with one parameter returning void
		var applyMethods = aggregate.GetType()
			.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(m => m.Name == "Apply" && m.GetParameters().Length == 1 && m.ReturnParameter.ParameterType == typeof(void))
			.Select(m => new { Method = m, MessageType = m.GetParameters().Single().ParameterType });

		foreach (var apply in applyMethods)
		{
			var applyMethod = apply.Method;
			_handlers.Add(apply.MessageType, m => applyMethod.Invoke(aggregate, new[] { m }));
		}
	}

	public void Dispatch(object eventMessage)
	{
		if (eventMessage == null)
			throw new ArgumentNullException(nameof(eventMessage));

		if (_handlers.TryGetValue(eventMessage.GetType(), out var handler))
			handler(eventMessage);
		else if (_throwOnApplyNotFound)
			_registered.ThrowHandlerNotFound(eventMessage);
	}

	private void Register(Type messageType, Action<object> handler)
	{
		_handlers[messageType] = handler;
	}
}