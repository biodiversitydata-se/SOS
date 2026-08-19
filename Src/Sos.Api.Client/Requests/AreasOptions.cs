using SOS.Api.Contracts.Areas.Responses;

namespace SOS.Api.Client.Requests;
/// <summary>Options for searching SOS areas.</summary>
public sealed class AreasOptions
{
    /// <summary>Area types included in the result.</summary>
    public IEnumerable<AreaTypeDto>? AreaTypes { get; set; }

    /// <summary>The text matched against searchable area properties.</summary>
    public string? SearchString { get; set; }

    /// <summary>The number of areas to skip.</summary>
    public int Skip { get; set; }

    /// <summary>The maximum number of areas to return.</summary>
    public int Take { get; set; } = 100;

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .Add("skip", Skip)
            .Add("take", Take)
            .AddValues("areaTypes", AreaTypes)
            .Add("searchString", string.IsNullOrWhiteSpace(SearchString) ? null : SearchString)
            .ToString();
    }
}


