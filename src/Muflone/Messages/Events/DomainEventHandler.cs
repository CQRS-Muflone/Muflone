﻿using Microsoft.Extensions.Logging;
using System;

namespace Muflone.Messages.Events;

public abstract class DomainEventHandler<TEvent> : IDomainEventHandler<TEvent> where TEvent : class, IDomainEvent
{
	protected readonly ILoggerFactory LoggerFactory;
	protected readonly ILogger Logger;

	protected DomainEventHandler(ILoggerFactory loggerFactory)
	{
		LoggerFactory = loggerFactory;
		Logger = loggerFactory.CreateLogger(GetType());
	}

	public abstract void Handle(TEvent @event);

	#region Dispose

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	~DomainEventHandler()
	{
		Dispose(false);
	}

	#endregion
}