﻿using SOS.Harvest.Processors.DarwinCoreArchive;
using SOS.Harvest.Processors.iNaturalist;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Checklist;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Observations.Api.IntegrationTests.TestData;

namespace SOS.Observations.Api.IntegrationTests.Setup;
public interface IProcessFixture
{
    List<Taxon> Taxa { get; }
    IDictionary<VocabularyId, IDictionary<object, int>> DwcaVocabularyById { get; }
    Dictionary<int, Taxon> TaxonById { get; }
    Task AddDatasetsToElasticsearchAsync(IEnumerable<Dataset> datasets, bool clearExistingObservations = true, int delayInMs = 1000);
    Task AddDataToElasticsearchAsync(IEnumerable<Lib.Models.Processed.DataStewardship.Event.Event> events, IEnumerable<Observation> observations, bool clearExistingObservations = true);
    Task AddDataToElasticsearchAsync(List<Dataset> datasets, List<Lib.Models.Processed.DataStewardship.Event.Event> events, List<Observation> observations, bool clearExistingObservations = true);
    Task AddDataToElasticsearchAsync(TestDatas.TestDataSet testDataSet, bool clearExistingObservations = true);
    Task AddEventsToElasticsearchAsync(IEnumerable<Lib.Models.Processed.DataStewardship.Event.Event> events, bool clearExistingObservations = true, int delayInMs = 1000);
    Task AddObservationsToElasticsearchAsync(IEnumerable<Observation> observations, bool clearExistingObservations = true, int delayInMs = 100);
    Task<DwcaObservationFactory> CreateDarwinCoreFactoryAsync(DataProvider dataProvider);
    DwcaObservationFactory GetDarwinCoreFactory(bool initAreaHelper);
    iNaturalistObservationFactory GetiNaturalistFactory(bool initAreaHelper);
    Task<long> GetDatasetsCount();
    DwcaDatasetFactory GetDwcaDatasetFactory();
    DwcaEventFactory GetDwcaEventFactory(bool initAreaHelper);
    DwcaObservationFactory GetDwcaObservationFactory(bool initAreaHelper);
    Task<long> GetEventsCount();
    Task<long> GetObservationsCount(bool protectedIndex = false);
    Task ImportDwcaFileAsync(string filePath, DataProvider dataProvider, ITestOutputHelper output);
    Task ImportDwcaFilesAsync(IEnumerable<(string filePath, DataProvider dataProvider)> files, ITestOutputHelper output);
    Task ImportDwcaFileUsingDwcArchiveReaderAsync(string filePath, DataProvider dataProvider, ITestOutputHelper output);
    Task InitializeElasticsearchIndices();
    Task CleanElasticsearchIndices();
    Task ProcessAndAddChecklistsToElasticSearch(IEnumerable<ArtportalenChecklistVerbatim> verbatimChecklists);
    Task<List<Observation>> ProcessAndAddObservationsToElasticSearch(IEnumerable<ArtportalenObservationVerbatim> verbatimObservations);
    //Task<List<Observation>> ProcessAndAddObservationsToElasticSearchUsingObservationProcessor(IEnumerable<ArtportalenObservationVerbatim> verbatimObservations);
    List<Checklist> ProcessChecklists(IEnumerable<ArtportalenChecklistVerbatim> verbatimChecklists);
    List<Observation> ProcessObservations(IEnumerable<ArtportalenObservationVerbatim> verbatimObservations, bool enableDiffusion = false);
    List<Observation> ProcessObservations(IEnumerable<DwcObservationVerbatim> verbatimObservations, bool initAreaHelper = false);
}