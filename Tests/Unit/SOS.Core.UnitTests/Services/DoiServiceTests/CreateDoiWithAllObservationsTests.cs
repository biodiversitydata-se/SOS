using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Driver.GridFS;
using SOS.Core.Models.DOI;
using SOS.Core.Models.Observations;
using SOS.Core.Repositories;
using SOS.Core.Services;
using SOS.Core.Tests.TestDataFactories;
using Xunit;

namespace SOS.Core.Tests.Services.DoiServiceTests
{
    [Collection("MongoDbIntegrationTests")]
    public class CreateDoiWithAllObservationsTests
    {
        private const string MongoDbConnectionString = "mongodb://localhost";
        private const string DatabaseName = "sos-local";

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestCreateDoiWithAllProcessedObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Drop MongoDb collections
            //-----------------------------------------------------------------------------------------------------------
            const int nrObservations = 10000;
            MongoDbContext observationsDbContext = new MongoDbContext(MongoDbConnectionString, DatabaseName, Constants.ObservationCollectionName);
            MongoDbContext doiDbContext = new MongoDbContext(MongoDbConnectionString, DatabaseName, Constants.DoiCollectionName);
            var observationRepository = new VersionedObservationRepository<ProcessedDwcObservation>(observationsDbContext);
            var doiRepository = new DoiRepository(doiDbContext);
            await observationRepository.DropObservationCollectionAsync();  
            await doiRepository.DropDoiFileBucketAsync();
            await doiRepository.DropDoiCollectionAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Add observations
            //-----------------------------------------------------------------------------------------------------------
            var speciesObservations = ProcessedDwcObservationTestDataFactory.CreateRandomObservations(nrObservations);
            await observationRepository.InsertDocumentsAsync(speciesObservations);

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - DOI data
            //-----------------------------------------------------------------------------------------------------------
            IDoiService doiService = new DoiService(observationRepository, doiRepository);
            DoiMetadata doiMetadata = new DoiMetadata
            {
                UserId = 3432,
                PersonName = "Art Vandelay",
                Description = "Creating a DOI for all observations"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Create DOI for all current observations
            //-----------------------------------------------------------------------------------------------------------
            var observationIdentifiers = observationRepository.GetAllObservationVersionIdentifiersEnumerable();
            var doiInfo = await doiService.CreateDoiAsync(observationIdentifiers, doiMetadata);


            //-----------------------------------------------------------------------------------------------------------
            // Act - Change one observation
            //-----------------------------------------------------------------------------------------------------------
            var modifiedSpeciesObservation = speciesObservations[0];
            var nameBeforeModified = modifiedSpeciesObservation.RecordedBy;
            modifiedSpeciesObservation.RecordedBy = "Art Vandelay";
            await observationRepository.InsertDocumentAsync(modifiedSpeciesObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert - Check that the changed observations is changed in database
            //-----------------------------------------------------------------------------------------------------------
            var modifiedSpeciesObsFromDb = await observationRepository.GetDocumentAsync(modifiedSpeciesObservation.DataProviderId,
                modifiedSpeciesObservation.CatalogNumber);
            modifiedSpeciesObsFromDb.Version.Should().Be(2, "this observation is updated");
            modifiedSpeciesObsFromDb.Current.RecordedBy.Should().Be("Art Vandelay");

            //-----------------------------------------------------------------------------------------------------------
            // Assert - Get all observations for the DOI. Check that the changed observation is restored to version 1.
            //-----------------------------------------------------------------------------------------------------------
            var doiObservations = await doiService.GetDoiObservationsAsync(doiInfo.FileName); // Get observations from DOI stored in MongoDb.
            doiObservations.Count().Should().Be(nrObservations);
            var restoredObservation = doiObservations.Single(x => x.RecordKey == modifiedSpeciesObservation.RecordKey);
            restoredObservation.RecordedBy.Should().Be(nameBeforeModified, "this should be the restored version 1");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            doiObservations.Count().Should().Be((int)nrObservations);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestCreateDoiWithHashForAllProcessedObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Drop MongoDb collections
            //-----------------------------------------------------------------------------------------------------------
            const int nrObservations = 10000;
            MongoDbContext observationsDbContext = new MongoDbContext(MongoDbConnectionString, DatabaseName, Constants.ObservationCollectionName);
            MongoDbContext doiDbContext = new MongoDbContext(MongoDbConnectionString, DatabaseName, Constants.DoiCollectionName);
            var observationRepository = new VersionedObservationRepository<ProcessedDwcObservation>(observationsDbContext);
            var doiRepository = new DoiRepository(doiDbContext);
            await observationRepository.DropObservationCollectionAsync();
            await doiRepository.DropDoiFileBucketAsync();
            await doiRepository.DropDoiCollectionAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Add observations
            //-----------------------------------------------------------------------------------------------------------
            var speciesObservations = ProcessedDwcObservationTestDataFactory.CreateRandomObservations(nrObservations);
            await observationRepository.InsertDocumentsAsync(speciesObservations);

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - DOI data
            //-----------------------------------------------------------------------------------------------------------
            IDoiService doiService = new DoiService(observationRepository, doiRepository);
            DoiMetadata doiMetadata = new DoiMetadata
            {
                UserId = 3432,
                PersonName = "Art Vandelay",
                Description = "Creating a DOI for all observations"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Create DOI for all current observations
            //-----------------------------------------------------------------------------------------------------------
            var observationVersionIdentifierSet = await observationRepository.CalculateHashForAllObservationsAndReturnIdentifiers();
            doiMetadata.Hash = observationVersionIdentifierSet.Hash;
            var doiInfo = await doiService.CreateDoiAsync(observationVersionIdentifierSet.ObservationVersionIdentifiers, doiMetadata);


            //-----------------------------------------------------------------------------------------------------------
            // Act - Change one observation
            //-----------------------------------------------------------------------------------------------------------
            var modifiedSpeciesObservation = speciesObservations[0];
            var nameBeforeModified = modifiedSpeciesObservation.RecordedBy;
            modifiedSpeciesObservation.RecordedBy = "Art Vandelay";
            await observationRepository.InsertDocumentAsync(modifiedSpeciesObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert - Check that the changed observations is changed in database
            //-----------------------------------------------------------------------------------------------------------
            var modifiedSpeciesObsFromDb = await observationRepository.GetDocumentAsync(modifiedSpeciesObservation.DataProviderId,
                modifiedSpeciesObservation.CatalogNumber);
            modifiedSpeciesObsFromDb.Version.Should().Be(2, "this observation is updated");
            modifiedSpeciesObsFromDb.Current.RecordedBy.Should().Be("Art Vandelay");

            //-----------------------------------------------------------------------------------------------------------
            // Assert - Get all observations for the DOI. Check that the changed observation is restored to version 1.
            //-----------------------------------------------------------------------------------------------------------
            IList<ProcessedDwcObservation> doiObservations = await doiService.GetDoiObservationsAsync(doiInfo.FileName); // Get observations from DOI stored in MongoDb.
            string calculatedHash = observationRepository.CalculateHash(doiObservations);
            calculatedHash.Should().Be(doiInfo.Metadata.Hash);
            doiObservations.Count().Should().Be(nrObservations);
            var restoredObservation = doiObservations.Single(x => x.RecordKey == modifiedSpeciesObservation.RecordKey);
            restoredObservation.RecordedBy.Should().Be(nameBeforeModified, "this should be the restored version 1");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            doiObservations.Count().Should().Be((int)nrObservations);
        }

    }
}