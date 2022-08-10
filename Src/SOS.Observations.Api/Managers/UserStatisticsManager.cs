using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     User statistics manager.
    /// </summary>
    public class UserStatisticsManager : IUserStatisticsManager
    {
        private readonly IUserObservationRepository _userObservationRepository;
        private readonly ILogger<UserStatisticsManager> _logger;
        private readonly Dictionary<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>> _userStatisticsItemsCache = new Dictionary<SpeciesCountUserStatisticsQuery, List<UserStatisticsItem>>(); // todo - use proper cache solution.
        private readonly Dictionary<PagedSpeciesCountUserStatisticsQuery, PagedResult<UserStatisticsItem>> _pagedUserStatisticsItemsCache = new Dictionary<PagedSpeciesCountUserStatisticsQuery, PagedResult<UserStatisticsItem>>(); // todo - use proper cache solution.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userObservationRepository"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public UserStatisticsManager(
            IUserObservationRepository userObservationRepository,
            ILogger<UserStatisticsManager> logger)
        {
            _userObservationRepository = userObservationRepository ??
                                              throw new ArgumentNullException(nameof(userObservationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Make sure we are working with live data
            _userObservationRepository.LiveMode = true;
        }

        public async Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query, 
            int? skip, 
            int? take,
            string sortBy,
            bool useCache = true)
        {
            PagedResult<UserStatisticsItem> result;
            var pagedQuery = PagedSpeciesCountUserStatisticsQuery.Create(query, skip, take, sortBy);
            if (useCache && _pagedUserStatisticsItemsCache.ContainsKey(pagedQuery))
            {
                result = _pagedUserStatisticsItemsCache[pagedQuery];
                return result;
            }

            if (!query.IncludeOtherAreasSpeciesCount)
            {
                result = await _userObservationRepository.PagedSpeciesCountSearchAsync(query, skip, take, sortBy);
            }
            else
            {
                var pagedResult = await _userObservationRepository.PagedSpeciesCountSearchAsync(query, skip, take, sortBy);
                var userIds = pagedResult.Records.Select(m => m.UserId).ToList();
                var updatedRecords = await _userObservationRepository.AreaSpeciesCountSearchAsync(query, userIds);
                pagedResult.Records = updatedRecords;
                result = pagedResult;
            }
            
            if (useCache && !_pagedUserStatisticsItemsCache.ContainsKey(pagedQuery))
            {
                _pagedUserStatisticsItemsCache.Add(pagedQuery, result); // todo - fix proper caching solution and concurrency handling.
            }
            
            return result;
        }

        public async Task<PagedResult<UserStatisticsItem>> SpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery query,
            int? skip,
            int? take,
            string sortBy,
            bool useCache = true)
        {
            List<UserStatisticsItem> records;
            if (useCache && _userStatisticsItemsCache.ContainsKey(query))
            {
                records = _userStatisticsItemsCache[query];
            }
            else
            {
                if (!query.IncludeOtherAreasSpeciesCount)
                {
                    records = await _userObservationRepository.SpeciesCountSearchAsync(query);
                }
                else
                {
                    // todo - implement logic. Calculate taxa count for each user and province.
                    records = null;
                }
            }

            UpdateSkipAndTake(ref skip, ref take, records.Count);
            var selectedRecords = records
                .OrderByDescending(m => m.SpeciesCount) // todo - use sortBy to decide sort property
                .Skip(skip.GetValueOrDefault())
                .Take(take.GetValueOrDefault());

            var result = new PagedResult<UserStatisticsItem>
            {
                Skip = skip.GetValueOrDefault(),
                Take = take.GetValueOrDefault(),
                TotalCount = records.Count,
                Records = selectedRecords
            };

            if (useCache && !_userStatisticsItemsCache.ContainsKey(query))
            {
                _userStatisticsItemsCache.Add(query, records); // todo - fix proper caching solution and concurrency handling.
            }

            return result;
        }


        private static void UpdateSkipAndTake(ref int? skip, ref int? take, int recordCount)
        {
            if (skip == null)
            {
                skip = 0;
            }

            if (skip > recordCount)
            {
                skip = recordCount;
            }

            if (take == null)
            {
                take = recordCount - skip;
            }
            else
            {
                take = Math.Min(recordCount - skip.Value, take.Value);
            }
        }
    }
}