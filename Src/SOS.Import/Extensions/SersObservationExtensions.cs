using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Verbatim.Sers;

namespace SOS.Import.Extensions
{
    public static class SersObservationExtensions
    {
        /// <summary>
        ///  Cast multiple sightings entities to models .
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<SersObservationVerbatim> ToVerbatims(this IEnumerable<SersService.WebSpeciesObservation> entities)
        {
            return entities.Select(e => e.ToVerbatim());
        }

        /// <summary>
        /// Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static SersObservationVerbatim ToVerbatim(this SersService.WebSpeciesObservation entity)
        {
            var observation = new SersObservationVerbatim();
            observation.ReportedBy = entity.Fields[(int)SersObservationFieldsId.ReportedBy].Value;
            observation.Modified = entity.Fields[(int)SersObservationFieldsId.Modified].Value;
            observation.Owner = entity.Fields[(int)SersObservationFieldsId.Owner].Value;
            observation.IndividualId = entity.Fields[(int)SersObservationFieldsId.IndividualId].Value;
            observation.RecordedBy = entity.Fields[(int)SersObservationFieldsId.RecordedBy].Value;
            observation.OccurrenceId = entity.Fields[(int)SersObservationFieldsId.OccurrenceId].Value;
            observation.DecimalLongitude = entity.Fields[(int)SersObservationFieldsId.DecimalLongitude].Value.WebParseDouble();
            observation.DecimalLatitude = entity.Fields[(int)SersObservationFieldsId.DecimalLatitude].Value.WebParseDouble();
            observation.CoordinateUncertaintyInMeters = entity.Fields[(int)SersObservationFieldsId.CoordinateUncertaintyInMeters].Value.WebParseInt32();
            observation.Start = entity.Fields[(int)SersObservationFieldsId.Start].Value.WebParseDateTime();
            observation.End = entity.Fields[(int)SersObservationFieldsId.End].Value.WebParseDateTime();
            observation.ScientificName = entity.Fields[(int)SersObservationFieldsId.ScientificName].Value;
            observation.DyntaxaTaxonId = entity.Fields[(int)SersObservationFieldsId.DyntaxaTaxonId].Value.WebParseInt32();
            observation.Municipality = entity.Fields[(int)SersObservationFieldsId.Municipality].Value;
            observation.County = entity.Fields[(int)SersObservationFieldsId.County].Value;
            observation.Locality = entity.Fields[(int)SersObservationFieldsId.Locality].Value;
            observation.LocationId = entity.Fields[(int)SersObservationFieldsId.LocationId].Value;
            return observation;
        }

        private enum SersObservationFieldsId
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
