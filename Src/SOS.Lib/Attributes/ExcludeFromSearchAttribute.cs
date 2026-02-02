namespace SOS.Lib.Attributes;

/// <summary>
/// Marks an AreaType as excluded from default search results
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ExcludeFromSearchAttribute : Attribute
{
}
