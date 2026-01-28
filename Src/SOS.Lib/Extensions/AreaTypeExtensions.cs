using SOS.Lib.Attributes;
using SOS.Lib.Enums;
using System.Reflection;

namespace SOS.Lib.Extensions;

/// <summary>
/// Extension methods for AreaType enum
/// </summary>
public static class AreaTypeExtensions
{
    private static readonly Lazy<AreaType[]> _excludedAreaTypes = new(() =>
    {
        return Enum.GetValues(typeof(AreaType))
            .Cast<AreaType>()
            .Where(areaType =>
            {
                var memberInfo = typeof(AreaType).GetField(areaType.ToString());
                return memberInfo?.GetCustomAttribute<ExcludeFromSearchAttribute>() != null;
            })
            .ToArray();
    });

    /// <summary>
    /// Gets all AreaType values that are marked with ExcludeFromSearchAttribute
    /// </summary>
    public static AreaType[] GetExcludedAreaTypes()
    {
        return _excludedAreaTypes.Value;
    }

    /// <summary>
    /// Gets all AreaType values that are NOT marked with ExcludeFromSearchAttribute
    /// </summary>
    public static AreaType[] GetIncludedAreaTypes()
    {
        var excludedTypes = GetExcludedAreaTypes();
        return Enum.GetValues(typeof(AreaType))
            .Cast<AreaType>()
            .Where(at => !excludedTypes.Contains(at))
            .ToArray();
    }
}
