using Elastic.Clients.Elasticsearch.Cluster;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.ApplicationInsights;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.IO.Excel;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.IO.GeoJson;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Cache;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Security;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.ApplicationInsights;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories;
using SOS.Observations.Api.Repositories.Interfaces;
using SOS.Observations.Api.Services.Interfaces;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Utilities.Objects;
using SOS.Shared.Api.Utilities.Objects.Interfaces;
using SOS.Shared.Api.Validators;
using SOS.Shared.Api.Validators.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DataProviderManager = SOS.Observations.Api.Managers.DataProviderManager;
using IDataProviderManager = SOS.Observations.Api.Managers.Interfaces.IDataProviderManager;

namespace SOS.Observations.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencyInjectionServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        // Add application insights.
        var applicationInsightsConfiguration = Settings.ApplicationInsightsConfiguration;
        services.AddSingleton(applicationInsightsConfiguration);        
        services.AddApplicationInsightsTelemetry(c =>
        {
            c.ConnectionString = applicationInsightsConfiguration.ConnectionString;
        });        
        services.AddApplicationInsightsTelemetryProcessor<IgnoreRequestPathsTelemetryProcessor>();
        services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();        

        //setup the Elasticsearch configuration
        var elasticConfiguration = Settings.SearchDbConfiguration;
        services.AddSingleton<IElasticClientManager, ElasticClientManager>(p => new ElasticClientManager(elasticConfiguration));
        services.AddSingleton(elasticConfiguration);

        // Processed Mongo Db
        var processedDbConfiguration = Settings.ProcessDbConfiguration;
        var processedSettings = processedDbConfiguration.GetMongoDbSettings();
        services.AddScoped<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
            processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));

        // Add configuration
        services.AddSingleton(configuration);
        services.AddSingleton(Settings.SemaphoreLimitsConfiguration);
        services.AddSingleton(Settings.ArtportalenApiServiceConfiguration);
        services.AddSingleton(Settings.ObservationApiConfiguration);
        services.AddSingleton(Settings.BlobStorageConfiguration);
        services.AddSingleton(Settings.DevOpsConfiguration);
        services.AddSingleton(Settings.InputValidationConfiguration!);
        services.AddSingleton(Settings.UserServiceConfiguration);
        services.AddSingleton(Settings.HealthCheckConfiguration);
        services.AddSingleton(Settings.VocabularyConfiguration);
        services.AddSingleton(Settings.AreaConfiguration);

        // Add security
        services.AddScoped<IAuthorizationProvider, CurrentUserAuthorization>();
        services.AddSingleton(Settings.CryptoConfiguration);

        // Add Caches
        services.AddSingleton<IAreaCache, AreaCache>();
        services.AddSingleton<ITaxonObservationCountCache, TaxonObservationCountCache>();
        services.AddSingleton<IDataProviderCache, DataProviderCache>();
        services.AddSingleton<ICache<int, ProjectInfo>, ProjectCache>();
        services.AddSingleton<ICache<VocabularyId, Vocabulary>, VocabularyCache>();
        services.AddSingleton<ICache<int, TaxonList>, TaxonListCache>();
        services.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
        services.AddSingleton<IClassCache<TaxonTree<IBasicTaxon>>, ClassCache<TaxonTree<IBasicTaxon>>>();
        services.AddSingleton<IClassCache<TaxonListSetsById>, ClassCache<TaxonListSetsById>>();
        var taxonSumAggregationCache = new ClassCache<Dictionary<int, TaxonSumAggregationItem>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<Dictionary<int, TaxonSumAggregationItem>>>()) { CacheDuration = TimeSpan.FromHours(48) };
        services.AddSingleton<IClassCache<Dictionary<int, TaxonSumAggregationItem>>>(taxonSumAggregationCache);
        var geoGridAggregationCache = new ClassCache<Dictionary<string, CacheEntry<GeoGridResultDto>>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<Dictionary<string, CacheEntry<GeoGridResultDto>>>>()) { CacheDuration = TimeSpan.FromHours(48) };
        services.AddSingleton<IClassCache<Dictionary<string, CacheEntry<GeoGridResultDto>>>>(geoGridAggregationCache);
        var taxonAggregationInternalCache = new ClassCache<Dictionary<string, CacheEntry<PagedResultDto<TaxonAggregationItemDto>>>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<Dictionary<string, CacheEntry<PagedResultDto<TaxonAggregationItemDto>>>>>()) { CacheDuration = TimeSpan.FromHours(1) };
        services.AddSingleton<IClassCache<Dictionary<string, CacheEntry<PagedResultDto<TaxonAggregationItemDto>>>>>(taxonAggregationInternalCache);
        var clusterHealthCache = new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()) { CacheDuration = TimeSpan.FromMinutes(2) };
        services.AddSingleton<IClassCache<ConcurrentDictionary<string, HealthResponse>>>(clusterHealthCache);
        services.AddSingleton<SortableFieldsCache>();

        // Add managers
        services.AddScoped<IAreaManager, AreaManager>();
        services.AddSingleton<IBlobStorageManager, BlobStorageManager>();
        services.AddSingleton<IChecklistManager, ChecklistManager>();
        services.AddScoped<IDataProviderManager, DataProviderManager>();
        services.AddScoped<IDataQualityManager, DataQualityManager>();
        services.AddScoped<IDataStewardshipManager, DataStewardshipManager>();
        services.AddScoped<IDevOpsManager, DevOpsManager>();
        services.AddScoped<IExportManager, ExportManager>();
        services.AddScoped<IFilterManager, FilterManager>();
        services.AddScoped<ILocationManager, LocationManager>();
        services.AddScoped<IObservationManager, ObservationManager>();
        services.AddScoped<IProcessInfoManager, ProcessInfoManager>();
        services.AddScoped<IProjectManager, ProjectManager>();
        services.AddScoped<ITaxonListManager, TaxonListManager>();
        services.AddSingleton<ITaxonManager, TaxonManager>();
        services.AddScoped<ITaxonSearchManager, TaxonSearchManager>();
        services.AddScoped<IUserManager, UserManager>();
        services.AddScoped<IVocabularyManager, VocabularyManager>();
        services.AddSingleton<IApiUsageStatisticsManager, ApiUsageStatisticsManager>();
        services.AddSingleton<SemaphoreLimitManager>();

        // Add repositories
        services.AddScoped<IApiUsageStatisticsRepository, ApiUsageStatisticsRepository>();
        services.AddScoped<IAreaRepository, AreaRepository>();
        services.AddScoped<IDataProviderRepository, DataProviderRepository>();
        services.AddScoped<IDatasetRepository, DatasetRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IProcessedChecklistRepository, ProcessedChecklistRepository>();
        services.AddScoped<IUserObservationRepository, UserObservationRepository>();
        services.AddScoped<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
        services.AddScoped<IProcessedLocationRepository, ProcessedLocationRepository>();
        services.AddScoped<IProcessedObservationRepository, ProcessedObservationRepository>();
        services.AddScoped<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();
        services.AddScoped<IProcessedTaxonRepository, ProcessedTaxonRepository>();
        services.AddScoped<IProcessInfoRepository, ProcessInfoRepository>();
        services.AddScoped<IProtectedLogRepository, ProtectedLogRepository>();
        services.AddScoped<ITaxonRepository, TaxonRepository>();
        services.AddScoped<IVocabularyRepository, VocabularyRepository>();
        services.AddScoped<IProjectInfoRepository, ProjectInfoRepository>();
        services.AddScoped<ITaxonListRepository, TaxonListRepository>();
        services.AddScoped<IUserExportRepository, UserExportRepository>();

        // Add services
        services.AddSingleton<IBlobStorageService, BlobStorageService>();
        services.AddSingleton<ICryptoService, CryptoService>();
        services.AddSingleton<IDevOpsService, DevOpsService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IHttpClientService, HttpClientService>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IArtportalenApiService, ArtportalenApiService>();

        // Add Utilites
        services.AddSingleton<ISearchFilterUtility, SearchFilterUtility>();
        services.AddScoped<IGeneralizationResolver, GeneralizationResolver>();

        // Add Validators
        services.AddScoped<IInputValidator, InputValidator>();

        // Add writers
        services.AddScoped<IDwcArchiveFileWriter, DwcArchiveFileWriter>();
        services.AddScoped<IDwcArchiveFileWriterCoordinator, DwcArchiveFileWriterCoordinator>();
        services.AddScoped<IDwcArchiveEventCsvWriter, DwcArchiveEventCsvWriter>();
        services.AddScoped<IDwcArchiveEventFileWriter, DwcArchiveEventFileWriter>();
        services.AddScoped<IDwcArchiveOccurrenceCsvWriter, DwcArchiveOccurrenceCsvWriter>();
        services.AddScoped<IExtendedMeasurementOrFactCsvWriter, ExtendedMeasurementOrFactCsvWriter>();
        services.AddScoped<ISimpleMultimediaCsvWriter, SimpleMultimediaCsvWriter>();
        services.AddScoped<ICsvFileWriter, CsvFileWriter>();
        services.AddScoped<IExcelFileWriter, ExcelFileWriter>();
        services.AddScoped<IGeoJsonFileWriter, GeoJsonFileWriter>();

        // Helpers, static data => single instance 
        services.AddSingleton<IVocabularyValueResolver, VocabularyValueResolver>();

        return services;
    }    
}