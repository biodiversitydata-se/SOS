using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Harvest.Harvesters.AquaSupport.Nors;
using SOS.Harvest.Repositories.Source.Aqua.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Nors;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters.Observations;

public class NorsObservationHarvesterTests
{
    /// <summary>
    ///     Constructor
    /// </summary>
    public NorsObservationHarvesterTests()
    {
        _norsRepositoryMock = new Mock<INorsRepository>();
        _verbatimClient = new Mock<IVerbatimClient>();
        _norsServiceConfiguration = new NorsServiceConfiguration
        { MaxNumberOfSightingsHarvested = 1 };
        _loggerMock = new Mock<ILogger<NorsObservationHarvester>>();
    }

    private readonly Mock<INorsRepository> _norsRepositoryMock;
    private readonly Mock<IVerbatimClient> _verbatimClient;
    private readonly NorsServiceConfiguration _norsServiceConfiguration;
    private readonly Mock<ILogger<NorsObservationHarvester>> _loggerMock;

    private NorsObservationHarvester TestObject => new NorsObservationHarvester(
        _verbatimClient.Object,
        _norsRepositoryMock.Object,
        _norsServiceConfiguration,
        _loggerMock.Object);

    /// <summary>
    ///     Test aggregation fail
    /// </summary>
    /// <returns></returns>
    [Fact(Skip = "too slow for being a unit test. Todo - move to integration test.")]
    public async Task HarvestNorsAsyncFail()
    {
        // -----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
       /* _verbatimClient.Setup(cts => cts..GetAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Fail"));
       */
        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var result = await TestObject.HarvestObservationsAsync(new DataProvider(), JobCancellationToken.Null);
        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------

        result.Count.Should().Be(0);
    }

    /// <summary>
    ///     Make a successful nors harvest
    /// </summary>
    /// <returns></returns>
    [Fact(Skip = "too slow for being a unit test. Todo - move to integration test.")]
    public async Task HarvestNorsAsyncSuccess()
    {
        // -----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        /*_verbatimClient.Setup(cts => cts.GetAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
            .ReturnsAsync(new XDocument());
        */
        /*_norsRepositoryMock.Setup(tr => tr.DeleteCollectionAsync())
            .ReturnsAsync(true);
        _norsRepositoryMock.Setup(tr => tr.AddCollectionAsync())
            .ReturnsAsync(true);
        _norsRepositoryMock
            .Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<NorsObservationVerbatim>>()))
            .ReturnsAsync(true);
        */
        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var result = await TestObject.HarvestObservationsAsync(new DataProvider(), JobCancellationToken.Null);
        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------

        result.Status.Should().Be(RunStatus.Success);
    }
}