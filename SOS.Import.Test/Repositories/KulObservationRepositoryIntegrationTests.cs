using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using KulService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Repositories.Source.Kul;
using SOS.Lib.Configuration.Import;
using Xunit;
using Xunit.Abstractions;

namespace SOS.Import.Test.Repositories
{
    public class KulObservationRepositoryIntegrationTests
    {
        [Fact]
        [Trait("Category","Integration")]
        public async Task TestGetObservationsUsingRepository()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var kulRepository = new KulObservationRepository(
                new Mock<ILogger<KulObservationRepository>>().Object, 
                importConfiguration.KulServiceConfiguration);
            var changedFrom = new DateTime(2015,1,1);
            var changedTo = changedFrom.AddYears(1);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = (await kulRepository.GetAsync(changedFrom, changedTo)).ToArray();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().NotBeNull();
            result.Count().Should().BeGreaterThan(10000, "because 2019-11-04 there were 12200 sightings");
        }

        private ImportConfiguration GetImportConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<KulObservationRepositoryIntegrationTests>()
                .Build();

            ImportConfiguration importConfiguration = config.GetSection(typeof(ImportConfiguration).Name).Get<ImportConfiguration>();
            return importConfiguration;
        }
    }
}