using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Lib.Database;
using SOS.Lib.Models.Shared;
using Microsoft.Extensions.Logging;

namespace SOS.IntegrationTests.Setup.LiveDbFixtures;
public class HarvestFixture
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

        var verbatimDbConfiguration = GetVerbatimMongoDbConfiguration();
        var importClient = new VerbatimClient(
            verbatimDbConfiguration.GetMongoDbSettings(),
            verbatimDbConfiguration.DatabaseName,
            verbatimDbConfiguration.ReadBatchSize,
            verbatimDbConfiguration.WriteBatchSize);
        serviceCollection.AddSingleton<IVerbatimClient>(importClient);
        serviceCollection.AddSingleton<IArtportalenVerbatimRepository, ArtportalenVerbatimRepository>();
        serviceCollection.AddSingleton<ArtportalenChecklistVerbatimRepository>();
        serviceCollection.AddSingleton<HarvestFixture>();

        return serviceCollection;
    }

    private static MongoDbConfiguration GetVerbatimMongoDbConfiguration()
    {
        // return ST settings
        return new MongoDbConfiguration()
        {
            Hosts = new MongoDbServer[] { new MongoDbServer { Name = "artmongo2-1-test.artdata.slu.se", Port = 27017 } },
            DatabaseName = "sos-harvest-st",
            AuthenticationDb = "admin",
            ReadBatchSize = 10000,
            WriteBatchSize = 10000,
            UserName = "user",
            Password = "password"
        };
    }

    public DwcCollectionRepository GetDwcCollectionRepository(DataProvider dataProvider)
    {
        return new DwcCollectionRepository(dataProvider,
            _verbatimClient,
            new NullLogger<ILogger>());
    }
}
