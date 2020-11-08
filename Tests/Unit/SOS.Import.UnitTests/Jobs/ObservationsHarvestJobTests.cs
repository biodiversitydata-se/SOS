using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Jobs;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.UnitTests.Managers
{
    public class ObservationsHarvestJobTests
    {
        private readonly Mock<IVocabulariesImportJob> _vocabularyImportJobMock;
        private readonly Mock<IArtportalenHarvestJob> _artportalenHarvestJobMock;
        private readonly Mock<IClamPortalHarvestJob> _clamPortalHarvestJobMock;
        private readonly Mock<IKulHarvestJob> _kulHarvestJobMock;
        private readonly Mock<IMvmHarvestJob> _mvmHarvestJobMock;
        private readonly Mock<INorsHarvestJob> _norsHarvestJobMock;
        private readonly Mock<ISersHarvestJob> _sersHarvestJobMock;
        private readonly Mock<ISharkHarvestJob> _sharkHarvestJobMock;
        private readonly Mock<IVirtualHerbariumHarvestJob> _virtualHerbariumHarvestJobMock;
        private readonly Mock<ILogger<ObservationsHarvestJob>> _loggerMock;

        //private ObservationsHarvestJob TestObject => new ObservationsHarvestJob(
        //    _taxonHarvestJobMock.Object,
        //    _fieldMappingImportJobMock.Object,
        //    _artportalenHarvestJobMock.Object,
        //    _clamPortalHarvestJobMock.Object,
        //    _kulHarvestJobMock.Object,
        //    _mvmHarvestJobMock.Object,
        //    _norsHarvestJobMock.Object,
        //    _sersHarvestJobMock.Object,
        //    _sharkHarvestJobMock.Object,
        //    _virtualHerbariumHarvestJobMock.Object,
        //    _loggerMock.Object);

        /// <summary>
        ///     Constructor
        /// </summary>
        public ObservationsHarvestJobTests()
        {
          _vocabularyImportJobMock = new Mock<IVocabulariesImportJob>();
            _artportalenHarvestJobMock = new Mock<IArtportalenHarvestJob>();
            _clamPortalHarvestJobMock = new Mock<IClamPortalHarvestJob>();
            _kulHarvestJobMock = new Mock<IKulHarvestJob>();
            _mvmHarvestJobMock = new Mock<IMvmHarvestJob>();
            _norsHarvestJobMock = new Mock<INorsHarvestJob>();
            _sersHarvestJobMock = new Mock<ISersHarvestJob>();
            _sharkHarvestJobMock = new Mock<ISharkHarvestJob>();
            _virtualHerbariumHarvestJobMock = new Mock<IVirtualHerbariumHarvestJob>();
            _loggerMock = new Mock<ILogger<ObservationsHarvestJob>>();
        }

        ///// <summary>
        ///// Test constructor
        ///// </summary>
        //[Fact]
        //public void ConstructorTest()
        //{
        //    TestObject.Should().NotBeNull();

        //    Action create = () => new ObservationsHarvestJob(
        //       null,
        //       _fieldMappingImportJobMock.Object,
        //       _artportalenHarvestJobMock.Object,
        //       _clamPortalHarvestJobMock.Object,
        //       _kulHarvestJobMock.Object,
        //       _mvmHarvestJobMock.Object,
        //       _norsHarvestJobMock.Object,
        //       _sersHarvestJobMock.Object,
        //       _sharkHarvestJobMock.Object,
        //       _virtualHerbariumHarvestJobMock.Object,
        //       _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonHarvestJob");

        //    create = () => new ObservationsHarvestJob(
        //        _taxonHarvestJobMock.Object,
        //        null,
        //        _artportalenHarvestJobMock.Object,
        //        _clamPortalHarvestJobMock.Object,
        //        _kulHarvestJobMock.Object,
        //        _mvmHarvestJobMock.Object,
        //        _norsHarvestJobMock.Object,
        //        _sersHarvestJobMock.Object,
        //        _sharkHarvestJobMock.Object,
        //        _virtualHerbariumHarvestJobMock.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fieldMappingImportJob");

        //    create = () => new ObservationsHarvestJob(
        //        _taxonHarvestJobMock.Object,
        //        _fieldMappingImportJobMock.Object,
        //       null,
        //        _clamPortalHarvestJobMock.Object,
        //        _kulHarvestJobMock.Object,
        //        _mvmHarvestJobMock.Object,
        //        _norsHarvestJobMock.Object,
        //        _sersHarvestJobMock.Object,
        //        _sharkHarvestJobMock.Object,
        //        _virtualHerbariumHarvestJobMock.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("artportalenHarvestJob");

        //    create = () => new ObservationsHarvestJob(
        //        _taxonHarvestJobMock.Object,
        //        _fieldMappingImportJobMock.Object,
        //        _artportalenHarvestJobMock.Object,
        //        null,
        //        _kulHarvestJobMock.Object,
        //        _mvmHarvestJobMock.Object,
        //        _norsHarvestJobMock.Object,
        //        _sersHarvestJobMock.Object,
        //        _sharkHarvestJobMock.Object,
        //        _virtualHerbariumHarvestJobMock.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamPortalHarvestJob");

        //    create = () => new ObservationsHarvestJob(
        //        _taxonHarvestJobMock.Object,
        //        _fieldMappingImportJobMock.Object,
        //        _artportalenHarvestJobMock.Object,
        //        _clamPortalHarvestJobMock.Object,
        //        null,
        //        _mvmHarvestJobMock.Object,
        //        _norsHarvestJobMock.Object,
        //        _sersHarvestJobMock.Object,
        //        _sharkHarvestJobMock.Object,
        //        _virtualHerbariumHarvestJobMock.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("kulHarvestJob");

        //    create = () => new ObservationsHarvestJob(
        //        _taxonHarvestJobMock.Object,
        //        _fieldMappingImportJobMock.Object,
        //        _artportalenHarvestJobMock.Object,
        //        _clamPortalHarvestJobMock.Object,
        //        _kulHarvestJobMock.Object,
        //        null,
        //        _norsHarvestJobMock.Object,
        //        _sersHarvestJobMock.Object,
        //        _sharkHarvestJobMock.Object,
        //        _virtualHerbariumHarvestJobMock.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mvmHarvestJob");

        //    create = () => new ObservationsHarvestJob(
        //        _taxonHarvestJobMock.Object,
        //        _fieldMappingImportJobMock.Object,
        //        _artportalenHarvestJobMock.Object,
        //        _clamPortalHarvestJobMock.Object,
        //        _kulHarvestJobMock.Object,
        //        _mvmHarvestJobMock.Object,
        //       null,
        //        _sersHarvestJobMock.Object,
        //        _sharkHarvestJobMock.Object,
        //        _virtualHerbariumHarvestJobMock.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("norsHarvestJob");

        //    create = () => new ObservationsHarvestJob(
        //        _taxonHarvestJobMock.Object,
        //        _fieldMappingImportJobMock.Object,
        //        _artportalenHarvestJobMock.Object,
        //        _clamPortalHarvestJobMock.Object,
        //        _kulHarvestJobMock.Object,
        //        _mvmHarvestJobMock.Object,
        //        _norsHarvestJobMock.Object,
        //       null,
        //        _sharkHarvestJobMock.Object,
        //        _virtualHerbariumHarvestJobMock.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sersHarvestJob");

        //    create = () => new ObservationsHarvestJob(
        //        _taxonHarvestJobMock.Object,
        //        _fieldMappingImportJobMock.Object,
        //        _artportalenHarvestJobMock.Object,
        //        _clamPortalHarvestJobMock.Object,
        //        _kulHarvestJobMock.Object,
        //        _mvmHarvestJobMock.Object,
        //        _norsHarvestJobMock.Object,
        //        _sersHarvestJobMock.Object,
        //       null,
        //        _virtualHerbariumHarvestJobMock.Object,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sharkHarvestJob");

        //    create = () => new ObservationsHarvestJob(
        //        _taxonHarvestJobMock.Object,
        //        _fieldMappingImportJobMock.Object,
        //        _artportalenHarvestJobMock.Object,
        //        _clamPortalHarvestJobMock.Object,
        //        _kulHarvestJobMock.Object,
        //        _mvmHarvestJobMock.Object,
        //        _norsHarvestJobMock.Object,
        //        _sersHarvestJobMock.Object,
        //        _sharkHarvestJobMock.Object,
        //        null,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("virtualHerbariumHarvestJob");

        //    create = () => new ObservationsHarvestJob(
        //        _taxonHarvestJobMock.Object,
        //        _fieldMappingImportJobMock.Object,
        //        _artportalenHarvestJobMock.Object,
        //        _clamPortalHarvestJobMock.Object,
        //        _kulHarvestJobMock.Object,
        //        _mvmHarvestJobMock.Object,
        //        _norsHarvestJobMock.Object,
        //        _sersHarvestJobMock.Object,
        //        _sharkHarvestJobMock.Object,
        //        _virtualHerbariumHarvestJobMock.Object,
        //        null);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        //}

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