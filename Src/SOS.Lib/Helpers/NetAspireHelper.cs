using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Helpers;
public static class NetAspireHelper
{
    public static string? GetServiceEndpoint(string serviceName, string endpointName, int index = 0) =>
          Environment.GetEnvironmentVariable($"services__{serviceName}__{endpointName}__{index}");
}
