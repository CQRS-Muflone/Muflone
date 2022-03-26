using System;

namespace Muflone.Exceptions;

public class AthenaExceptionArgs : EventArgs
{
    public readonly string Message;
    public readonly string StackTrace;
    public readonly string Source;

    public AthenaExceptionArgs(Exception ex)
    {
        Message = ex.InnerException is not null
            ? ex.InnerException.Message
            : ex.Message;
        StackTrace = ex.StackTrace;
        Source = ex.Source;
    }
}