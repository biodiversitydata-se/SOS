using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public abstract class ProcessedObservationBaseRepository : ProcessRepositoryBase<Observation, string>
    {
        /// <summary>
        /// Get core queries
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skipAuthorizationFilters"></param>
        /// <returns></returns>
        protected (ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>, ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>>) 
            GetCoreQueries(SearchFilterBase filter, bool skipAuthorizationFilters = false)
        {
            var query = filter.ToQuery(skipAuthorizationFilters: skipAuthorizationFilters);
            var excludeQuery = filter.ToExcludeQuery();

            return (query, excludeQuery);
        }

        /// <summary>
        /// Get public index name and also protected index name if user is authorized
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        protected string GetCurrentIndex(SearchFilterBase filter)
        {
            if (
                (
                    (filter.ExtendedAuthorization.ObservedByMe) || 
                    (filter.ExtendedAuthorization.ReportedByMe) || 
                    (!filter.ExtendedAuthorization.ProtectionFilter.Equals(ProtectionFilter.Public))
                ) &&
                (filter?.ExtendedAuthorization.UserId ?? 0) == 0
            )
            {
                throw new AuthenticationRequiredException("Not authenticated");
            }

            return filter.ExtendedAuthorization.ProtectionFilter switch
            {
                ProtectionFilter.BothPublicAndSensitive => $"{PublicIndexName}, {ProtectedIndexName}",
                ProtectionFilter.Sensitive => ProtectedIndexName,
                _ => PublicIndexName
            };
        }

        /// <summary>
        /// Constructor used in admin mode
        /// </summary>
        /// <param name="liveMode"></param>
        /// <param name="elasticClientManager"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessedObservationBaseRepository(
            bool liveMode,
            IElasticClientManager elasticClientManager,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ProcessedObservationBaseRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
        {
            LiveMode = liveMode;
        }

        /// <inheritdoc />
        public string PublicIndexName => IndexName;

        /// <inheritdoc />
        public string ProtectedIndexName => IndexHelper.GetIndexName<Observation>(IndexPrefix, ClientCount == 1, LiveMode ? ActiveInstance : InActiveInstance, true);
    }
}
