using System.Collections.Generic;

namespace SOS.Lib.Models.Verbatim.DarwinCore
{
    /// <summary>
    /// Event object including all observations associated with this event.
    /// There could be a problem to store this object in MongoDB due to the document size limit.
    /// </summary>
    public class DwcEventOccurrenceVerbatim : DwcEventVerbatim
    {
        /// <summary>
        ///     Darwin Core term name: basisOfRecord.
        ///     The specific nature of the data record -
        ///     a subtype of the dcterms:type.
        ///     Recommended best practice is to use a controlled
        ///     vocabulary such as the Darwin Core Type Vocabulary
        ///     (http://rs.tdwg.org/dwc/terms/type-vocabulary/index.htm).
        ///     In Species Gateway this property has the value
        ///     HumanObservation.
        /// </summary>
        public string BasisOfRecord { get; set; }

        /// <summary>
        ///     Darwin Core term name: identificationVerificationStatus.
        ///     A categorical indicator of the extent to which the taxonomic
        ///     identification has been verified to be correct.
        ///     Recommended best practice is to use a controlled vocabulary
        ///     such as that used in HISPID/ABCD.
        /// </summary>
        public string IdentificationVerificationStatus { get; set; }

        /// <summary>
        ///     Observations linked to the event.
        /// </summary>
        public ICollection<DwcObservationVerbatim> Observations { get; set; }

        /// <summary>
        ///     Darwin Core term name: recordedBy.
        ///     A list (concatenated and separated) of names of people,
        ///     groups, or organizations responsible for recording the
        ///     original Occurrence. The primary collector or observer,
        ///     especially one who applies a personal identifier
        ///     (recordNumber), should be listed first.
        /// </summary>
        public string RecordedBy { get; set; }

        /// <summary>
        /// Time spent to find taxa
        /// </summary>
        public string SamplingEffortTime { get; set; }

        /// <summary>
        /// List of taxon
        /// </summary>
        public ICollection<DwcTaxon> Taxa { get; set; }
    }
}