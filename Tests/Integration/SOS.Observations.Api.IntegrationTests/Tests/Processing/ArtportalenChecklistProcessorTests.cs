using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.Processing;

[Collection(TestCollection.Name)]
public class ArtportalenChecklistProcessorTests : TestBase
{
    public ArtportalenChecklistProcessorTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public void Test_process_verbatim_checklists()
    {
        // Arrange
        var verbatimChecklists = Builder<ArtportalenChecklistVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedChecklists()
            .Build();

        // Act
        var checklists = ProcessFixture.ProcessChecklists(verbatimChecklists);

        // Assert
        checklists.Count.Should().Be(100);
    }
}