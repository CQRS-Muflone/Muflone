using System;

namespace Muflone.Core;

public class HandlerForDomainEventNotFoundException : Exception
{
    public HandlerForDomainEventNotFoundException()
    {
    }

    public HandlerForDomainEventNotFoundException(string message)
            : base(message)
    {
    }

    public HandlerForDomainEventNotFoundException(string message, Exception innerException)
            : base(message, innerException)
    {
    }
}