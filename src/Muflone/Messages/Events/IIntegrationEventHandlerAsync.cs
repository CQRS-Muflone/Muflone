﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages.Events;

public interface IIntegrationEventHandlerAsync : IDisposable
{
}

public interface IIntegrationEventHandlerAsync<in T> : IIntegrationEventHandlerAsync where T : IIntegrationEvent
{
	Task HandleAsync(T @event, CancellationToken cancellationToken = default);
}