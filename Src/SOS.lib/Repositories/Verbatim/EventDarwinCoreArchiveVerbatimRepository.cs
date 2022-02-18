using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    /// <summary>
    ///     Darwin core event verbatim repository
    /// </summary>
    public class EventDarwinCoreArchiveVerbatimRepository : VerbatimRepositoryBase<DwcEventVerbatim, int>,
        IEventDarwinCoreArchiveVerbatimRepository
    {
        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Mongodb collection name
        /// </summary>
        protected override string CollectionName => $"DwcaEvent_{_dataProvider.Id:D3}_{_dataProvider.Identifier}{(TempMode ? "_temp" : "")}";

        /// <inheritdoc />
        public IEnumerable<DistinictValueCount<string>> GetDistinctValuesCount(
            Expression<Func<DwcEventVerbatim, DistinctValueObject<string>>> expression,
            int limit)
        {
            var result = GetMongoCollection(CollectionName).Aggregate(new AggregateOptions { AllowDiskUse = true })
                .Project(expression)
                .Group(o => o.Value,
                    grouping => new DistinictValueCount<string> { Value = grouping.Key, Count = grouping.Count() })
                .SortByDescending(o => o.Count)
                .Limit(limit)
                .ToList();

            return result;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public EventDarwinCoreArchiveVerbatimRepository(
            DataProvider dataProvider,
            IVerbatimClient importClient,
            ILogger logger) : base(importClient, logger)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        }
    }

    /// <summary>
    ///     Darwin core event occurrence verbatim repository
    /// </summary>
    public class EventOccurrenceDarwinCoreArchiveVerbatimRepository : VerbatimRepositoryBase<DwcEventOccurrenceVerbatim, int>,
        IEventOccurrenceDarwinCoreArchiveVerbatimRepository
    {
        private readonly DataProvider _dataProvider;
        private readonly IVerbatimRepositoryBase<DwcObservationVerbatim, int> _observationsRepository;

        /// <summary>
        /// Mongodb collection name
        /// </summary>
        protected override string CollectionName => $"DwcaEventOccurrence_{_dataProvider.Id:D3}_{_dataProvider.Identifier}{(TempMode ? "_temp" : "")}";

        private string CollectionNameObservations => $"DwcaEventOccurrence_{_dataProvider.Id:D3}_{_dataProvider.Identifier}_Obs{(TempMode ? "_temp" : "")}";

        /// <inheritdoc />
        public IEnumerable<DistinictValueCount<string>> GetDistinctValuesCount(
            Expression<Func<DwcEventOccurrenceVerbatim, DistinctValueObject<string>>> expression,
            int limit)
        {
            var result = GetMongoCollection(CollectionName).Aggregate(new AggregateOptions { AllowDiskUse = true })
                .Project(expression)
                .Group(o => o.Value,
                    grouping => new DistinictValueCount<string> { Value = grouping.Key, Count = grouping.Count() })
                .SortByDescending(o => o.Count)
                .Limit(limit)
                .ToList();

            return result;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public EventOccurrenceDarwinCoreArchiveVerbatimRepository(
            DataProvider dataProvider,
            IVerbatimClient importClient,
            ILogger logger) : base(importClient, logger)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));

            _observationsRepository =
                new VerbatimRepositoryBase<DwcObservationVerbatim, int>(importClient, CollectionNameObservations, logger);
        }

        /// <inheritdoc />
        public override async Task<bool> AddCollectionAsync()
        {
            var added = (await Task.WhenAll(new[]
            {
                base.AddCollectionAsync(),
                _observationsRepository.AddCollectionAsync()
            })).All(t => t);

            if (!added)
            {
                return false;
            }

            var indexModels = new[]
            {
                new CreateIndexModel<DwcObservationVerbatim>(
                    Builders<DwcObservationVerbatim>.IndexKeys.Ascending(io => io.EventID))
            };

            await _observationsRepository.AddIndexes(indexModels);

            return true;
        }

        /// <inheritdoc />
        public override async Task<bool> AddManyAsync(IEnumerable<DwcEventOccurrenceVerbatim> eventRecords)
        {
            if (!eventRecords?.Any() ?? true)
            {
                return true;
            }

            // Store observations in own collection
            foreach (var eventRecord in eventRecords)
            {
                await _observationsRepository.AddManyAsync(eventRecord.Observations);
                eventRecord.Observations = null;
            }

            return await AddManyAsync(eventRecords, MongoCollection);
        }

        /// <inheritdoc />
        public override async Task<bool> DeleteCollectionAsync()
        {
            return (await Task.WhenAll(new[]
            {
                base.DeleteCollectionAsync(),
                _observationsRepository.DeleteCollectionAsync()
            })).All(t => t);
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<DwcEventOccurrenceVerbatim>> GetBatchAsync(int startId, int endId)
        {
            var events = (await base.GetBatchAsync(startId, endId, MongoCollection))?.ToArray();

            if (!events?.Any() ?? true)
            {
                return null;
            }

            foreach (var eventRecord in events)
            {
                var observationsFilter = Builders<DwcObservationVerbatim>.Filter.Eq(e => e.EventID, eventRecord.EventID);
                eventRecord.Observations = (await _observationsRepository.QueryAsync(observationsFilter))?.ToList();
            }

            return events;
        }

        /// <inheritdoc />
        public override async Task<bool> PermanentizeCollectionAsync()
        {
            return (await Task.WhenAll(new[]
            {
                base.PermanentizeCollectionAsync(),
                _observationsRepository.PermanentizeCollectionAsync()
            })).All(t => t);
        }

        /// <inheritdoc />
        public override bool TempMode
        {
            get => base.TempMode;
            set
            {
                _observationsRepository.TempMode = value;
                base.TempMode = value;
            }
        }
    }

}