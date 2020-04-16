using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Verbatim.Nors;

namespace SOS.Import.Extensions
{
    public static class NorsObservationExtensions
    {
        /// <summary>
        ///  Cast multiple sightings entities to models .
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<NorsObservationVerbatim> ToVerbatims(this IEnumerable<NorsService.WebSpeciesObservation> entities)
        {
            return entities.Select(e => e.ToVerbatim());
        }

        /// <summary>
        /// Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static NorsObservationVerbatim ToVerbatim(this NorsService.WebSpeciesObservation entity)
        {
            var observation = new NorsObservationVerbatim();
            observation.ReportedBy = entity.Fields[(int)NorsObservationFieldsId.ReportedBy].Value;
            observation.Modified = entity.Fields[(int)NorsObservationFieldsId.Modified].Value;
            observation.Owner = entity.Fields[(int)NorsObservationFieldsId.Owner].Value;
            observation.IndividualId = entity.Fields[(int)NorsObservationFieldsId.IndividualId].Value;
            observation.RecordedBy = entity.Fields[(int)NorsObservationFieldsId.RecordedBy].Value;
            observation.OccurrenceId = entity.Fields[(int)NorsObservationFieldsId.OccurrenceId].Value;
            observation.DecimalLongitude = entity.Fields[(int)NorsObservationFieldsId.DecimalLongitude].Value.WebParseDouble();
            observation.DecimalLatitude = entity.Fields[(int)NorsObservationFieldsId.DecimalLatitude].Value.WebParseDouble();
            observation.CoordinateUncertaintyInMeters = entity.Fields[(int)NorsObservationFieldsId.CoordinateUncertaintyInMeters].Value.WebParseInt32();
            observation.Start = entity.Fields[(int)NorsObservationFieldsId.Start].Value.WebParseDateTime();
            observation.End = entity.Fields[(int)NorsObservationFieldsId.End].Value.WebParseDateTime();
            observation.ScientificName = entity.Fields[(int)NorsObservationFieldsId.ScientificName].Value;
            observation.DyntaxaTaxonId = entity.Fields[(int)NorsObservationFieldsId.DyntaxaTaxonId].Value.WebParseInt32();
            observation.Municipality = entity.Fields[(int)NorsObservationFieldsId.Municipality].Value;
            observation.County = entity.Fields[(int)NorsObservationFieldsId.County].Value;
            observation.Locality = entity.Fields[(int)NorsObservationFieldsId.Locality].Value;
            observation.LocationId = entity.Fields[(int)NorsObservationFieldsId.LocationId].Value;
            return observation;
        }

        private enum NorsObservationFieldsId
        {
            ReportedBy = 0,
            Modified = 1,
            Owner = 2,
            IndividualId = 3,
            RecordedBy = 4,
            OccurrenceId = 5,
            DecimalLongitude = 6,
            DecimalLatitude = 7,
            CoordinateUncertaintyInMeters = 8,
            Start = 9,
            End = 10,
            ScientificName = 11,
            DyntaxaTaxonId = 12,
            Municipality = 13,
            County = 14,
            Locality = 15,
            LocationId = 16
        }
    }
}
