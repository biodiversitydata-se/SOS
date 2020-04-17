using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Jobs;
using SOS.Lib.Jobs.Import;
using Xunit;

namespace SOS.Import.UnitTests.Managers
{
    public class ObservationsHarvestJobTests
    {
        private readonly Mock<ITaxonHarvestJob> _taxonHarvestJobMock;
        private readonly Mock<IFieldMappingImportJob> _fieldMappingImportJobMock;
        private readonly Mock<IArtportalenHarvestJob> _artportalenHarvestJobMock;
        private readonly Mock<IClamPortalHarvestJob> _clamPortalHarvestJobMock;
        private readonly Mock<IKulHarvestJob> _kulHarvestJobMock;
        private readonly Mock<INorsHarvestJob> _norsHarvestJobMock;
        private readonly Mock<ISersHarvestJob> _sersHarvestJobMock;
        private readonly Mock<ILogger<ObservationsHarvestJob>> _loggerMock;

        private ObservationsHarvestJob TestObject => new ObservationsHarvestJob(
            _taxonHarvestJobMock.Object,
            _fieldMappingImportJobMock.Object,
            _artportalenHarvestJobMock.Object,
            _clamPortalHarvestJobMock.Object,
            _kulHarvestJobMock.Object,
            _norsHarvestJobMock.Object,
            _sersHarvestJobMock.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public ObservationsHarvestJobTests()
        {
            _taxonHarvestJobMock = new Mock<ITaxonHarvestJob>();
            _fieldMappingImportJobMock = new Mock<IFieldMappingImportJob>();
            _artportalenHarvestJobMock = new Mock<IArtportalenHarvestJob>();
            _clamPortalHarvestJobMock = new Mock<IClamPortalHarvestJob>();
            _kulHarvestJobMock = new Mock<IKulHarvestJob>();
            _loggerMock = new Mock<ILogger<ObservationsHarvestJob>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new ObservationsHarvestJob(
               null,
               _fieldMappingImportJobMock.Object,
               _artportalenHarvestJobMock.Object,
               _clamPortalHarvestJobMock.Object,
               _kulHarvestJobMock.Object,
               _norsHarvestJobMock.Object,
               _sersHarvestJobMock.Object,
               _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonHarvestJob");

            create = () => new ObservationsHarvestJob(
                _taxonHarvestJobMock.Object,
                null,
                _artportalenHarvestJobMock.Object,
                _clamPortalHarvestJobMock.Object,
                _kulHarvestJobMock.Object,
                _norsHarvestJobMock.Object,
                _sersHarvestJobMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fieldMappingImportJob");

            create = () => new ObservationsHarvestJob(
                _taxonHarvestJobMock.Object,
                _fieldMappingImportJobMock.Object,
               null,
                _clamPortalHarvestJobMock.Object,
                _kulHarvestJobMock.Object,
                _norsHarvestJobMock.Object,
                _sersHarvestJobMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("artportalenHarvestJob");

            create = () => new ObservationsHarvestJob(
                _taxonHarvestJobMock.Object,
                _fieldMappingImportJobMock.Object,
                _artportalenHarvestJobMock.Object,
                null,
                _kulHarvestJobMock.Object,
                _norsHarvestJobMock.Object,
                _sersHarvestJobMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamPortalHarvestJob");

            create = () => new ObservationsHarvestJob(
                _taxonHarvestJobMock.Object,
                _fieldMappingImportJobMock.Object,
                _artportalenHarvestJobMock.Object,
                _clamPortalHarvestJobMock.Object,
                null,
                _norsHarvestJobMock.Object,
                _sersHarvestJobMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("kulHarvestJob");

            create = () => new ObservationsHarvestJob(
                _taxonHarvestJobMock.Object,
                _fieldMappingImportJobMock.Object,
                _artportalenHarvestJobMock.Object,
                _clamPortalHarvestJobMock.Object,
                _kulHarvestJobMock.Object,
               null,
                _sersHarvestJobMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("norsHarvestJob");

            create = () => new ObservationsHarvestJob(
                _taxonHarvestJobMock.Object,
                _fieldMappingImportJobMock.Object,
                _artportalenHarvestJobMock.Object,
                _clamPortalHarvestJobMock.Object,
                _kulHarvestJobMock.Object,
                _norsHarvestJobMock.Object,
               null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sersHarvestJob");

            create = () => new ObservationsHarvestJob(
                _taxonHarvestJobMock.Object,
                _fieldMappingImportJobMock.Object,
                _artportalenHarvestJobMock.Object,
                _clamPortalHarvestJobMock.Object,
                _kulHarvestJobMock.Object,
                _norsHarvestJobMock.Object,
                _sersHarvestJobMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Run harvest job successfully
        /// </summary>
        /// <returns></returns>
       /* [Fact]
        public async Task RunAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _artportalenHarvestJobMock.Setup(ts => ts.RunAsync(JobCancellationToken.Null))
                .ReturnsAsync(true);

            _taxonHarvestJobMock.Setup(ts => ts.RunAsync())
                .ReturnsAsync(true);

            _fieldMappingImportJobMock.Setup(ts => ts.RunAsync())
                .ReturnsAsync(true);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.RunAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }*/
    }
}
