using SOS.Lib.Enums;
using System.Collections.Generic;

namespace SOS.Lib.Configuration.Shared;
public class SemaphoreLimitsConfiguration
{
    public Dictionary<ApiUserType, int> Observation { get; set; } = new();
    public Dictionary<ApiUserType, int> Aggregation { get; set; } = new();
}