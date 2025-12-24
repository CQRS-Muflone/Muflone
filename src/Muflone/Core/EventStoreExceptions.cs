using System;

namespace Muflone.Core;

public class AggregateDeletedException : Exception
{
    public readonly IDomainId Id;
    public readonly Type Type;

    public AggregateDeletedException(IDomainId id, Type type)
            : base($"Aggregate '{id.Value}' (type {type.Name}) was deleted.")
    {
        Id = id;
        Type = type;
    }
}

public class AggregateNotFoundException : Exception
{
    public readonly IDomainId Id;
    public readonly Type Type;

    public AggregateNotFoundException(IDomainId id, Type type)
            : base($"Aggregate '{id.Value}' (type {type.Name}) was not found.")
    {
        Id = id;
        Type = type;
    }
}

public class AggregateVersionException : Exception
{
    public readonly IDomainId Id;
    public readonly Type Type;
    public readonly long AggregateVersion;
    public readonly long RequestedVersion;

    public AggregateVersionException(IDomainId id, Type type, long aggregateVersion, long requestedVersion)
            : base(
                    $"Requested version {requestedVersion} of aggregate '{id.Value}' (type {type.Name}) - aggregate version is {aggregateVersion}")
    {
        Id = id;
        Type = type;
        AggregateVersion = aggregateVersion;
        RequestedVersion = requestedVersion;
    }
}