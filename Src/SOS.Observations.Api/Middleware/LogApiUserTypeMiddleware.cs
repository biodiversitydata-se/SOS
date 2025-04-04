using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using SOS.Lib.Helpers;

namespace SOS.Observations.Api.Middleware;

public class LogApiUserTypeMiddleware
{
    private readonly RequestDelegate _next;

    public LogApiUserTypeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var userType = UserTypeHelper.GetUserType(context.Request);
        context.Items["ApiUserType"] = userType;

        await _next(context);
    }
}