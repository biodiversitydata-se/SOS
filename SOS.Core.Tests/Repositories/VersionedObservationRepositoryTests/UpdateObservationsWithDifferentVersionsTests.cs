using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Driver;
using SOS.Core.Models.Observations;
using SOS.Core.Models.Versioning;
using SOS.Core.Repositories;
using SOS.Core.Tests.TestRepositories;
using Xunit;

namespace SOS.Core.Tests.Repositories.VersionedObservationRepositoryTests
{
    public class UpdateObservationsWithDifferentVersionsTests
    {
        private const string MongoUrl = "mongodb://localhost";
        private const string DatabaseName = "diff_sample";
        private const string CollectionName = "observations";

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestAddAndUpdateOneObservationCreatingDifferentVersions()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Database connection, etc.
            //-----------------------------------------------------------------------------------------------------------
            MongoDbContext dbContext = new MongoDbContext(MongoUrl, DatabaseName, CollectionName);
            await dbContext.Mongodb.DropCollectionAsync(CollectionName);
            var observationRepository = new VersionedObservationRepository<ProcessedDwcObservation>(dbContext);

            
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Observation versions. One original version and 3 different updated versions.
            //-----------------------------------------------------------------------------------------------------------
            ProcessedDwcObservation originalObservation = ProcessedDwcObservationTestRepository.CreateRandomObservation();
            var observationVersion2 = (ProcessedDwcObservation)originalObservation.Clone();
            observationVersion2.RecordedBy = "Peter van Nostrand";
            var observationVersion3 = (ProcessedDwcObservation)observationVersion2.Clone();
            observationVersion3.CoordinateX = 54.234;
            var observationVersion4 = (ProcessedDwcObservation)observationVersion3.Clone();
            observationVersion4.RecordedBy = "Art Vandelay";

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Variables
            //-----------------------------------------------------------------------------------------------------------
            int dataProviderId = originalObservation.DataProviderId;
            string catalogNumber = originalObservation.CatalogNumber;


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------                                    
            await observationRepository.InsertDocumentAsync(originalObservation); // Version 1, First insert
            await observationRepository.InsertDocumentAsync(observationVersion2); // Version 2, Change [RecordedBy]
            await observationRepository.InsertDocumentAsync(observationVersion3); // Version 3, Change [CoordinateX]
            await observationRepository.DeleteDocumentAsync(dataProviderId, catalogNumber); // Version 4, Delete document
            await observationRepository.InsertDocumentAsync(observationVersion4); // Version 5, Change [RecordedBy]


            //-----------------------------------------------------------------------------------------------------------
            // Act - Restore versions
            //-----------------------------------------------------------------------------------------------------------
            var restoredVer5 = await observationRepository.RestoreDocumentAsync(dataProviderId, catalogNumber, 5);
            var restoredVer4 = await observationRepository.RestoreDocumentAsync(dataProviderId, catalogNumber, 4);
            var restoredVer3 = await observationRepository.RestoreDocumentAsync(dataProviderId, catalogNumber, 3);
            var restoredVer2 = await observationRepository.RestoreDocumentAsync(dataProviderId, catalogNumber, 2);
            var restoredVer1 = await observationRepository.RestoreDocumentAsync(dataProviderId, catalogNumber, 1);



            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            restoredVer5.Should().BeEquivalentTo(observationVersion4, "in version 5, the observationVersion4 was inserted");
            restoredVer4.Should().BeNull("the observation was deleted in version 4");
            restoredVer3.Should().BeEquivalentTo(observationVersion3, "in version 3, the observationVersion3 was inserted");
            restoredVer2.Should().BeEquivalentTo(observationVersion2, "in version 2, the observationVersion2 was inserted");
            restoredVer1.Should().BeEquivalentTo(originalObservation, "in the first version, the observationVersion1 was inserted");
        }



        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestAddAndUpdateMultipleObservations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Database connection, etc.
            //-----------------------------------------------------------------------------------------------------------
            MongoDbContext dbContext = new MongoDbContext(MongoUrl, DatabaseName, CollectionName);
            await dbContext.Mongodb.DropCollectionAsync(CollectionName);
            var observationRepository = new VersionedObservationRepository<ProcessedDwcObservation>(dbContext);
            const int numberOfObservations = 100000;

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create <DataProviderId, CatalogNumber> index
            //-----------------------------------------------------------------------------------------------------------
            var indexDefinition = Builders<VersionedObservation<ProcessedDwcObservation>>.IndexKeys.Combine(
                Builders<VersionedObservation<ProcessedDwcObservation>>.IndexKeys.Ascending(f => f.DataProviderId),
                Builders<VersionedObservation<ProcessedDwcObservation>>.IndexKeys.Ascending(f => f.CatalogNumber));
            CreateIndexOptions opts = new CreateIndexOptions { Unique = true };
            await observationRepository.Collection.Indexes.CreateOneAsync(new CreateIndexModel<VersionedObservation<ProcessedDwcObservation>>(indexDefinition, opts));

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create <CompositeId> index
            //-----------------------------------------------------------------------------------------------------------
            //await observationRepository.Collection.Indexes.CreateOneAsync(Builders<VersionedDocumentObservation<SpeciesObservation>>.IndexKeys.Ascending(_ => _.CompositeId), opts);

            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create original observations
            //-----------------------------------------------------------------------------------------------------------
            var speciesObservations = ProcessedDwcObservationTestRepository.CreateRandomObservations(numberOfObservations);


            //-----------------------------------------------------------------------------------------------------------
            // Act - Insert documents first version
            //-----------------------------------------------------------------------------------------------------------
            await observationRepository.InsertDocumentsAsync(speciesObservations);


            //-----------------------------------------------------------------------------------------------------------
            // Act - Update two observations and insert
            //-----------------------------------------------------------------------------------------------------------
            speciesObservations[0].RecordedBy = "Art Vandelay";
            speciesObservations[2].RecordedBy = "Peter Van Nostrand";

            // Insert again. The function make diff check on all observations and updates only those that have changed.
            await observationRepository.InsertDocumentsAsync(speciesObservations);


            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            var obs0 = await observationRepository.GetDocumentAsync(speciesObservations[0].DataProviderId, speciesObservations[0].CatalogNumber);
            var obs1 = await observationRepository.GetDocumentAsync(speciesObservations[1].DataProviderId, speciesObservations[1].CatalogNumber);
            var obs2 = await observationRepository.GetDocumentAsync(speciesObservations[2].DataProviderId, speciesObservations[2].CatalogNumber);

            obs0.Version.Should().Be(2, "This observation has been updated");
            obs1.Version.Should().Be(1, "This observation has not been updated");
            obs2.Version.Should().Be(2, "This observation has been updated");
        }

        ///// <summary>
        ///// Utility function for generating test code.
        ///// </summary>
        //[Fact]
        //public void CreateProperties()
        //{
        //    var enumerable = typeof(ProcessedDwcObservation).GetProperties().Select(x => x.Name);
        //    StringBuilder sb = new StringBuilder();
        //    foreach (var obj in enumerable)
        //    {
        //        sb.AppendLine($"observation.{obj} = null;");
        //    }

        //    string str = sb.ToString();
        //}
    }
}
