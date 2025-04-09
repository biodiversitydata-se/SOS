using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System;
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

    public static string GetMemoryUsageSummary()
    {
        try
        {
            var memoryUsage = GC.GetTotalMemory(false);
            GCMemoryInfo memoryInfo = GC.GetGCMemoryInfo();
            var process = Process.GetCurrentProcess();

            return $"Memory Usage (MB): GC Heap Usage: {memoryUsage / 1024 / 1024}, Process RAM: {process.WorkingSet64 / 1024 / 1024}, Peak RAM: {process.PeakWorkingSet64 / 1024 / 1024}";
        }
        catch (Exception e)
        {
            return $"Failed to get memory usage: {e.Message}";
        }
    }
}
