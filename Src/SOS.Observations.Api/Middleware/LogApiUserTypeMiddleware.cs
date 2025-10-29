using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SOS.Lib.Helpers;
using System;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Middleware;

public class LogApiUserTypeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogApiUserTypeMiddleware> _logger;

    public LogApiUserTypeMiddleware(RequestDelegate next, ILogger<LogApiUserTypeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {       
        try
        {
            var userType = UserTypeHelper.GetUserType(context.Request);
            context.Items["ApiUserType"] = userType;
            await _next(context);
        }
        catch (TaskCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {            
            _logger.LogInformation("Request was cancelled by client. Path: {Path}", context.Request?.Path);            
        }
        catch (Exception ex)
        {            
            _logger.LogError(ex, "Unexpected error in request pipeline. Path: {Path}", context.Request?.Path);
            throw;
        }
    }
}
