using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Verbatim
{
    /// <summary>
    ///     Darwin core occurrence verbatim repository
    /// </summary>
    public class DarwinCoreArchiveVerbatimRepository : VerbatimRepositoryBase<DwcObservationVerbatim, int>,
        IDarwinCoreArchiveVerbatimRepository
    {
        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Mongodb collection name
        /// </summary>
        protected override string CollectionName => $"DwcaOccurrence_{_dataProvider.Id:D3}_{_dataProvider.Identifier}{(TempMode ? "_temp" : "")}";

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
        public DarwinCoreArchiveVerbatimRepository(
            DataProvider dataProvider,
            IVerbatimClient importClient,
            ILogger logger) : base(importClient, logger)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        }


        /// <inheritdoc/>
        public async Task<Stream> GetSourceFileAsync()
        {
            return await base.GetSourceFileAsync(_dataProvider.Id);
        }

        /// <inheritdoc/>
        public async Task<bool> StoreSourceFileAsync(Stream fileStream)
        {
           fileStream.Position = 0;
           return await base.StoreSourceFileAsync(_dataProvider.Id, fileStream);
        }
    }
}