using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Core.Models.DOI;
using SOS.Core.Models.Observations;
using SOS.Core.Repositories;
using SOS.Core.Services;
using SOS.Core.Tests.TestDataFactories;
using Xunit;

namespace SOS.Core.Tests.Services.DoiServiceTests
{
    [Collection("MongoDbIntegrationTests")]
    public class CalculateHashForObservationsTests
    {
        private const string MongoDbConnectionString = "mongodb://localhost";
        private const string DatabaseName = "sos-local";

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestCalculateHashForAllProcessedObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Drop MongoDb collections
            //-----------------------------------------------------------------------------------------------------------
            const int nrObservations = 10;
            MongoDbContext observationsDbContext = new MongoDbContext(MongoDbConnectionString, DatabaseName, Constants.ObservationCollectionName);
            var observationRepository = new VersionedObservationRepository<ProcessedDwcObservation>(observationsDbContext);
            await observationRepository.DropObservationCollectionAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Add observations
            //-----------------------------------------------------------------------------------------------------------
            var speciesObservations = ProcessedDwcObservationTestDataFactory.CreateRandomObservations(nrObservations);
            await observationRepository.InsertDocumentsAsync(speciesObservations);

            //-----------------------------------------------------------------------------------------------------------
            // Act, Assert - Calculate Hash
            //-----------------------------------------------------------------------------------------------------------
            var hash = await observationRepository.CalculateHashForAllObservations();
            var hashAfterNoChanges = await observationRepository.CalculateHashForAllObservations();
            hash.Should().Be(hashAfterNoChanges, "because there have been no changes");

            //-----------------------------------------------------------------------------------------------------------
            // Act, Assert - Change one observation and calculate hash again
            //-----------------------------------------------------------------------------------------------------------
            var modifiedSpeciesObservation = speciesObservations[0];
            var nameBeforeModified = modifiedSpeciesObservation.RecordedBy;
            modifiedSpeciesObservation.RecordedBy = "Art Vandelay";
            await observationRepository.InsertDocumentAsync(modifiedSpeciesObservation);
            var hashAfterChanges = await observationRepository.CalculateHashForAllObservations();
            hashAfterChanges.Should().NotBe(hash, "because changes have been made");

            //-----------------------------------------------------------------------------------------------------------
            // Act, Assert - Change back to original name value and calculate hash
            //-----------------------------------------------------------------------------------------------------------
            modifiedSpeciesObservation.RecordedBy = nameBeforeModified;
            await observationRepository.InsertDocumentAsync(modifiedSpeciesObservation);
            var hashAfterReset = await observationRepository.CalculateHashForAllObservations();
            hashAfterReset.Should().Be(hash, "because the changed name has been restored");
        }

    }
}
