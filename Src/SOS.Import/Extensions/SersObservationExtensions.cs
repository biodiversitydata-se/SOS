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
            observation.ReportedBy = entity.Fields.FirstOrDefault(p => p.Property.Id == SersService.SpeciesObservationPropertyId.ReportedBy)?.Value;
            observation.Modified = entity.Fields.FirstOrDefault(p => p.Property.Id == SersService.SpeciesObservationPropertyId.Modified)?.Value;
            observation.Owner = entity.Fields.FirstOrDefault(p => p.Property.Id == SersService.SpeciesObservationPropertyId.Owner)?.Value;
            observation.IndividualId = entity.Fields.FirstOrDefault(p => p.Property.Id == SersService.SpeciesObservationPropertyId.OccurrenceID)?.Value;
            observation.RecordedBy = entity.Fields.FirstOrDefault(p => p.Property.Id == SersService.SpeciesObservationPropertyId.RecordedBy)?.Value;
            observation.OccurrenceId = entity.Fields.FirstOrDefault(p => p.Property.Id == SersService.SpeciesObservationPropertyId.OccurrenceID)?.Value;
            observation.DecimalLongitude = entity.Fields.First(p => p.Property.Id == SersService.SpeciesObservationPropertyId.DecimalLongitude).Value.WebParseDouble();
            observation.DecimalLatitude = entity.Fields.First(p => p.Property.Id == SersService.SpeciesObservationPropertyId.DecimalLatitude).Value.WebParseDouble();
            observation.CoordinateUncertaintyInMeters = entity.Fields.FirstOrDefault(p => p.Property.Id == SersService.SpeciesObservationPropertyId.CoordinateUncertaintyInMeters)?.Value?.WebParseInt32();
            observation.Start = entity.Fields.First(p => p.Property.Id == SersService.SpeciesObservationPropertyId.Start).Value.WebParseDateTime();
            observation.End = entity.Fields.First(p => p.Property.Id == SersService.SpeciesObservationPropertyId.End).Value.WebParseDateTime();
            observation.ScientificName = entity.Fields.FirstOrDefault(p => p.Property.Id == SersService.SpeciesObservationPropertyId.ScientificName)?.Value;
            observation.DyntaxaTaxonId = entity.Fields.First(p => p.Property.Id == SersService.SpeciesObservationPropertyId.DyntaxaTaxonID).Value.WebParseInt32();
            observation.Municipality = entity.Fields.FirstOrDefault(p => p.Property.Id == SersService.SpeciesObservationPropertyId.Municipality)?.Value;
            observation.County = entity.Fields.FirstOrDefault(p => p.Property.Id == SersService.SpeciesObservationPropertyId.County)?.Value;
            observation.Locality = entity.Fields.FirstOrDefault(p => p.Property.Id == SersService.SpeciesObservationPropertyId.Locality)?.Value;
            observation.LocationId = entity.Fields.FirstOrDefault(p => p.Property.Id == SersService.SpeciesObservationPropertyId.LocationId)?.Value;
            
            return observation;
        }
    }
}
