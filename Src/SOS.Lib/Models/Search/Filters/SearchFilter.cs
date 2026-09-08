using SOS.Lib.Enums;

namespace SOS.Lib.Models.Search.Filters;

/// <summary>
/// 
/// </summary>
public class SearchFilter : SearchFilterBase
{
    private const int EstimatedBytesPerTaxonId = 12;
    private const int EstimatedBytesPerCoordinate = 48;
    private const int EstimatedBytesPerGeometry = 128;

    /// <summary>
    /// Constructor
    /// </summary>
    public SearchFilter() : this(0)
    {

    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="protectionFilter"></param>
    public SearchFilter(int userId, ProtectionFilter protectionFilter = ProtectionFilter.Public) : base(userId, protectionFilter)
    {
        Output = new OutputFilter();
    }

    /// <summary>
    ///     This parameter allows you to create a dynamic view of the collection, or more precisely,
    ///     to decide what fields should or should not be returned, using a projection. Put another way,
    ///     your projection is a conditional query where you dictates which fields should be returned by the API.
    ///     Omit this parameter and you will receive the complete collection of fields.
    /// </summary>
    public OutputFilter Output { get; set; }

    /// <summary>
    /// Estimates whether the parts of the filter that can make an Elasticsearch query very large
    /// exceed the specified size. The estimate avoids serializing the filter, when used as cache key,
    /// and stops as soon as the limit is exceeded.
    /// </summary>
    /// <param name="maxEstimatedSizeInBytes">Maximum estimated query size in bytes.</param>
    public bool IsSearchFilterLarge(int maxEstimatedSizeInBytes = 20_000)
    {
        if (maxEstimatedSizeInBytes < 0)
        {
            return true;
        }

        long estimatedSize = 0;
        estimatedSize += (long)(Taxa?.Ids?.Count() ?? 0) * EstimatedBytesPerTaxonId;
        if (estimatedSize > maxEstimatedSizeInBytes)
        {
            return true;
        }

        return IsGeometryFilterLarge(Location?.Geometries, ref estimatedSize, maxEstimatedSizeInBytes) ||
               IsGeometryFilterLarge(Location?.AreaGeographic?.GeometryFilter, ref estimatedSize, maxEstimatedSizeInBytes);
    }

    private static bool IsGeometryFilterLarge(
        GeographicsFilter? filter,
        ref long estimatedSize,
        int maxEstimatedSizeInBytes)
    {
        if (filter?.Geometries == null)
        {
            return false;
        }

        foreach (var geometry in filter.Geometries)
        {
            if (geometry == null)
            {
                continue;
            }

            estimatedSize += (EstimatedBytesPerGeometry + (long)geometry.NumPoints * EstimatedBytesPerCoordinate);
            if (estimatedSize > maxEstimatedSizeInBytes)
            {
                return true;
            }
        }

        return false;
    }
}
