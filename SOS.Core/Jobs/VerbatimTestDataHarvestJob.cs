using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SOS.Core.Models.Observations;
using SOS.Core.Repositories;
using SOS.Core.TestDataFactories;

namespace SOS.Core.Jobs
{
    public interface IVerbatimTestDataHarvestJob
    {
        void Run(int nrObservations);
    }

    public class VerbatimTestDataHarvestJob : IVerbatimTestDataHarvestJob
    {
        private readonly IRepositorySettings _repositorySettings;

        public VerbatimTestDataHarvestJob() : this(SystemSettings.GetRepositorySettings())
        {
        }

        public VerbatimTestDataHarvestJob(IRepositorySettings repositorySettings)
        {
            _repositorySettings = repositorySettings;
        }

        public void Run(int nrObservations)
        {
            Console.WriteLine($"Start harvesting VerbatimTestDataProvider database: { DateTime.Now.ToLongTimeString() }");
            // 1. Get data from data source (in this case generate random observations)
            var verbatimObservations = VerbatimTestDataProviderObservationFactory.CreateRandomObservations(nrObservations);

            // 2. Drop verbatim collection to avoid duplicates. This should eventually be removed...
            MongoDbContext dbContext = new MongoDbContext(_repositorySettings.MongoDbConnectionString, _repositorySettings.DatabaseName, Constants.VerbatimTestDataProviderObservations);
            var repository = new VerbatimObservationRepository<VerbatimTestDataProviderObservation>(dbContext);
            repository.DropVerbatimObservationCollectionAsync().Wait();

            // 3. Add observations to database
            repository.InsertObservationsAsync(verbatimObservations).Wait();

            Console.WriteLine($"Finished harvesting VerbatimTestDataProvider database: { DateTime.Now.ToLongTimeString() }");
        }
    }
}
