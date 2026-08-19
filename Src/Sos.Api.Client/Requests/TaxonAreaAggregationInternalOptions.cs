using SOS.Api.Contracts.Analysis.Requests;

namespace SOS.Api.Client.Requests;

/// <summary>Options for aggregating observations by taxon and area.</summary>
public sealed class TaxonAreaAggregationInternalOptions : SosRoleRequestOptions
{
    /// <summary>The area type used for aggregation.</summary>
    public AreaTypeAggregateDto? AreaType { get; set; }

    /// <summary>Controls validation of search-filter values.</summary>
    public bool ValidateSearchFilter { get; set; }

    /// <summary>Controls the use of the sensitive-observation index.</summary>
    public bool SensitiveObservations { get; set; }

    /// <summary>Controls summation of observation counts through the taxon tree.</summary>
    public bool SumTaxaTree { get; set; }

    /// <summary>Controls removal of taxon-tree nodes without observations when sums are calculated.</summary>
    public bool PruneTaxaTree { get; set; }

    /// <summary>Creates the endpoint query string.</summary>
    public string ToQueryString()
    {
        return new SosQueryBuilder()
            .AddEnum("areaType", AreaType)
            .Add("validateSearchFilter", ValidateSearchFilter)
            .Add("sensitiveObservations", SensitiveObservations)
            .Add("sumTaxaTree", SumTaxaTree)
            .Add("pruneTaxaTree", PruneTaxaTree)
            .ToString();
    }
}
