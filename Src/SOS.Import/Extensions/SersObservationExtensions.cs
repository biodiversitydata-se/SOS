using System;
using System.Collections.Generic;
using System.Linq;
using SersService;
using SOS.Lib.Models.Verbatim.Sers;

namespace SOS.Import.Extensions
{
    public static class SersObservationExtensions
    {
        /// <summary>
        ///     Cast multiple sightings entities to models .
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<SersObservationVerbatim> ToVerbatims(this IEnumerable<WebSpeciesObservation> entities)
        {
            return entities.Select(e => e.ToVerbatim());
        }

        /// <summary>
        ///     Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static SersObservationVerbatim ToVerbatim(this WebSpeciesObservation entity)
        {
            var observation = new SersObservationVerbatim();
            observation.ReportedBy = entity.Fields
                .FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.ReportedBy)?.Value;
            DateTime.TryParse(
                entity.Fields.FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.Modified)
                    ?.Value, out var modified);
            observation.Modified = modified;
            observation.Owner = entity.Fields.FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.Owner)
                ?.Value;
            observation.IndividualId = entity.Fields
                .FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.IndividualID)?.Value;
            observation.RecordedBy = entity.Fields
                .FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.RecordedBy)?.Value;
            observation.OccurrenceId = entity.Fields
                .FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.OccurrenceID)?.Value;
            observation.DecimalLongitude = entity.Fields
                .First(p => p.Property.Id == SpeciesObservationPropertyId.DecimalLongitude).Value.WebParseDouble();
            observation.DecimalLatitude = entity.Fields
                .First(p => p.Property.Id == SpeciesObservationPropertyId.DecimalLatitude).Value.WebParseDouble();
            observation.CoordinateUncertaintyInMeters = entity.Fields
                .FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.CoordinateUncertaintyInMeters)?.Value
                ?.WebParseInt32();
            observation.Start = entity.Fields.First(p => p.Property.Id == SpeciesObservationPropertyId.Start).Value
                .WebParseDateTime();
            observation.End = entity.Fields.First(p => p.Property.Id == SpeciesObservationPropertyId.End).Value
                .WebParseDateTime();
            observation.ScientificName = entity.Fields
                .FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.ScientificName)?.Value;
            observation.DyntaxaTaxonId = entity.Fields
                .First(p => p.Property.Id == SpeciesObservationPropertyId.DyntaxaTaxonID).Value.WebParseInt32();
            observation.Municipality = entity.Fields
                .FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.Municipality)?.Value;
            observation.County = entity.Fields.FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.County)
                ?.Value;
            observation.Locality = entity.Fields
                .FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.Locality)?.Value;
            observation.LocationId = entity.Fields
                .FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.LocationId)?.Value;

            return observation;
        }
    }
}