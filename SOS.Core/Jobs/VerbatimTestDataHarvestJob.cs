using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SOS.Core.Models.Observations;
using SOS.Core.Repositories;
using SOS.Core.TestDataFactories;

namespace SOS.Core.Jobs
{
    public class VerbatimTestDataHarvestJob
    {
        private const string MongoUrl = "mongodb://localhost";
        private const string DatabaseName = "diff_sample";

        public void Run(int nrObservations)
        {
            Console.WriteLine($"Start harvesting VerbatimTestDataProvider database: { DateTime.Now.ToLongTimeString() }");
            // 1. Get data from data source (in this case generate random observations)
            var verbatimObservations = VerbatimTestDataProviderObservationFactory.CreateRandomObservations(nrObservations);

            // 2. Drop verbatim collection to avoid duplicates. This should eventually be removed...
            MongoDbContext dbContext = new MongoDbContext(MongoUrl, DatabaseName, Constants.VerbatimTestDataProviderObservations);
            var repository = new VerbatimObservationRepository<VerbatimTestDataProviderObservation>(dbContext);
            repository.DropVerbatimObservationCollectionAsync().Wait();

            // 3. Add observations to database
            repository.InsertObservationsAsync(verbatimObservations).Wait();

            Console.WriteLine($"Finished harvesting VerbatimTestDataProvider database: { DateTime.Now.ToLongTimeString() }");
        }
    }
}
