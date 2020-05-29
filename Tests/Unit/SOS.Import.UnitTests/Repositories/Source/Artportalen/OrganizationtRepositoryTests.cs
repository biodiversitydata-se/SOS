using System;
using System.Collections.Generic;
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
    ///     Project tests
    /// </summary>
    public class OrganizationRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public OrganizationRepositoryTests()
        {
            _artportalenDataServiceMock = new Mock<IArtportalenDataService>();
            _loggerMock = new Mock<ILogger<OrganizationRepository>>();
        }

        private readonly Mock<IArtportalenDataService> _artportalenDataServiceMock;
        private readonly Mock<ILogger<OrganizationRepository>> _loggerMock;

        private OrganizationRepository TestObject => new OrganizationRepository(
            _artportalenDataServiceMock.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new OrganizationRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("artportalenDataService");

            create = () => new OrganizationRepository(
                _artportalenDataServiceMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        ///     Test get projects fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<OrganizationEntity>(It.IsAny<string>(), null))
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
        ///     Test get projects success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncSuccess()
        {
            IEnumerable<OrganizationEntity> projects = new[]
            {
                new OrganizationEntity {Id = 1, Name = "Org 1"},
                new OrganizationEntity {Id = 2, Name = "Org 2"}
            };

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<OrganizationEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(projects);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }
    }
}