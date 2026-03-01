using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Muflone.Core;
using Muflone.Messages;
using OpenTelemetry.Trace;

namespace Muflone.Persistence;

/// <summary>
/// Decorator that wraps an IRepository implementation with OpenTelemetry client spans.
/// </summary>
public sealed class InstrumentedRepository : IRepository
{
	private readonly IRepository _inner;
	private static readonly ActivitySource ActivitySource = new(
		OpenTelemetryConstants.ActivitySourceNames.Repository,
		OpenTelemetryConstants.Version);

	public InstrumentedRepository(IRepository inner)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
	}

	public async Task<TAggregate?> GetByIdAsync<TAggregate>(IDomainId id, CancellationToken cancellationToken = default)
		where TAggregate : class, IAggregate
	{
		var activityName = $"{typeof(TAggregate).Name} GetById";
		Activity? activity = ActivitySource.StartActivity(activityName, ActivityKind.Client);

		try
		{
			activity?.SetTag(OpenTelemetryConstants.Tags.DbSystem, OpenTelemetryConstants.TagValues.DbSystemEventStore);
			activity?.SetTag(OpenTelemetryConstants.Tags.DbOperation, "GetById");
			activity?.SetTag(OpenTelemetryConstants.Tags.DbCollectionName, typeof(TAggregate).Name);

			var result = await _inner.GetByIdAsync<TAggregate>(id, cancellationToken);

			activity?.SetStatus(ActivityStatusCode.Ok);
			return result;
		}
		catch (Exception ex)
		{
			activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
			activity?.AddException(ex);
			throw;
		}
		finally
		{
			activity?.Dispose();
		}
	}

	public async Task<TAggregate?> GetByIdAsync<TAggregate>(IDomainId id, long version, CancellationToken cancellationToken = default)
		where TAggregate : class, IAggregate
	{
		var activityName = $"{typeof(TAggregate).Name} GetById";
		Activity? activity = ActivitySource.StartActivity(activityName, ActivityKind.Client);

		try
		{
			activity?.SetTag(OpenTelemetryConstants.Tags.DbSystem, OpenTelemetryConstants.TagValues.DbSystemEventStore);
			activity?.SetTag(OpenTelemetryConstants.Tags.DbOperation, "GetById");
			activity?.SetTag(OpenTelemetryConstants.Tags.DbCollectionName, typeof(TAggregate).Name);

			var result = await _inner.GetByIdAsync<TAggregate>(id, version, cancellationToken);

			activity?.SetStatus(ActivityStatusCode.Ok);
			return result;
		}
		catch (Exception ex)
		{
			activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
			activity?.AddException(ex);
			throw;
		}
		finally
		{
			activity?.Dispose();
		}
	}

	public async Task SaveAsync(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders, CancellationToken cancellationToken = default)
	{
		var aggregateType = aggregate.GetType().Name;
		var activityName = $"{aggregateType} Save";
		Activity? activity = ActivitySource.StartActivity(activityName, ActivityKind.Client);

		try
		{
			activity?.SetTag(OpenTelemetryConstants.Tags.DbSystem, OpenTelemetryConstants.TagValues.DbSystemEventStore);
			activity?.SetTag(OpenTelemetryConstants.Tags.DbOperation, "Save");
			activity?.SetTag(OpenTelemetryConstants.Tags.DbCollectionName, aggregateType);

			await _inner.SaveAsync(aggregate, commitId, updateHeaders, cancellationToken);

			activity?.SetStatus(ActivityStatusCode.Ok);
		}
		catch (Exception ex)
		{
			activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
			activity?.AddException(ex);
			throw;
		}
		finally
		{
			activity?.Dispose();
		}
	}

	public async Task SaveAsync(IAggregate aggregate, Guid commitId, CancellationToken cancellationToken = default)
	{
		var aggregateType = aggregate.GetType().Name;
		var activityName = $"{aggregateType} Save";
		Activity? activity = ActivitySource.StartActivity(activityName, ActivityKind.Client);

		try
		{
			activity?.SetTag(OpenTelemetryConstants.Tags.DbSystem, OpenTelemetryConstants.TagValues.DbSystemEventStore);
			activity?.SetTag(OpenTelemetryConstants.Tags.DbOperation, "Save");
			activity?.SetTag(OpenTelemetryConstants.Tags.DbCollectionName, aggregateType);

			await _inner.SaveAsync(aggregate, commitId, cancellationToken);

			activity?.SetStatus(ActivityStatusCode.Ok);
		}
		catch (Exception ex)
		{
			activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
			activity?.AddException(ex);
			throw;
		}
		finally
		{
			activity?.Dispose();
		}
	}

	public void Dispose()
	{
		_inner.Dispose();
	}
}
