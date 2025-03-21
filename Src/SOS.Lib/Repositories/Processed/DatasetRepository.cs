using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Search.Filters;
//using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Dataset observation repository.
    /// </summary>
    public class DatasetRepository : ProcessRepositoryBase<Dataset, string>,
        IDatasetRepository
    {
        /// <summary>
        /// Add the collection
        /// </summary>
        /// <returns></returns>
        private async Task<bool> AddCollectionAsync()
        {
            var createIndexResponse = await Client.Indices.CreateAsync<Dataset>(IndexName, s => s
               .Settings(s => s
                   .NumberOfShards(NumberOfShards)
                   .NumberOfReplicas(NumberOfReplicas)
                   .Settings(s => s
                       .MaxResultWindow(100000)
                       .MaxTermsCount(110000)
                   )
               )
               .Mappings(map => map
                   .Properties(ps => ps
                        .KeywordVal(kwlc => kwlc.Id, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.Identifier)
                        .KeywordVal(kwlc => kwlc.DataStewardship)
                        .KeywordVal(kwlc => kwlc.Title, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.ProgrammeArea, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.DescriptionAccessRights, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.License, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.Description, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.Spatial, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.Language, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.Metadatalanguage, IndexSetting.None)
                        .Object(o => o.Project, p => p
                            .Properties(ps => ps
                                .KeywordVal(kwlc => kwlc.Project.First().ProjectId)
                                .KeywordVal(kwlc => kwlc.Project.First().ProjectCode)
                                .NumberVal(kwlc => kwlc.Project.First().ProjectType, IndexSetting.SearchSortAggregate, NumberType.Byte)
                            )
                        )
                        .Object(o => o.Assigner, p => p
                            .Properties(ps => ps
                                .KeywordVal(kwlc => kwlc.Assigner.OrganisationCode, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Assigner.OrganisationID, IndexSetting.None)
                            )
                        )
                        .Object(o => o.Creator, p => p
                            .Properties(ps => ps
                                .KeywordVal(kwlc => kwlc.Creator.First().OrganisationCode, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Creator.First().OrganisationID, IndexSetting.None)
                            )
                        )
                        .Object(o => o.OwnerinstitutionCode, p => p
                            .Properties(ps => ps
                                .KeywordVal(kwlc => kwlc.OwnerinstitutionCode.OrganisationCode, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.OwnerinstitutionCode.OrganisationID, IndexSetting.None)
                            )
                        )
                        .Object(o => o.Publisher, p => p
                            .Properties(ps => ps
                                .KeywordVal(kwlc => kwlc.Publisher.OrganisationCode, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Publisher.OrganisationID, IndexSetting.None)
                            )
                        )
                    )
                )
            );

            return createIndexResponse.Acknowledged && createIndexResponse.IsValidResponse ? true : throw new Exception($"Failed to create Dataset index. Error: {createIndexResponse.DebugInformation}");
        }

        public async Task<List<Dataset>> GetDatasetsByIds(IEnumerable<string> ids, IEnumerable<string> excludeFields = null, IEnumerable<SortOrderFilter> sortOrders = null)
        {
            if (ids == null || !ids.Any()) throw new ArgumentException("ids is empty");

            var sortDescriptor = await Client.GetSortDescriptorAsync<Dataset>(IndexName, sortOrders);

            var source = new SourceConfig(new SourceFilter { 
                Excludes = excludeFields != null && excludeFields.Count() > 0 ? Fields.FromStrings(excludeFields.ToArray()) : null,
                Includes = new[] { "occurrence.occurrenceId" }.ToFields() 
            });
            var queries = new List<Action<QueryDescriptor<Dataset>>>();
            queries.Add(q => q
                .TryAddTermsCriteria("identifier", ids)
            );
          
            var searchResponse = await Client.SearchAsync<Dataset>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(queries.ToArray())
                    )
                )
                .Source(source)
                .Size(ids?.Count() ?? 0)
                .Sort(sortDescriptor.ToArray())
                .TrackTotalHits(new TrackHits(false))
            );

            if (!searchResponse.IsValidResponse) throw new InvalidOperationException(searchResponse.DebugInformation);
            var datasets = searchResponse.Documents.ToList();
            return datasets;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="clusterHealthCache"></param>
        /// <param name="logger"></param>
        public DatasetRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
            ILogger<DatasetRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, clusterHealthCache, logger)
        {
            LiveMode = true;
            _id = nameof(Models.Processed.Observation.Observation); // The active instance should be the same as the ProcessedObservationRepository which uses the Observation type.
        }

        /// <inheritdoc />
        public async Task<bool> ClearCollectionAsync()
        {
            await DeleteCollectionAsync();
            return await AddCollectionAsync();
        }

        /// <inheritdoc />
        public string UniqueIndexName => IndexHelper.GetIndexName<Dataset>(IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, false);

        /// <inheritdoc />
        public async Task<bool> VerifyCollectionAsync()
        {
            var response = await Client.Indices.ExistsAsync(IndexName);

            if (!response.Exists)
            {
                await AddCollectionAsync();
            }

            return !response.Exists;
        }

        public async Task WaitForIndexCreation(long expectedRecordsCount, TimeSpan? timeout = null)
        {
            Logger.LogInformation($"Begin waiting for index creation. Index={IndexName}, ExpectedRecordsCount={expectedRecordsCount}, Timeout={timeout}");
            if (timeout == null) timeout = TimeSpan.FromMinutes(10);
            var sleepTime = TimeSpan.FromSeconds(5);
            int nrIterations = (int)(Math.Ceiling(timeout.Value.TotalSeconds / sleepTime.TotalSeconds));
            long docCount = await IndexCountAsync();
            var iterations = 0;

            // Compare number of documents processed with actually db count
            // If docCount is less than process count, indexing is not ready yet
            while (docCount < expectedRecordsCount && iterations < nrIterations)
            {
                iterations++; // Safety to prevent infinite loop.                                
                await Task.Delay(sleepTime);
                docCount = await IndexCountAsync();
            }

            if (iterations == nrIterations)
            {
                Logger.LogError($"Failed waiting for index creation due to timeout. Index={IndexName}. ExpectedRecordsCount={expectedRecordsCount}, DocCount={docCount}");
            }
            else
            {
                Logger.LogInformation($"Finish waiting for index creation. Index={IndexName}.");
            }
        }
    }
}