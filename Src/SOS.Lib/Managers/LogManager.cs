using Microsoft.Extensions.Logging;
using System;

namespace SOS.Lib.Managers;
public class LogManager
{    
    public static ILogger<LogManager> Logger;

    public static void LogDebug(string? message, params object?[] args)
    {
        if (Logger == null) return;
        Logger.LogDebug(message, args);
    }

    public static void LogInformation(string? message, params object?[] args)
    {
        if (Logger == null) return;
        Logger.LogInformation(message, args);
    }

    public static void LogError(string? message, params object?[] args)
    {
        if (Logger == null) return;
        Logger.LogError(message, args);
    }

    public static void LogError(Exception? exception, string? message, params object?[] args)
    {
        if (Logger == null) return;
        Logger.LogError(exception, message, args);
    }
}
