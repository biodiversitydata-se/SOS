using SOS.Lib.Models.Search.Filters;

namespace SOS.Observations.Api.Managers;

/// <summary>
/// A filter prepared for one search request, including its authorization context.
/// Do not cache this object or modify its filter between queries.
/// </summary>
public sealed class PreparedObservationFilter
{
    internal SearchFilter Filter { get; }

    internal PreparedObservationFilter(SearchFilter filter) => Filter = filter;
}
