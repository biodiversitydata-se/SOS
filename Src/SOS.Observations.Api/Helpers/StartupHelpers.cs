using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Observations.Api.Helpers;

public static class StartupHelpers
{
    public static IReadOnlyList<ApiVersion> GetApiVersions(ApiDescription apiDescription)
    {
        var apiVersionMetadata = apiDescription.ActionDescriptor.GetApiVersionMetadata();
        var actionApiVersionModel = apiVersionMetadata.Map(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

        var apiVersions = actionApiVersionModel.DeclaredApiVersions.Any()
            ? actionApiVersionModel.DeclaredApiVersions
            : actionApiVersionModel.ImplementedApiVersions;
        return apiVersions;
    }
    public static bool GetEnvironmentBool(string environmentVariable, bool defaultValue = false)
    {
        string value = Environment.GetEnvironmentVariable(environmentVariable);
        if (value != null)
        {
            if (bool.TryParse(value, out var boolValue))
            {
                return boolValue;
            }
            return defaultValue;
        }
        else
        {
            return defaultValue;
        }
    }
}