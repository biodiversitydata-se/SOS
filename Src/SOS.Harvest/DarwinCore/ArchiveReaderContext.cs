using DwC_A;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;

namespace SOS.Harvest.DarwinCore
{
    /// <summary>
    /// Holds context information used when reading DwC-A files. 
    /// The files should be read in the following order:
    /// 1. Dataset
    /// 2. Occurrences (because dataset info is used)
    /// 3. Events (because dataset and occurrence info is used)
    /// </summary>
    public class ArchiveReaderContext
    {
        public ArchiveReader ArchiveReader { get; set; }
        public Dictionary<string, DwcVerbatimDataset> DatasetByEventId { get; set; }
        public Dictionary<string, List<string>> OccurrenceIdsByEventId { get; set; }
        public IIdIdentifierTuple DataProvider { get; set; }
        public int MaxNrObservationsToReturn { get; set; } = int.MaxValue;
        public int BatchSize { get; set; } = 100000;

        public static ArchiveReaderContext Create(ArchiveReader archiveReader)
        {
            return new ArchiveReaderContext
            {
                ArchiveReader = archiveReader
            };
        }

        public static ArchiveReaderContext Create(ArchiveReader archiveReader, IIdIdentifierTuple dataProvider)
        {
            return new ArchiveReaderContext
            {
                ArchiveReader = archiveReader,
                DataProvider = dataProvider
            };
        }

        public static ArchiveReaderContext Create(ArchiveReader archiveReader, IIdIdentifierTuple dataProvider, DwcaConfiguration dwcaConfiguration)
        {
            return new ArchiveReaderContext
            {
                ArchiveReader = archiveReader,
                DataProvider = dataProvider,
                MaxNrObservationsToReturn = dwcaConfiguration.MaxNumberOfSightingsHarvested.GetValueOrDefault(int.MaxValue),
                BatchSize= dwcaConfiguration.BatchSize
            };
        }
    }
}