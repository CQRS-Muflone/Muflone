using System;

public class HandlerConsumerRegistrationException(Type consumerType, Type type, Exception ex) : Exception
{
    public Type ConsumerType { get; set; } = consumerType;
    public Type Type { get; set; } = type;
    public Exception Exception { get; set; } = ex;
}
