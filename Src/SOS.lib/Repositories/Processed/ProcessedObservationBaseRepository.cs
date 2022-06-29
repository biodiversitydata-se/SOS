﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
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
    public class ProcessedObservationBaseRepository : ProcessRepositoryBase<Observation, string>
    {
       

        /// <summary>
        /// Get core queries
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        protected (ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>, ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>>) GetCoreQueries(SearchFilterBase filter)
        {
            var query = filter.ToQuery();
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
            if (((filter?.ExtendedAuthorization.ObservedByMe ?? false) || (filter?.ExtendedAuthorization.ReportedByMe ?? false) || (filter?.ExtendedAuthorization.ProtectedObservations ?? false)) &&
                (filter?.ExtendedAuthorization.UserId ?? 0) == 0)
            {
                throw new AuthenticationRequiredException("Not authenticated");
            }

            if (!filter?.ExtendedAuthorization.ProtectedObservations ?? true)
            {
                return PublicIndexName;
            }

            if (!HttpContextAccessor?.HttpContext?.User?.HasAccessToScope(ProtectedScope) ?? true)
            {
                throw new AuthenticationRequiredException("Not authorized");
            }

            return ProtectedIndexName;
        }

        /// <summary>
        /// Http context accessor.
        /// </summary>
        public IHttpContextAccessor HttpContextAccessor { get; set; }
        

        /// <summary>
        /// Constructor used in public mode
        /// </summary>
        /// <param name="liveMode"></param>
        /// <param name="elasticClientManager"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ProcessedObservationBaseRepository(
            bool liveMode,
            IElasticClientManager elasticClientManager,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            IHttpContextAccessor httpContextAccessor,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ProcessedObservationBaseRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
        {
            LiveMode = liveMode;
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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