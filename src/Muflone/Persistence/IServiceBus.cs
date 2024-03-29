﻿using System.Threading;
using System.Threading.Tasks;
using Muflone.Messages.Commands;

namespace Muflone.Persistence;

public interface IServiceBus
{
	Task SendAsync<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand;
}