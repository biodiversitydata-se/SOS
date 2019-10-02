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
    public interface IVerbatimTestDataProcessJob
    {
        void Run();
    }

    public class VerbatimTestDataProcessJob : IVerbatimTestDataProcessJob
    {
        private readonly IRepositorySettings _repositorySettings;

        public VerbatimTestDataProcessJob() : this(SystemSettings.GetRepositorySettings())
        {
        }

        public VerbatimTestDataProcessJob(IRepositorySettings repositorySettings)
        {
            _repositorySettings = repositorySettings;
        }

        public void Run()
        {
            Console.WriteLine($"Start processing VerbatimTestDataProvider observations: { DateTime.Now.ToLongTimeString() }");
            
            // 1. Get all verbatim observations
            MongoDbContext dbContext = new MongoDbContext(
                _repositorySettings.MongoDbConnectionString, 
                _repositorySettings.DatabaseName, 
                Constants.VerbatimTestDataProviderObservations);
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
            MongoDbContext observationsDbContext = new MongoDbContext(
                _repositorySettings.MongoDbConnectionString, 
                _repositorySettings.DatabaseName, 
                Constants.ObservationCollectionName);
            var observationRepository = new VersionedObservationRepository<ProcessedDwcObservation>(observationsDbContext);
            observationRepository.InsertDocumentsAsync(processedObservationsBag.ToList()).Wait();

            Console.WriteLine($"Finished processing VerbatimTestDataProvider observations: { DateTime.Now.ToLongTimeString() }");
        }
    }
}
