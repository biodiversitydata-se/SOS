using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using SOS.Lib.Helpers;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;

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
        catch (TaskCanceledException e)
        {
             _logger.LogWarning(e, "Request was canceled by client.");
        }
    }
}