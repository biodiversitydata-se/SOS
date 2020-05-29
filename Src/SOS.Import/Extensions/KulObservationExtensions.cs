using System.Collections.Generic;
using System.Linq;
using KulService;
using SOS.Lib.Models.Verbatim.Kul;

namespace SOS.Import.Extensions
{
    public static class KulObservationExtensions
    {
        /// <summary>
        ///     Cast multiple sightings entities to models .
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<KulObservationVerbatim> ToVerbatims(this IEnumerable<WebSpeciesObservation> entities)
        {
            return entities.Select(e => e.ToVerbatim());
        }

        /// <summary>
        ///     Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static KulObservationVerbatim ToVerbatim(this WebSpeciesObservation entity)
        {
            var observation = new KulObservationVerbatim();
            observation.ReportedBy = entity.Fields
                .FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.ReportedBy)?.Value;
            observation.Owner = entity.Fields.FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.Owner)
                ?.Value;
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
            observation.Locality = entity.Fields
                .FirstOrDefault(p => p.Property.Id == SpeciesObservationPropertyId.Locality)?.Value;
            observation.DyntaxaTaxonId = entity.Fields
                .First(p => p.Property.Id == SpeciesObservationPropertyId.DyntaxaTaxonID).Value.WebParseInt32();
            observation.VerbatimScientificName = entity.Fields
                .First(p => p.Property.Id == SpeciesObservationPropertyId.VerbatimScientificName).Value;
            observation.TaxonRemarks =
                entity.Fields.First(p => p.Property.Id == SpeciesObservationPropertyId.TaxonRemarks).Value;
            observation.IndividualCount = entity.Fields
                .First(p => p.Property.Id == SpeciesObservationPropertyId.IndividualCount).Value.WebParseInt32();
            observation.CountryCode =
                entity.Fields.First(p => p.Property.Id == SpeciesObservationPropertyId.CountryCode).Value;
            observation.AssociatedOccurrences = entity.Fields
                .First(p => p.Property.Id == SpeciesObservationPropertyId.AssociatedOccurrences).Value;

            return observation;
        }
    }
}