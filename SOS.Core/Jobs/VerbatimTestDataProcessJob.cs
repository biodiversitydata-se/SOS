using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using SOS.Core.GIS;
using SOS.Core.Models.Observations;
using SOS.Core.Models.Versioning;
using SOS.Core.ObservationProcessors;
using SOS.Core.Repositories;

namespace SOS.Core.Jobs
{
    public class VerbatimTestDataProcessJob
    {
        private const string MongoUrl = "mongodb://localhost";
        private const string DatabaseName = "diff_sample";

        public void Run()
        {
            Console.WriteLine($"Start processing VerbatimTestDataProvider observations: { DateTime.Now.ToLongTimeString() }");
            
            // 1. Get all verbatim observations
            MongoDbContext dbContext = new MongoDbContext(MongoUrl, DatabaseName, Constants.VerbatimTestDataProviderObservations);
            var repository = new VerbatimObservationRepository<VerbatimTestDataProviderObservation>(dbContext);
            IEnumerable<VerbatimTestDataProviderObservation> verbatimObservations = repository.GetAllObservations();

            // 2. Process observations
            var processedObservationsBag = new ConcurrentBag<ProcessedDwcObservation>();
            TestDataProviderProcessor observationProcessor = new TestDataProviderProcessor();
            Parallel.ForEach(verbatimObservations, obs =>
            {
                var processedObservation = observationProcessor.ProcessObservation(obs);
                if (FileBasedGeographyService.IsObservationInSweden(processedObservation))
                {
                    processedObservationsBag.Add(processedObservation);
                }
            });

            // 3. Save observations
            MongoDbContext observationsDbContext = new MongoDbContext(MongoUrl, DatabaseName, Constants.ObservationCollectionName);
            var observationRepository = new VersionedObservationRepository<ProcessedDwcObservation>(observationsDbContext);
            observationRepository.InsertDocumentsAsync(processedObservationsBag.ToList()).Wait();

            Console.WriteLine($"Finished processing VerbatimTestDataProvider observations: { DateTime.Now.ToLongTimeString() }");
        }
    }
}
