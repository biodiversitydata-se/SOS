using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace SOS.Lib.Helpers;
public static class LogHelper
{
    public static void AddHttpContextItems(HttpContext? httpContext, ControllerContext controllerContext, MethodBase? methodBase)
    {
        if (httpContext == null) return;        
        httpContext.Items["Endpoint"] = controllerContext?.ActionDescriptor?.AttributeRouteInfo?.Template;
        httpContext.Items["Handler"] = $"{methodBase?.DeclaringType?.FullName}.{methodBase?.Name}";
    }
}
