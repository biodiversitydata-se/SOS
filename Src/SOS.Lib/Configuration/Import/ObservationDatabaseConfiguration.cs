﻿
namespace SOS.Lib.Configuration.Import
{
    public class ObservationDatabaseConfiguration
    {
        /// <summary>
        ///     Observations database connection settings.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     The number of observations that will be fetched in each loop.
        /// </summary>
        public int ChunkSize { get; set; } = 1000000;

        /// <summary>
        ///     The number of sightings that should be harvested.
        ///     If set to null all sightings will be fetched.
        /// </summary>
        public int? MaxNumberOfSightingsHarvested { get; set; } = null;

        /// <summary>
        /// Number of threads to run
        /// </summary>
        public int NoOfThreads { get; set; } = 2;
    }
}