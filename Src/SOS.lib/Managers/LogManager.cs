using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Managers;
public class LogManager
{    
    public static ILogger<LogManager> Logger;

    public static void LogInformation(string? message, params object?[] args)
    {
        if (Logger == null) return;
        Logger.LogInformation(message, args);
    }
}
