﻿using Microsoft.Extensions.Diagnostics.HealthChecks;
using SOS.Lib.Models.Search.Filters;
using SOS.Observations.Api.Managers.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Observations.Api.HealthChecks
{
    public class SearchPerformanceHealthCheck : IHealthCheck
    {
        private readonly IObservationManager _observationManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        public SearchPerformanceHealthCheck(IObservationManager observationManager)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
        }

        private async Task<(TimeSpan Duration, long TotalCount)> SearchPicaPicaAsync()
        {
            var searchFilter = new SearchFilter(0)
            {
                Taxa = new TaxonFilter
                {
                    Ids = new[] { 103032 },
                    IncludeUnderlyingTaxa = true
                },
                VerificationStatus = SearchFilterBase.StatusVerification.BothVerifiedAndNotVerified,
                PositiveSightings = true,
                Output = new OutputFilter
                {
                    Fields = new List<string> { "taxon.id" }
                }
            };

            // Warm up
            await _observationManager.GetChunkAsync(0, null, searchFilter, 0, 2);

            var sw = new Stopwatch();
            sw.Start();
            var result = await _observationManager.GetChunkAsync(0, null, searchFilter, 0, 2);
            sw.Stop();

            return (Duration: sw.Elapsed, TotalCount: result.TotalCount);
        }

        /// <summary>
        /// Make health check
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var result = await SearchPicaPicaAsync();

                if (result.TotalCount == 0)
                {
                    return new HealthCheckResult(HealthStatus.Unhealthy, $"No observations returned. Duration: {result.Duration.TotalMilliseconds:N0}ms");
                }

                if (result.Duration.TotalMilliseconds > 15000)
                {
                    return new HealthCheckResult(HealthStatus.Degraded, $"Duration is > 15000ms. Duration: {result.Duration.TotalMilliseconds:N0}ms");
                }

                return new HealthCheckResult(HealthStatus.Healthy, $"Duration is {result.Duration.TotalMilliseconds:N0}ms");
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Health check failed", e);
            }
        }
    }
}