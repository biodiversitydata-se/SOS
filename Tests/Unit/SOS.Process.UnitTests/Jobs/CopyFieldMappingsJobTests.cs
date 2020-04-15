using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Jobs;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Jobs
{
    /// <summary>
    /// Tests for activate instance job
    /// </summary>
    public class CopyFieldMappingsJobTests
    {
        private readonly Mock<IFieldMappingVerbatimRepository> _fieldMappingVerbatimRepositoryMock;
        private readonly Mock<IProcessedFieldMappingRepository> _processedFieldMappingRepositoryMock;
        private readonly Mock<IProcessInfoRepository> _processInfoRepositoryMock;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepository;
        private readonly Mock<ILogger<CopyFieldMappingsJob>> _loggerMock;

        private CopyFieldMappingsJob TestObject => new CopyFieldMappingsJob(
            _fieldMappingVerbatimRepositoryMock.Object,
            _processedFieldMappingRepositoryMock.Object,
            _harvestInfoRepository.Object,
            _processInfoRepositoryMock.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public CopyFieldMappingsJobTests()
        {
            _fieldMappingVerbatimRepositoryMock = new Mock<IFieldMappingVerbatimRepository>();
            _processedFieldMappingRepositoryMock = new Mock<IProcessedFieldMappingRepository>();
            _processInfoRepositoryMock = new Mock<IProcessInfoRepository>();
            _harvestInfoRepository = new Mock<IHarvestInfoRepository>();
            _loggerMock = new Mock<ILogger<CopyFieldMappingsJob>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new CopyFieldMappingsJob(
                null,
                _processedFieldMappingRepositoryMock.Object,
                _harvestInfoRepository.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fieldMappingVerbatimRepository");

            create = () => new CopyFieldMappingsJob(
                _fieldMappingVerbatimRepositoryMock.Object,
                null,
                _harvestInfoRepository.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedFieldMappingRepository");

            create = () => new CopyFieldMappingsJob(
                _fieldMappingVerbatimRepositoryMock.Object,
                _processedFieldMappingRepositoryMock.Object,
                null,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

            create = () => new CopyFieldMappingsJob(
                _fieldMappingVerbatimRepositoryMock.Object,
                _processedFieldMappingRepositoryMock.Object,
                _harvestInfoRepository.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processInfoRepository");

            create = () => new CopyFieldMappingsJob(
                _fieldMappingVerbatimRepositoryMock.Object,
                _processedFieldMappingRepositoryMock.Object,
                _harvestInfoRepository.Object,
                _processInfoRepositoryMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful test of processing
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _fieldMappingVerbatimRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<FieldMapping>
                {
                    new FieldMapping { Id = FieldMappingFieldId.Activity, Values = new List<FieldMappingValue>(), ExternalSystemsMapping = new List<ExternalSystemMapping>()},
                });

            _processedFieldMappingRepositoryMock.Setup(r => r.DeleteCollectionAsync())
                .ReturnsAsync(true);

            _processedFieldMappingRepositoryMock.Setup(r => r.AddManyAsync(It.IsAny<IEnumerable<FieldMapping>>()))
                .ReturnsAsync(true);

            _harvestInfoRepository.Setup(r => r.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HarvestInfo("ID", DataSet.Taxa, DateTime.Now){ Status = RunStatus.Success});

            _processInfoRepositoryMock.Setup(r => r.VerifyCollectionAsync());

            _processInfoRepositoryMock.Setup(r => r.AddOrUpdateAsync(It.IsAny<ProcessInfo>()));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.RunAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Test processing fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            _fieldMappingVerbatimRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<FieldMapping>
                {
                    new FieldMapping { Id = FieldMappingFieldId.Activity, Values = new List<FieldMappingValue>(), ExternalSystemsMapping = new List<ExternalSystemMapping>()},
                });

            _processedFieldMappingRepositoryMock.Setup(r => r.DeleteCollectionAsync())
                .ReturnsAsync(false);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.RunAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        /// Test processing exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _fieldMappingVerbatimRepositoryMock.Setup(r => r.GetAllAsync())
               .Throws<Exception>();
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.RunAsync(); };
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            await act.Should().ThrowAsync<Exception>();
        }
    }
}