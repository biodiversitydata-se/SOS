using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Verbatim.Kul;

namespace SOS.Import.Extensions
{
    public static class KulObservationExtensions
    {
        /// <summary>
        ///  Cast multiple sightings entities to models .
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<KulObservationVerbatim> ToVerbatims(this IEnumerable<KulService.WebSpeciesObservation> entities)
        {
            return entities.Select(e => e.ToVerbatim());
        }

        /// <summary>
        /// Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static KulObservationVerbatim ToVerbatim(this KulService.WebSpeciesObservation entity)
        {
            KulObservationVerbatim observation = new KulObservationVerbatim();
            observation.ReportedBy = entity.Fields[(int)KulObservationFieldsId.ReportedBy].Value;
            observation.Owner = entity.Fields[(int)KulObservationFieldsId.Owner].Value;
            observation.RecordedBy = entity.Fields[(int)KulObservationFieldsId.RecordedBy].Value;
            observation.OccurrenceId = entity.Fields[(int)KulObservationFieldsId.OccurrenceId].Value;
            observation.DecimalLongitude = entity.Fields[(int)KulObservationFieldsId.DecimalLongitude].Value.WebParseDouble();
            observation.DecimalLatitude = entity.Fields[(int)KulObservationFieldsId.DecimalLatitude].Value.WebParseDouble();
            observation.CoordinateUncertaintyInMeters = entity.Fields[(int)KulObservationFieldsId.CoordinateUncertaintyInMeters].Value.WebParseInt32();
            observation.Start = entity.Fields[(int)KulObservationFieldsId.Start].Value.WebParseDateTime();
            observation.End = entity.Fields[(int)KulObservationFieldsId.End].Value.WebParseDateTime();
            observation.Locality = entity.Fields[(int)KulObservationFieldsId.Locality].Value;
            observation.DyntaxaTaxonId = entity.Fields[(int)KulObservationFieldsId.DyntaxaTaxonId].Value.WebParseInt32();
            observation.VerbatimScientificName = entity.Fields[(int)KulObservationFieldsId.VerbatimScientificName].Value;
            observation.TaxonRemarks = entity.Fields[(int)KulObservationFieldsId.TaxonRemarks].Value;
            observation.IndividualCount = entity.Fields[(int)KulObservationFieldsId.IndividualCount].Value.WebParseInt32();
            observation.CountryCode = entity.Fields[(int)KulObservationFieldsId.CountryCode].Value;
            observation.AssociatedOccurrences = entity.Fields[(int)KulObservationFieldsId.AssociatedOccurrences].Value;
            return observation;
        }

        private enum KulObservationFieldsId
        {
            ReportedBy = 0,
            Owner = 1,
            RecordedBy = 2,
            OccurrenceId = 3,
            DecimalLongitude = 4,
            DecimalLatitude = 5,
            CoordinateUncertaintyInMeters = 6,
            Start = 7,
            End = 8,
            Locality = 9,
            DyntaxaTaxonId = 10,
            VerbatimScientificName = 11,
            TaxonRemarks = 12,
            IndividualCount = 13,
            CountryCode = 14,
            AssociatedOccurrences = 15
        }
    }
}
