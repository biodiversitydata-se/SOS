using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;

namespace SOS.Shared.Api.Extensions.Controller;

public static class ControllerExtensions
{
    extension(ControllerBase controller)
    {
        public void LogObservationCount(long observationCount)
        {
            if (controller?.HttpContext == null)
            {
                return;
            }

            controller.HttpContext.Items.TryAdd("Observation-count", observationCount);
        }

        public void LogUserType(ApiUserType userType)
        {
            if (controller?.HttpContext == null)
            {
                return;
            }

            controller.HttpContext.Items.TryAdd("ApiUserType", userType);
        }

        public string? GetEndpointName(ControllerContext controllerContext)
        {
            return $"/{controllerContext?.ActionDescriptor?.AttributeRouteInfo?.Template}";
        }

        public ApiUserType GetApiUserType()
        {
            if (controller.HttpContext.Items.TryGetValue("ApiUserType", out var userTypeObj) && userTypeObj is ApiUserType userType)
            {
                return userType;
            }

            return ApiUserType.Unknown;
        }
    }

    extension(ControllerContext controllerContext)
    {
        public string? GetEndpointName()
        {
            return $"/{controllerContext?.ActionDescriptor?.AttributeRouteInfo?.Template}";
        }
    }
}
