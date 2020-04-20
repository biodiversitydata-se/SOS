using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Verbatim.Mvm;

namespace SOS.Import.Extensions
{
    public static class MvmObservationExtensions
    {
        /// <summary>
        ///  Cast multiple sightings entities to models .
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<MvmObservationVerbatim> ToVerbatims(this IEnumerable<MvmService.WebSpeciesObservation> entities)
        {
            return entities.Select(e => e.ToVerbatim());
        }

        /// <summary>
        /// Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static MvmObservationVerbatim ToVerbatim(this MvmService.WebSpeciesObservation entity)
        {
            var observation = new MvmObservationVerbatim();
            observation.ReportedBy = entity.Fields[(int)MvmObservationFieldsId.ReportedBy].Value;
            observation.Modified = entity.Fields[(int)MvmObservationFieldsId.Modified].Value;
            observation.Owner = entity.Fields[(int)MvmObservationFieldsId.Owner].Value;
            observation.IndividualId = entity.Fields[(int)MvmObservationFieldsId.IndividualId].Value;
            observation.RecordedBy = entity.Fields[(int)MvmObservationFieldsId.RecordedBy].Value;
            observation.OccurrenceId = entity.Fields[(int)MvmObservationFieldsId.OccurrenceId].Value;
            observation.DecimalLongitude = entity.Fields[(int)MvmObservationFieldsId.DecimalLongitude].Value.WebParseDouble();
            observation.DecimalLatitude = entity.Fields[(int)MvmObservationFieldsId.DecimalLatitude].Value.WebParseDouble();
            observation.CoordinateUncertaintyInMeters = entity.Fields[(int)MvmObservationFieldsId.CoordinateUncertaintyInMeters].Value.WebParseInt32();
            observation.Start = entity.Fields[(int)MvmObservationFieldsId.Start].Value.WebParseDateTime();
            observation.End = entity.Fields[(int)MvmObservationFieldsId.End].Value.WebParseDateTime();
            observation.ScientificName = entity.Fields[(int)MvmObservationFieldsId.ScientificName].Value;
            observation.DyntaxaTaxonId = entity.Fields[(int)MvmObservationFieldsId.DyntaxaTaxonId].Value.WebParseInt32();
            observation.Municipality = entity.Fields[(int)MvmObservationFieldsId.Municipality].Value;
            observation.County = entity.Fields[(int)MvmObservationFieldsId.County].Value;
            observation.Locality = entity.Fields[(int)MvmObservationFieldsId.Locality].Value;
            observation.LocationId = entity.Fields[(int)MvmObservationFieldsId.LocationId].Value;
            return observation;
        }

        private enum MvmObservationFieldsId
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
