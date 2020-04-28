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
            var observation = new KulObservationVerbatim();
            observation.ReportedBy = entity.Fields.FirstOrDefault(p => p.Property.Id == KulService.SpeciesObservationPropertyId.ReportedBy)?.Value;
            observation.Owner = entity.Fields.FirstOrDefault(p => p.Property.Id == KulService.SpeciesObservationPropertyId.Owner)?.Value;
            observation.RecordedBy = entity.Fields.FirstOrDefault(p => p.Property.Id == KulService.SpeciesObservationPropertyId.RecordedBy)?.Value;
            observation.OccurrenceId = entity.Fields.FirstOrDefault(p => p.Property.Id == KulService.SpeciesObservationPropertyId.OccurrenceID)?.Value;
            observation.DecimalLongitude = entity.Fields.First(p => p.Property.Id == KulService.SpeciesObservationPropertyId.DecimalLongitude).Value.WebParseDouble();
            observation.DecimalLatitude = entity.Fields.First(p => p.Property.Id == KulService.SpeciesObservationPropertyId.DecimalLatitude).Value.WebParseDouble();
            observation.CoordinateUncertaintyInMeters = entity.Fields.FirstOrDefault(p => p.Property.Id == KulService.SpeciesObservationPropertyId.CoordinateUncertaintyInMeters)?.Value?.WebParseInt32();
            observation.Start = entity.Fields.First(p => p.Property.Id == KulService.SpeciesObservationPropertyId.Start).Value.WebParseDateTime();
            observation.End = entity.Fields.First(p => p.Property.Id == KulService.SpeciesObservationPropertyId.End).Value.WebParseDateTime();
            observation.Locality = entity.Fields.FirstOrDefault(p => p.Property.Id == KulService.SpeciesObservationPropertyId.Locality)?.Value;
            observation.DyntaxaTaxonId = entity.Fields.First(p => p.Property.Id == KulService.SpeciesObservationPropertyId.DyntaxaTaxonID).Value.WebParseInt32();
            observation.VerbatimScientificName = entity.Fields.First(p => p.Property.Id == KulService.SpeciesObservationPropertyId.VerbatimScientificName).Value;
            observation.TaxonRemarks = entity.Fields.First(p => p.Property.Id == KulService.SpeciesObservationPropertyId.TaxonRemarks).Value;
            observation.IndividualCount = entity.Fields.First(p => p.Property.Id == KulService.SpeciesObservationPropertyId.IndividualCount).Value.WebParseInt32();
            observation.CountryCode = entity.Fields.First(p => p.Property.Id == KulService.SpeciesObservationPropertyId.CountryCode).Value;
            observation.AssociatedOccurrences = entity.Fields.First(p => p.Property.Id == KulService.SpeciesObservationPropertyId.AssociatedOccurrences).Value;
            
            return observation;
        }
    }
}
