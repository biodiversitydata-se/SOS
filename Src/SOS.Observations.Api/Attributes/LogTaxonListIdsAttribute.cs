using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Observations.Api.Configuration;
using System.Globalization;

namespace SOS.Observations.Api.Attributes;

/// <summary>
/// Captures the requested taxon lists before the action changes or resolves the filter.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class LogTaxonListIdsAttribute : ActionFilterAttribute
{
    public LogTaxonListIdsAttribute()
    {
        // Capture model-bound filters before automatic model validation can short-circuit the action.
        Order = int.MinValue;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<ObservationApiConfiguration>();
        if (!configuration.EnableTaxonListLogging) return;

        var ids = context.ActionArguments.Values.SelectMany(argument => argument switch
        {
            SearchFilterBaseDto filter => filter.Taxon?.TaxonListIds ?? Enumerable.Empty<int>(),
            SignalFilterDto filter => filter.Taxon?.TaxonListIds ?? Enumerable.Empty<int>(),
            TaxonFilterDto filter => filter.TaxonListIds ?? Enumerable.Empty<int>(),
            TaxonFilterBaseDto filter => filter.TaxonListIds ?? Enumerable.Empty<int>(),
            _ => Enumerable.Empty<int>()
        });

        var taxonListIds = string.Join(",",
            ids.Distinct().OrderBy(id => id).Select(id => id.ToString(CultureInfo.InvariantCulture)));

        if (taxonListIds.Length > 0)
        {
            context.HttpContext.Items["TaxonListIds"] = taxonListIds;
        }
    }
}
