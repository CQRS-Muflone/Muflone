using System;

namespace Muflone.Core
{
  public class AggregateDeletedException : Exception
  {
    public readonly Guid Id;
    public readonly Type Type;

    public AggregateDeletedException(Guid id, Type type)
        : base($"Aggregate '{id}' (type {type.Name}) was deleted.")
    {
      Id = id;
      Type = type;
    }
  }

  public class AggregateNotFoundException : Exception
  {
    public readonly Guid Id;
    public readonly Type Type;

    public AggregateNotFoundException(Guid id, Type type)
        : base($"Aggregate '{id}' (type {type.Name}) was not found.")
    {
      Id = id;
      Type = type;
    }
  }

  public class AggregateVersionException : Exception
  {
    public readonly Guid Id;
    public readonly Type Type;
    public readonly int AggregateVersion;
    public readonly int RequestedVersion;

    public AggregateVersionException(Guid id, Type type, int aggregateVersion, int requestedVersion)
        : base($"Requested version {requestedVersion} of aggregate '{id}' (type {type.Name}) - aggregate version is {aggregateVersion}")
    {
      Id = id;
      Type = type;
      AggregateVersion = aggregateVersion;
      RequestedVersion = requestedVersion;
    }
  }
}
