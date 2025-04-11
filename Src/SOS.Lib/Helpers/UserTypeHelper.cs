using Microsoft.AspNetCore.Http;
using SOS.Lib.Enums;
using System.Collections.Generic;

namespace SOS.Lib.Helpers;
public static class UserTypeHelper
{
    private static HashSet<string> _priorityApiAccounts = [
            "612f1c88f24b16120c3c4418",
            "62864211f24b1603401ea955",
            "5ee390aadbbebc0a34bc684c"
        ];

    public static ApiUserType GetUserType(HttpRequest request)
    {
        if (request.Headers.TryGetValue("request-user-id", out var apiAccountId))
        {
            if (_priorityApiAccounts.Contains(apiAccountId))
            {
                return ApiUserType.Priority;
            }

            return ApiUserType.Standard;
        }

        string requestingSystem = null;
        if (request.Headers.TryGetValue("X-Requesting-System", out var requestingSystemVal))
        {
            requestingSystem = requestingSystemVal.ToString();
        }
        else if (request.Headers.TryGetValue("Requesting-System", out var requestingSystemV))
        {
            requestingSystem = requestingSystemV.ToString();
        }

        if (requestingSystem != null)
        {
            if (requestingSystem.StartsWith("Artfakta", System.StringComparison.InvariantCultureIgnoreCase))
            {
                return ApiUserType.Artfakta;
            }
            else if (requestingSystem.StartsWith("Artportalen", System.StringComparison.InvariantCultureIgnoreCase))
            {
                return ApiUserType.Artportalen;
            }
            else if (requestingSystem.StartsWith("printobs", System.StringComparison.InvariantCultureIgnoreCase))
            {
                return ApiUserType.Fynddata;
            }
        }

        return ApiUserType.ArtdataInternal;
    }
}
