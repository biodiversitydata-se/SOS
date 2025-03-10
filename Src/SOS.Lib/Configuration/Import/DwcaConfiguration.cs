﻿
namespace SOS.Lib.Configuration.Import
{
    /// <summary>
    /// DwC-A configuration.
    /// </summary>
    public class DwcaConfiguration
    {
        /// <summary>
        /// How many observations to read/batch
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// The path to temporarily store DwC-A files when importing them.
        /// </summary>
        public string ImportPath { get; set; }

        /// <summary>
        ///     The number of sightings that should be harvested.
        ///     If set to null all sightings will be fetched.
        /// </summary>
        public int? MaxNumberOfSightingsHarvested { get; set; } = null;

        public bool ForceHarvestUnchangedDwca { get; set; } = false;
    }
}