using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Repositories.Source.Artportalen;
using SOS.Import.Services.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Source.Artportalen
{
    /// <summary>
    /// Meta data repository tests
    /// </summary>
    public class AreaRepositoryTests
    {
        private readonly Mock<IArtportalenDataService> _artportalenDataServiceMock;
        private readonly Mock<ILogger<AreaRepository>> _loggerMock;

        private AreaRepository TestObject => new AreaRepository(
            _artportalenDataServiceMock.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public AreaRepositoryTests()
        {
            _artportalenDataServiceMock = new Mock<IArtportalenDataService>();
            _loggerMock = new Mock<ILogger<AreaRepository>>();
        }

        /// <summary>
        /// Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new AreaRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("artportalenDataService");

            create = () => new AreaRepository(
                _artportalenDataServiceMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Test get areas success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncSuccess()
        {
            var areas = new[]
            {
               new AreaEntity{ Id = 1},
               new AreaEntity{ Id = 2}
            };

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<AreaEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(areas);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get areas fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<AreaEntity>(It.IsAny<string>(), null))
               .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }

        /// <summary>
        /// Test get areas success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAreasExceptGeometryFieldAsyncSuccess()
        {
            var areas = new[]
            {
               new AreaEntity{ Id = 1},
               new AreaEntity{ Id = 2}
            };

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<AreaEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(areas);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetAreasExceptGeometryFieldAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get areas fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAreasExceptGeometryFieldAsyncxception()
        {
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<AreaEntity>(It.IsAny<string>(), null))
               .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetAreasExceptGeometryFieldAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
    }
}
