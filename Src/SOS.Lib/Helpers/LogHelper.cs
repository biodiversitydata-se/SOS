using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace SOS.Lib.Helpers;
public static class LogHelper
{
    public static void AddHttpContextItems(HttpContext? httpContext, ControllerContext controllerContext)
    {
        if (httpContext == null) return;
        httpContext.Items["Endpoint"] = $"/{controllerContext?.ActionDescriptor?.AttributeRouteInfo?.Template}";
        httpContext.Items["QueryString"] = $"/{httpContext?.Request?.QueryString}";
        httpContext.Items["Handler"] = $"{controllerContext?.ActionDescriptor?.ControllerTypeInfo?.FullName}.{controllerContext?.ActionDescriptor?.MethodInfo?.Name}";
    }

    public static void AddHttpContextItems(HttpContext? httpContext)
    {
        if (httpContext == null) return;
        httpContext.Items["Endpoint"] = $"/{httpContext?.Request.Path}";
        httpContext.Items["QueryString"] = $"/{httpContext?.Request?.QueryString}";
    }
}
