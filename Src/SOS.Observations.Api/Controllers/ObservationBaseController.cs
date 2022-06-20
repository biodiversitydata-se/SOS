using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    /// Base controller for observation related controller
    /// </summary>
    public class ObservationBaseController : SearchBaseController
    {
        private readonly ITaxonManager _taxonManager;

        protected readonly IObservationManager ObservationManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="areaManager"></param>
        /// <param name="taxonManager"></param>
        protected ObservationBaseController(IObservationManager observationManager,
            IAreaManager areaManager, ITaxonManager taxonManager) : base(areaManager)
        {
            ObservationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
        }

        protected string ReplaceDomain(string str, string domain, string path)
        {
            // This is a bad solution to fix problems when behind load balancer...
            return Regex.Replace(str, string.Format(@"(https?:\/\/.*?)(\/{0}\/v2)?(\/.*)", path), m => domain + m.Groups[3].Value);
        }

        protected Result ValidateAggregationPagingArguments(int skip, int? take, bool handleInfinityTake = false)
        {
            if (skip < 0) return Result.Failure("Skip must be 0 or greater.");

            if (handleInfinityTake && take == -1)
            {
                return Result.Success();
            }

            if (take <= 0) return Result.Failure("Take must be greater than 0");
            if (take != null && skip + take > ObservationManager.MaxNrElasticSearchAggregationBuckets)
                return Result.Failure($"Skip+Take={skip + take}. Skip+Take must be less than or equal to {ObservationManager.MaxNrElasticSearchAggregationBuckets}.");

            return Result.Success();
        }

        protected override Result ValidateSearchFilter(SearchFilterBaseDto filter, bool bboxMandatory = false)
        {
            var result = base.ValidateSearchFilter(filter, bboxMandatory);

            var taxaValidationResult = ValidateTaxa(filter.Taxon?.Ids);
            if (taxaValidationResult.IsFailure)
            {
                if (result.IsFailure)
                {
                    return Result.Failure($"{result.Error}, {taxaValidationResult.Error}");
                }
                return taxaValidationResult;
            }

            return result;
        }

            protected Result ValidateTaxa(IEnumerable<int> taxonIds)
        {
            var missingTaxa = taxonIds?
                .Where(tid => !_taxonManager.TaxonTree.TreeNodeById.ContainsKey(tid))
                .Select(tid => $"TaxonId doesn't exist ({tid})");

            return missingTaxa?.Any() ?? false ?
                Result.Failure(string.Join(". ", missingTaxa))
                :
                Result.Success();
        }
    }
}
