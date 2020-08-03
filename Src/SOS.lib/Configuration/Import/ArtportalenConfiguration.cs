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
        public int ChunkSize { get; set; } = 1000000;

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
    }
}