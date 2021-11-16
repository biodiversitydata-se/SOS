using System;

namespace SOS.Administration.Gui.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ProtectedLogObservationDto
    {
        /// <summary>
        /// County
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// Date request was made
        /// </summary>
        public DateTime? IssueDate { get; set; }

        /// <summary>
        /// observation latitude
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Locality
        /// </summary>
        public string Locality { get; set; }

        /// <summary>
        /// observation longitude
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Municipality
        /// </summary>
        public string Municipality { get; set; }

        /// <summary>
        /// Province 
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// Common name of taxon
        /// </summary>
        public string TaxonCommonName { get; set; }

        /// <summary>
        /// Taxon id
        /// </summary>
        public int? TaxonId { get; set; }

        /// <summary>
        /// Protection level of taxa
        /// </summary>
        [Obsolete("Replaced by TaxonSensitivityCategory")]
        public int? TaxonProtectionLevel { get; set; }

        /// <summary>
        /// Protection level of taxa
        /// </summary>
        public int? TaxonSensitivityCategory { get; set; }

        /// <summary>
        /// Scientific name of taxon
        /// </summary>
        public string TaxonScientificName { get; set; }
    }
}
