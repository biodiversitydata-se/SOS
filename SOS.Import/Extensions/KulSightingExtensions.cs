using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SOS.Lib.Models.Verbatim.Kul;

namespace SOS.Import.Extensions
{
    public static class KulSightingExtensions
    {
        /// <summary>
        ///  Cast multiple sightings entities to models .
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<KulSightingVerbatim> ToAggregates(this IEnumerable<KulService.WebSpeciesObservation> entities)
        {
            return entities.Select(e => e.ToAggregate());
        }

        /// <summary>
        /// Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static KulSightingVerbatim ToAggregate(this KulService.WebSpeciesObservation entity)
        {
            KulSightingVerbatim sighting = new KulSightingVerbatim();
            sighting.ReportedBy = entity.Fields[(int)KulObservationFieldsId.ReportedBy].Value;
            sighting.Owner = entity.Fields[(int)KulObservationFieldsId.Owner].Value;
            sighting.RecordedBy = entity.Fields[(int)KulObservationFieldsId.RecordedBy].Value;
            sighting.OccurrenceId = entity.Fields[(int)KulObservationFieldsId.OccurrenceId].Value;
            sighting.DecimalLongitude = entity.Fields[(int)KulObservationFieldsId.DecimalLongitude].Value.WebParseDouble();
            sighting.DecimalLatitude = entity.Fields[(int)KulObservationFieldsId.DecimalLatitude].Value.WebParseDouble();
            sighting.CoordinateUncertaintyInMeters = entity.Fields[(int)KulObservationFieldsId.CoordinateUncertaintyInMeters].Value.WebParseInt32();
            sighting.Start = entity.Fields[(int)KulObservationFieldsId.Start].Value.WebParseDateTime();
            sighting.End = entity.Fields[(int)KulObservationFieldsId.End].Value.WebParseDateTime();
            sighting.Locality = entity.Fields[(int)KulObservationFieldsId.Locality].Value;
            sighting.DyntaxaTaxonId = entity.Fields[(int)KulObservationFieldsId.DyntaxaTaxonId].Value.WebParseInt32();
            sighting.VerbatimScientificName = entity.Fields[(int)KulObservationFieldsId.VerbatimScientificName].Value;
            sighting.TaxonRemarks = entity.Fields[(int)KulObservationFieldsId.TaxonRemarks].Value;
            sighting.IndividualCount = entity.Fields[(int)KulObservationFieldsId.IndividualCount].Value.WebParseInt32();
            sighting.CountryCode = entity.Fields[(int)KulObservationFieldsId.CountryCode].Value;
            sighting.AssociatedOccurrences = entity.Fields[(int)KulObservationFieldsId.AssociatedOccurrences].Value;
            return sighting;
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
