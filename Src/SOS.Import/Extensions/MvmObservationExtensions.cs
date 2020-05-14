using System;
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
            observation.ReportedBy = entity.Fields.FirstOrDefault(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.ReportedBy)?.Value;
            observation.Modified = entity.Fields.FirstOrDefault(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.Modified)?.Value;
            observation.Owner = entity.Fields.FirstOrDefault(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.Owner)?.Value;
            observation.IndividualId = entity.Fields.FirstOrDefault(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.IndividualID)?.Value;
            observation.RecordedBy = entity.Fields.FirstOrDefault(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.RecordedBy)?.Value;
            observation.OccurrenceId = entity.Fields.FirstOrDefault(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.OccurrenceID)?.Value;
            observation.DecimalLongitude = entity.Fields.First(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.DecimalLongitude).Value.WebParseDouble();
            observation.DecimalLatitude = entity.Fields.First(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.DecimalLatitude).Value.WebParseDouble();
            observation.CoordinateUncertaintyInMeters = entity.Fields.FirstOrDefault(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.CoordinateUncertaintyInMeters)?.Value?.WebParseInt32();            
            observation.Start = entity.Fields.First(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.Start).Value.WebParseDateTime();
            observation.End = entity.Fields.First(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.End).Value.WebParseDateTime();
            observation.ScientificName = entity.Fields.FirstOrDefault(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.ScientificName)?.Value;
            observation.DyntaxaTaxonId = entity.Fields.First(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.DyntaxaTaxonID).Value.WebParseInt32();
            observation.Municipality = entity.Fields.FirstOrDefault(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.Municipality)?.Value;
            observation.County = entity.Fields.FirstOrDefault(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.County)?.Value;
            observation.Locality = entity.Fields.FirstOrDefault(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.Locality)?.Value;
            observation.LocationId = entity.Fields.FirstOrDefault(p => p.Property.Id == MvmService.SpeciesObservationPropertyId.LocationId)?.Value;
            return observation;
        }
    }
}
