using System;

namespace Zammad.Client.Core;

public sealed class LogicException : Exception
{
    public LogicException()
        : base() { }

    public LogicException(string message)
        : base(message) { }

    public LogicException(string message, Exception innerException)
        : base(message, innerException) { }

    public static LogicException UnexpectedNullResult => new("Expected Zammad API to return a result, but got null.");
}
