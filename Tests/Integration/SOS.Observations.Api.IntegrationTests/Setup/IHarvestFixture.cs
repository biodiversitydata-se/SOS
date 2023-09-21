using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Observations.Api.IntegrationTests.Setup;
public interface IHarvestFixture
{
    ArtportalenChecklistVerbatimRepository ArtportalenChecklistVerbatimRepository { get; set; }
    IArtportalenVerbatimRepository ArtportalenVerbatimRepository { get; set; }
    DwcCollectionRepository GetDwcCollectionRepository(DataProvider dataProvider);
}