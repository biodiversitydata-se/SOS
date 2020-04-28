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
            observation.ReportedBy = entity.Fields.FirstOrDefault(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.ReportedBy)?.Value;
            observation.Modified = entity.Fields.FirstOrDefault(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.Modified)?.Value;
            observation.Owner = entity.Fields.FirstOrDefault(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.Owner)?.Value;
            observation.IndividualId = entity.Fields.FirstOrDefault(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.OccurrenceID)?.Value;
            observation.RecordedBy = entity.Fields.FirstOrDefault(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.RecordedBy)?.Value;
            observation.OccurrenceId = entity.Fields.FirstOrDefault(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.OccurrenceID)?.Value;
            observation.DecimalLongitude = entity.Fields.First(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.DecimalLongitude).Value.WebParseDouble();
            observation.DecimalLatitude = entity.Fields.First(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.DecimalLatitude).Value.WebParseDouble();
            observation.CoordinateUncertaintyInMeters = entity.Fields.FirstOrDefault(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.CoordinateUncertaintyInMeters)?.Value?.WebParseInt32();
            observation.Start = entity.Fields.First(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.Start).Value.WebParseDateTime();
            observation.End = entity.Fields.First(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.End).Value.WebParseDateTime();
            observation.ScientificName = entity.Fields.FirstOrDefault(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.ScientificName)?.Value;
            observation.DyntaxaTaxonId = entity.Fields.First(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.DyntaxaTaxonID).Value.WebParseInt32();
            observation.Municipality = entity.Fields.FirstOrDefault(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.Municipality)?.Value;
            observation.County = entity.Fields.FirstOrDefault(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.County)?.Value;
            observation.Locality = entity.Fields.FirstOrDefault(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.Locality)?.Value;
            observation.LocationId = entity.Fields.FirstOrDefault(p => p.Property.Id == NorsService.SpeciesObservationPropertyId.LocationId)?.Value;
            return observation;

        }
    }
}
