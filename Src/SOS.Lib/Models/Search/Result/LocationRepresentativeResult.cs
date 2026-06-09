using System;
using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Result;

/// <summary>
/// Result returned by a collapsed representative location query.
/// </summary>
/// <typeparam name="T">Representative record type.</typeparam>
public class LocationRepresentativeResult<T>
{
    /// <summary>
    /// One representative record per location.
    /// </summary>
    public IEnumerable<T> Records { get; set; } = Array.Empty<T>();

    /// <summary>
    /// Number of matching observations per location id.
    /// </summary>
    public Dictionary<string, long> ObservationCountByLocationId { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns a sequence containing up to the specified number of representative locations.
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Total number of representative records returned.
    /// </summary>
    public long TotalCount { get; set; }
}
