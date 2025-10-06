using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SOS.Administration.Api.Managers;
using SOS.Administration.Api.Managers.Interfaces;
using SOS.Harvest.Factories.Vocabularies;
using SOS.Harvest.Harvesters;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Helpers;
using SOS.Harvest.Helpers.Interfaces;
using SOS.Harvest.Services;
using SOS.Harvest.Services.Interfaces;
using SOS.Harvest.Services.Taxon;
using SOS.Harvest.Services.Taxon.Interfaces;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Security;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;

namespace SOS.Administration.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencyInjectionServices(this IServiceCollection services)
    {        
        services.AddSingleton(Settings.SosApiConfiguration);        
        services.AddSingleton(Settings.ImportConfiguration.GeoRegionApiConfiguration);
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IAuthorizationProvider, CurrentUserAuthorization>();
        services.AddScoped<IVocabularyHarvester, VocabularyHarvester>();
        services.AddScoped<ITaxonServiceProxy, TaxonServiceProxy>();

        // Add managers
        services.AddScoped<IReportManager, ReportManager>();
        services.AddScoped<ICacheManager, CacheManager>();
        services.AddScoped<DiagnosticsManager>();
        services.AddScoped<IApiUsageStatisticsManager, ApiUsageStatisticsManager>();
        services.AddScoped<IDataProviderManager, DataProviderManager>();
        services.AddScoped<IIptManager, IptManager>();

        // Add repositories
        services.AddScoped<IApiUsageStatisticsRepository, ApiUsageStatisticsRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IVocabulariesDiffHelper, VocabulariesDiffHelper>();
        services.AddScoped<IDataProviderRepository, DataProviderRepository>();
        services.AddScoped<ITaxonRepository, TaxonRepository>();
        services.AddScoped<IProcessInfoRepository, ProcessInfoRepository>();
        services.AddScoped<IVocabularyRepository, VocabularyRepository>();
        services.AddScoped<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
        services.AddScoped<IProjectInfoRepository, ProjectInfoRepository>();
        services.AddScoped<ITaxonListRepository, TaxonListRepository>();
        services.AddScoped<IAreaRepository, AreaRepository>();
        services.AddScoped<Harvest.Repositories.Source.Artportalen.Interfaces.IMetadataRepository, Harvest.Repositories.Source.Artportalen.MetadataRepository>();

        // Add services
        services.AddScoped<ITaxonService, TaxonService>();
        services.AddSingleton<IApplicationInsightsService, ApplicationInsightsService>();
        services.AddScoped<IFileDownloadService, FileDownloadService>();
        services.AddScoped<IHttpClientService, HttpClientService>();
        services.AddScoped<IArtportalenDataService, ArtportalenDataService>();
        services.AddScoped<IApiManagementUserService, ApiManagementUserService>();
        services.AddScoped<IUserService, UserService>();

        // Add Vocabulary Factories
        services.AddScoped<AccessRightsVocabularyFactory>();
        services.AddScoped<AccessRightsVocabularyFactory>();
        services.AddScoped<ActivityVocabularyFactory>();
        services.AddScoped<AreaTypeVocabularyFactory>();
        services.AddScoped<BasisOfRecordVocabularyFactory>();
        services.AddScoped<BehaviorVocabularyFactory>();
        services.AddScoped<BiotopeVocabularyFactory>();
        services.AddScoped<BirdNestActivityVocabularyFactory>();
        services.AddScoped<ContinentVocabularyFactory>();
        services.AddScoped<CountryVocabularyFactory>();
        services.AddScoped<DeterminationMethodVocabularyFactory>();
        services.AddScoped<DiscoveryMethodVocabularyFactory>();
        services.AddScoped<EstablishmentMeansVocabularyFactory>();
        services.AddScoped<InstitutionVocabularyFactory>();
        services.AddScoped<LifeStageVocabularyFactory>();
        services.AddScoped<OccurrenceStatusVocabularyFactory>();
        services.AddScoped<ReproductiveConditionVocabularyFactory>();
        services.AddScoped<SensitivityCategoryVocabularyFactory>();
        services.AddScoped<SexVocabularyFactory>();
        services.AddScoped<SubstrateVocabularyFactory>();
        services.AddScoped<TypeVocabularyFactory>();
        services.AddScoped<UnitVocabularyFactory>();
        services.AddScoped<VerificationStatusVocabularyFactory>();
        services.AddScoped<TaxonCategoryVocabularyFactory>();

        // Verbatim Mongo Db
        var verbatimSettings = Settings.VerbatimDbConfiguration.GetMongoDbSettings();
        services.AddSingleton<IVerbatimClient>(new VerbatimClient(verbatimSettings, Settings.VerbatimDbConfiguration.DatabaseName,
            Settings.VerbatimDbConfiguration.ReadBatchSize, Settings.VerbatimDbConfiguration.WriteBatchSize));

        // Processed Mongo Db
        var processedDbConfiguration = Settings.ProcessDbConfiguration;
        var processedSettings = processedDbConfiguration.GetMongoDbSettings();
        services.AddSingleton<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
            processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));

        // Configuration
        services.AddSingleton(new ArtportalenConfiguration());
        services.AddSingleton(new ApiManagementServiceConfiguration());
        services.AddSingleton(new TaxonServiceConfiguration() { BaseAddress = "https://taxonapi.artdata.slu.se/darwincore/download?version=custom" });
        if (Settings.ApplicationInsightsConfiguration != null)
            services.AddSingleton(Settings.ApplicationInsightsConfiguration);
        else
            services.AddSingleton(new ApplicationInsightsConfiguration());
        services.AddSingleton(Settings.UserServiceConfiguration);
        
        // Caches
        services.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
        services.AddSingleton<ICache<int, ProjectInfo>, ProjectCache>();
        services.AddSingleton<IDataProviderCache, DataProviderCache>();
        services.AddSingleton<ICache<VocabularyId, Vocabulary>, VocabularyCache>();
        services.AddSingleton<IAreaCache, AreaCache>();
        services.AddSingleton<ICache<int, TaxonList>, TaxonListCache>();

        return services;
    }
}