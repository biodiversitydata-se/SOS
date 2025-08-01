﻿using Elastic.Clients.Elasticsearch;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Analysis.Api.Repositories.Interfaces
{
    public interface IProcessedObservationRepository : IProcessedObservationCoreRepository
    {
        /// <summary>
        /// Aggregate by user passed field
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="precisionThreshold"></param>
        /// <param name="afterKey"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<SearchAfterResult<dynamic, IReadOnlyCollection<FieldValue>>> AggregateByUserFieldAsync(SearchFilter filter, string aggregationField, int? precisionThreshold, string? afterKey = null, int? take = 10);
    }
}
