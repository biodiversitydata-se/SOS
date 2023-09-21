using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Lib.Models.Shared;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.DwC.Interfaces;
using SOS.Harvest.Harvesters.DwC;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Services.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.IO.DwcArchive;
using SOS.Lib.Configuration.Export;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Managers;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Processed;

namespace SOS.Observations.Api.IntegrationTests.Setup.ContainerDbFixtures;
public class HarvestFixture : IHarvestFixture
{
    private IVerbatimClient _verbatimClient;
    public IArtportalenVerbatimRepository ArtportalenVerbatimRepository { get; set; }
    public ArtportalenChecklistVerbatimRepository ArtportalenChecklistVerbatimRepository { get; set; }

    public HarvestFixture(IVerbatimClient verbatimClient,
        IArtportalenVerbatimRepository artportalenVerbatimRepository,
        ArtportalenChecklistVerbatimRepository artportalenChecklistVerbatimRepository)
    {
        _verbatimClient = verbatimClient;
        ArtportalenVerbatimRepository = artportalenVerbatimRepository;
        ArtportalenChecklistVerbatimRepository = artportalenChecklistVerbatimRepository;
    }

    public static ServiceCollection GetServiceCollection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        serviceCollection.AddSingleton<IFileDownloadService, FileDownloadService>();
        serviceCollection.AddSingleton<IHttpClientService, HttpClientService>();
        serviceCollection.AddSingleton(GetDwcaConfiguration());
        serviceCollection.AddSingleton(GetDwcaFilesCreationConfiguration());
        serviceCollection.AddSingleton<IDataProviderRepository, DataProviderRepository>();
        serviceCollection.AddSingleton<IDwcaObservationProcessor, DwcaObservationProcessor>();
        serviceCollection.AddSingleton<IDwcArchiveFileWriterCoordinator, DwcArchiveFileWriterCoordinator>();
        serviceCollection.AddSingleton<IDwcArchiveFileWriter, DwcArchiveFileWriter>();
        serviceCollection.AddSingleton<IDwcArchiveEventFileWriter, DwcArchiveEventFileWriter>();
        serviceCollection.AddSingleton<IDwcArchiveOccurrenceCsvWriter, DwcArchiveOccurrenceCsvWriter>();
        serviceCollection.AddSingleton<IExtendedMeasurementOrFactCsvWriter, ExtendedMeasurementOrFactCsvWriter>();
        serviceCollection.AddSingleton<ISimpleMultimediaCsvWriter, SimpleMultimediaCsvWriter>();
        serviceCollection.AddSingleton<IFileService, FileService>();
        serviceCollection.AddSingleton<IDwcArchiveEventCsvWriter, DwcArchiveEventCsvWriter>();
        serviceCollection.AddSingleton<IProcessManager, ProcessManager>();
        serviceCollection.AddSingleton<IValidationManager, ValidationManager>();
        serviceCollection.AddSingleton<IInvalidObservationRepository, InvalidObservationRepository>();
        serviceCollection.AddSingleton<IInvalidEventRepository, InvalidEventRepository>();
        serviceCollection.AddSingleton<IDiffusionManager, DiffusionManager>();

        serviceCollection.AddSingleton<IDwcObservationHarvester, DwcObservationHarvester>();
        serviceCollection.AddSingleton<IArtportalenVerbatimRepository, ArtportalenVerbatimRepository>();
        serviceCollection.AddSingleton<ArtportalenChecklistVerbatimRepository>();
        serviceCollection.AddSingleton<IHarvestFixture, HarvestFixture>();

        return serviceCollection;
    }

    private static DwcaFilesCreationConfiguration GetDwcaFilesCreationConfiguration()
    {
        return new DwcaFilesCreationConfiguration
        {
            CheckForIllegalCharacters = false,
            FolderPath = Path.GetTempPath(),
            IsEnabled = false
        };
    }

    private static DwcaConfiguration GetDwcaConfiguration()
    {
        return new DwcaConfiguration()
        {
            UseDwcaCollectionRepository = true,
            ImportPath = Path.GetTempPath(),
            BatchSize = 10000,
            ForceHarvestUnchangedDwca = true
        };
    }

    public DwcCollectionRepository GetDwcCollectionRepository(DataProvider dataProvider)
    {
        return new DwcCollectionRepository(dataProvider,
            _verbatimClient,
            new NullLogger<ILogger>());
    }
}
