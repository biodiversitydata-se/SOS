using System;

namespace SOS.Lib.Models.Reports
{
    internal class CountyOccurrenceReportRow
    {
        public long CountyId { get; set; }
        public string County { get; set; }
        public DateTime? LastRecorded { get; set; }
        public long ObservationCountAllDatasets { get; set; }
        public long ObservationCountArtportalen { get; set; }
        public long ObservationCountArtportalenVerified { get; set; }
        public long ObservationCountAllDatasetsPre2019 { get; set; }
        public long ObservationCountArtportalenPre2019 { get; set; }
        public long ObservationCountArtportalenVerifiedPre2019 { get; set; }
        public long ObservationCountAllDatasetsPost2018 { get; set; }
        public long ObservationCountArtportalenPost2018 { get; set; }
        public long ObservationCountArtportalenVerifiedPost2018 { get; set; }
    }
}
