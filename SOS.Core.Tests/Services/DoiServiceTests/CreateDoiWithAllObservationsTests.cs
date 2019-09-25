using System.Collections.Generic;
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
using SOS.Core.Tests.TestRepositories;
using Xunit;

namespace SOS.Core.Tests.Services.DoiServiceTests
{
    public class CreateDoiWithAllObservationsTests
    {
        private const string MongoUrl = "mongodb://localhost";
        private const string DatabaseName = "diff_sample";

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestCreateDoiWithAllProcessedObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Reset/Initiliaze MongoDb collections
            //-----------------------------------------------------------------------------------------------------------
            MongoDbContext observationsDbContext = new MongoDbContext(MongoUrl, DatabaseName, Constants.ObservationCollectionName);
            MongoDbContext doiDbContext = new MongoDbContext(MongoUrl, DatabaseName, Constants.DoiCollectionName);
            var observationRepository = new VersionedObservationRepository<ProcessedDwcObservation>(observationsDbContext);
            var doiRepository = new DoiRepository(doiDbContext);
            await doiRepository.DropDoiFileBucketAsync();
            await doiRepository.DropDoiCollectionAsync();
            var nrObservations = await observationRepository.GetTotalNumberOfObservationsAsync();
            if (nrObservations == 0) await InsertObservationsTestDataAsync(observationRepository);

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
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var doiInfo = await doiService.CreateDoiWithAllObservationsAsync(doiMetadata);
            var doiObservations = await doiService.GetDoiObservationsAsync(doiInfo.FileName); // Get observations from DOI stored in MongoDb.

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            doiObservations.Count().Should().Be((int)nrObservations);
        }

        private async Task InsertObservationsTestDataAsync(VersionedObservationRepository<ProcessedDwcObservation> observationRepository)
        {
            var speciesObservations = ProcessedDwcObservationTestRepository.CreateRandomObservations(10000);
            await observationRepository.InsertDocumentsAsync(speciesObservations);
        }
    }
}