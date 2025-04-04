using System;

namespace SOS.Lib.Exceptions;

public class SemaphoreTimeoutException : Exception
{
    public SemaphoreTimeoutException() : base("Too many concurrent requests. Semaphore wait timeout.") { }
    public SemaphoreTimeoutException(string message) : base(message) { }
    public SemaphoreTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}

