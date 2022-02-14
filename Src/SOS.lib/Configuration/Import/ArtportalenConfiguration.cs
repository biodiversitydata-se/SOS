using System;
using System.Collections.Generic;

namespace SOS.Lib.Configuration.Import
{
    public class ArtportalenConfiguration
    {
        /// <summary>
        /// Max number of new observations to perform live catch up
        /// </summary>
        public int CatchUpLimit { get; set; }

        /// <summary>
        ///     Artportalen connection settings backup database.
        /// </summary>
        public string ConnectionStringBackup { get; set; }

        /// <summary>
        ///     Artportalen connection settings live database.
        /// </summary>
        public string ConnectionStringLive { get; set; }

        /// <summary>
        ///     The number of observations that will be fetched in each loop.
        /// </summary>
        public int ChunkSize { get; set; } = 100000;

        /// <summary>
        ///     The number of observations that will be fetched in each loop.
        /// </summary>
        public int IncrementalChunkSize { get; set; } = 25000;

        /// <summary>
        ///     The number of sightings that should be harvested.
        ///     If set to null all sightings will be fetched.
        /// </summary>
        public int? MaxNumberOfSightingsHarvested { get; set; } = null;

        public int NoOfThreads { get; set; } = 2;

        /// <summary>
        ///     Set to true to add sightings for testing purpose.
        /// </summary>
        public bool AddTestSightings { get; set; } = false;

        /// <summary>
        ///     Sighting ids that should be added when AddTestSightings is set to true.
        /// </summary>
        public List<int> AddTestSightingIds { get; set; }

        /// <summary>
        /// Harvest sightings from this start date
        /// </summary>
        public DateTime? HarvestStartDate { get; set; }

        /// <summary>
        /// Time to sleep after a batch has run (ms) 
        /// </summary>
        public int SleepAfterBatch { get; set; }

        /// <summary>
        /// Use triggered observation rule if set
        /// </summary>
        public bool UseTriggeredObservationRule { get; set; }
    }
}