using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.VirtualHerbarium
{
    /// <summary>
    ///     Verbatim from Virtual Herbarium
    /// </summary>
    public class VirtualHerbariumObservationVerbatim : IEntity<int>
    {
        /// <summary>
        ///     Accession No
        /// </summary>
        public string AccessionNo { get; set; }

        /// <summary>
        ///     Collector
        /// </summary>
        public string Collector { get; set; }

        /// <summary>
        ///     Coordinate Precision
        /// </summary>
        public int? CoordinatePrecision { get; set; }

        /// <summary>
        ///     Coordinate Source
        /// </summary>
        public string CoordinateSource { get; set; }

        /// <summary>
        ///     Collector number
        /// </summary>
        public string Collectornumber { get; set; }

        /// <summary>
        ///     Date Collected
        /// </summary>
        public string DateCollected { get; set; }

        /// <summary>
        ///     Decimal Latitude
        /// </summary>
        public double DecimalLatitude { get; set; }

        /// <summary>
        ///     Decimal Longitude
        /// </summary>
        public double DecimalLongitude { get; set; }

        /// <summary>
        ///     District
        /// </summary>
        public string District { get; set; }

        /// <summary>
        ///     Dyntaxa taxon id
        /// </summary>
        public int DyntaxaId { get; set; }

        /// <summary>
        ///     File id
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        ///     Institution Code
        /// </summary>
        public string InstitutionCode { get; set; }

        /// <summary>
        ///     Locality
        /// </summary>
        public string Locality { get; set; }

        /// <summary>
        ///     Notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        ///     Original name
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        ///     Original text
        /// </summary>
        public string OriginalText { get; set; }

        /// <summary>
        ///     Province
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        ///     Taxon scientific name
        /// </summary>
        public string ScientificName { get; set; }

        /// <summary>
        ///     Unique id
        /// </summary>
        public int Id { get; set; }
    }
}