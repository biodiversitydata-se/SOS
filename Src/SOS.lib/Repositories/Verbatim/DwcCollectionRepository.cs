using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Log;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{    
    /// <summary>
    ///     Darwin core event verbatim repository
    /// </summary>
    public class DwcCollectionRepository : IDisposable    
    {        
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly DataProvider _dataProvider;
        private readonly EventVerbatimRepository _eventRepository;
        private readonly OccurrenceVerbatimRepository _occurrenceRepository;
        private readonly DatasetVerbatimRepository _datasetRepository;
        
        public DatasetVerbatimRepository DatasetRepository => _datasetRepository;
        public EventVerbatimRepository EventRepository => _eventRepository;
        public OccurrenceVerbatimRepository OccurrenceRepository => _occurrenceRepository;        
        public void BeginTempMode()
        {
            DatasetRepository.TempMode = true;
            EventRepository.TempMode = true;
            OccurrenceRepository.TempMode = true;
        }

        public void EndTempMode()
        {
            DatasetRepository.TempMode = false;
            EventRepository.TempMode = false;
            OccurrenceRepository.TempMode = false;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public DwcCollectionRepository(
            DataProvider dataProvider,
            IVerbatimClient importClient,
            Microsoft.Extensions.Logging.ILogger logger) //: base(importClient, logger)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
            _eventRepository = new EventVerbatimRepository(
                    dataProvider,
                    importClient,
                    logger);

            _occurrenceRepository = new OccurrenceVerbatimRepository(
                    dataProvider,
                    importClient,
                    logger);

            _datasetRepository = new DatasetVerbatimRepository(
                    dataProvider,
                    importClient,
                    logger);
            
        }

        public async Task<bool> DeleteCollectionsAsync(bool? tempMode = true)
        {
            bool[] results = await Task.WhenAll(
                OccurrenceRepository.DeleteCollectionAsync(),
                EventRepository.DeleteCollectionAsync(tempMode),
                DatasetRepository.DeleteCollectionAsync()
            );

            return results.All(m => m == true);
        }

        public async Task<bool> AddCollectionsAsync()
        {
            bool[] results = await Task.WhenAll(
                OccurrenceRepository.AddCollectionAsync(),
                EventRepository.AddCollectionAsync(),
                DatasetRepository.AddCollectionAsync()
            );

            return results.All(m => m == true);
        }

        public async Task<bool> PermanentizeCollectionsAsync()
        {            
            bool[] results = await Task.WhenAll(
                OccurrenceRepository.PermanentizeCollectionAsync(),
                EventRepository.PermanentizeCollectionAsync(),
                DatasetRepository.PermanentizeCollectionAsync()
            );

            return results.All(m => m == true);
        }

        public void Dispose()
        {
            
        }
    }

    public class DatasetVerbatimRepository : VerbatimRepositoryBase<DwcVerbatimDataset, int>
    {
        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Mongodb collection name
        /// </summary>
        protected override string CollectionName => $"DwcaCollection_{_dataProvider.Id:D3}_{_dataProvider.Identifier}_Datasets{(TempMode ? "_temp" : "")}";

        /// <inheritdoc />
        public IEnumerable<DistinictValueCount<string>> GetDistinctValuesCount(
            Expression<Func<DwcVerbatimDataset, DistinctValueObject<string>>> expression,
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
        public DatasetVerbatimRepository(
            DataProvider dataProvider,
            IVerbatimClient importClient,
            Microsoft.Extensions.Logging.ILogger logger) : base(importClient, logger)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        }

        /// <inheritdoc />
        public override async Task<bool> AddCollectionAsync()
        {            
            var added = await base.AddCollectionAsync();
            if (!added) return false;

            var indexModels = new[]
            {
                new CreateIndexModel<DwcVerbatimDataset>(
                    Builders<DwcVerbatimDataset>.IndexKeys.Ascending(io => io.Identifier))
            };
            await AddIndexes(indexModels);

            return true;
        }

        /// <inheritdoc />
        public override async Task<bool> AddManyAsync(IEnumerable<DwcVerbatimDataset> datasetRecords)
        {
            if (!datasetRecords?.Any() ?? true)
            {
                return true;
            }

            return await AddManyAsync(datasetRecords, MongoCollection);
        }

        /// <inheritdoc />
        public override async Task<bool> DeleteCollectionAsync()
        {
            var result = await base.DeleteCollectionAsync();
            return result;
        }        

        /// <inheritdoc />
        public override async Task<bool> PermanentizeCollectionAsync()
        {
            var result = await base.PermanentizeCollectionAsync();
            return result;
        }       
    }

    /// <summary>
    ///     Darwin core event occurrence verbatim repository
    /// </summary>
    public class EventVerbatimRepository : VerbatimRepositoryBase<DwcEventOccurrenceVerbatim, int>,
        IEventOccurrenceDarwinCoreArchiveVerbatimRepository
    {
        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Mongodb collection name
        /// </summary>
        protected override string CollectionName => GetCollectionName(TempMode);
        
        protected override string GetCollectionName(bool? tempMode)
        {
            if (tempMode.HasValue)
                return $"DwcaCollection_{_dataProvider.Id:D3}_{_dataProvider.Identifier}_Events{(tempMode.Value ? "_temp" : "")}";
            else
                return $"DwcaCollection_{_dataProvider.Id:D3}_{_dataProvider.Identifier}_Events{(TempMode ? "_temp" : "")}";
        }

        protected IMongoCollection<DwcEventOccurrenceVerbatim> GetMongoCollection(bool? tempMode) => GetMongoCollection(GetCollectionName(tempMode));

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
        public EventVerbatimRepository(
            DataProvider dataProvider,
            IVerbatimClient importClient,
            Microsoft.Extensions.Logging.ILogger logger) : base(importClient, logger)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        }

        /// <inheritdoc />
        public override async Task<bool> AddCollectionAsync(bool? tempMode = true)
        {
            string collectionName = GetCollectionName(tempMode);
            var added = await AddCollectionAsync(collectionName);
            if (!added) return false;

            var indexModels = new[]
            {
                new CreateIndexModel<DwcEventOccurrenceVerbatim>(
                    Builders<DwcEventOccurrenceVerbatim>.IndexKeys.Ascending(io => io.EventID))
            };            
            await AddIndexes(indexModels);

            return true;
        }

        /// <inheritdoc />
        public override async Task<bool> AddManyAsync(IEnumerable<DwcEventOccurrenceVerbatim> eventRecords)
        {
            if (!eventRecords?.Any() ?? true)
            {
                return true;
            }

            return await AddManyAsync(eventRecords, MongoCollection);
        }

        public async Task<bool> AddManyAsync(IEnumerable<DwcEventOccurrenceVerbatim> eventRecords, bool? tempMode = true)
        {
            if (!eventRecords?.Any() ?? true)
            {
                return true;
            }
           
            var mongoCollection = GetMongoCollection(tempMode);
            return await AddManyAsync(eventRecords, mongoCollection);
        }

        /// <inheritdoc />
        public override async Task<bool> DeleteCollectionAsync()
        {
            var result = await DeleteCollectionAsync();
            return result;
        }

        public async Task<bool> DeleteCollectionAsync(bool? tempMode = null)
        {
            string collectionName = GetCollectionName(tempMode);
            var result = await DeleteCollectionAsync(collectionName);
            return result;            
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<DwcEventOccurrenceVerbatim>> GetBatchAsync(int startId, int endId)
        {
            var events = (await base.GetBatchAsync(startId, endId, MongoCollection))?.ToArray();

            if (!events?.Any() ?? true)
            {
                return null;
            }
           
            return events;
        }
    }


    public class OccurrenceVerbatimRepository : VerbatimRepositoryBase<DwcObservationVerbatim, int>, IDarwinCoreArchiveVerbatimRepository        
    {
        private readonly DataProvider _dataProvider;        

        /// <summary>
        /// Mongodb collection name
        /// </summary>
        protected override string CollectionName => GetCollectionName(TempMode);

        protected override string GetCollectionName(bool? tempMode)
        {
            if (tempMode.HasValue)
                return $"DwcaCollection_{_dataProvider.Id:D3}_{_dataProvider.Identifier}_Observations{(tempMode.Value ? "_temp" : "")}";
            else
                return $"DwcaCollection_{_dataProvider.Id:D3}_{_dataProvider.Identifier}_Observations{(TempMode ? "_temp" : "")}";
        }

        /// <inheritdoc />
        public IEnumerable<DistinictValueCount<string>> GetDistinctValuesCount(
            Expression<Func<DwcObservationVerbatim, DistinctValueObject<string>>> expression,
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
        public OccurrenceVerbatimRepository(
            DataProvider dataProvider,
            IVerbatimClient importClient,
            Microsoft.Extensions.Logging.ILogger logger) : base(importClient, logger)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        }

        /// <inheritdoc />
        public override async Task<bool> AddCollectionAsync()
        {
            var added = await base.AddCollectionAsync();            
            if (!added) return false;            

            var indexModels = new[]
            {
                new CreateIndexModel<DwcObservationVerbatim>(
                    Builders<DwcObservationVerbatim>.IndexKeys.Ascending(io => io.EventID)),
                new CreateIndexModel<DwcObservationVerbatim>(
                    Builders<DwcObservationVerbatim>.IndexKeys.Ascending(io => io.OccurrenceID))
            };
            await AddIndexes(indexModels);

            return true;
        }

        /// <inheritdoc />
        public override async Task<bool> AddManyAsync(IEnumerable<DwcObservationVerbatim> occurrenceRecords)
        {
            if (!occurrenceRecords?.Any() ?? true)
            {
                return true;
            }            

            return await AddManyAsync(occurrenceRecords, MongoCollection);
        }

        /// <inheritdoc />
        public override async Task<bool> DeleteCollectionAsync()
        {
            var result = await base.DeleteCollectionAsync();
            return result;
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<DwcObservationVerbatim>> GetBatchAsync(int startId, int endId)
        {
            var occurrences = (await base.GetBatchAsync(startId, endId, MongoCollection))?.ToArray();

            if (!occurrences?.Any() ?? true)
            {
                return null;
            }
          
            return occurrences;
        }
    }
}