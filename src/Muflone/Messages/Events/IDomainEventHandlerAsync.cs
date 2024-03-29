﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Events;

public interface IDomainEventHandlerAsync : IDisposable
{
}

public interface IDomainEventHandlerAsync<in TEvent> : IDomainEventHandlerAsync where TEvent : class, IDomainEvent
{
	Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}