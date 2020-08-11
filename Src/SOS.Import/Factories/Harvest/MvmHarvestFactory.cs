using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvmService;
using SOS.Import.Extensions;
using SOS.Import.Factories.Harvest.Interfaces;
using SOS.Lib.Models.Verbatim.Mvm;

namespace SOS.Import.Factories.Harvest
{
    public class MvmHarvestFactory : IHarvestFactory<IEnumerable<WebSpeciesObservation>, MvmObservationVerbatim>
    {
        /// <inheritdoc />
        public async Task<IEnumerable<MvmObservationVerbatim>> CastEntitiesToVerbatimsAsync(IEnumerable<WebSpeciesObservation> entities, bool incrementalHarvest = false)
        {
            return await Task.Run(() =>
            {
                return
                    from e in entities
                    select CastEntityToVerbatim(e);
            });
        }

        /// <summary>
        ///     Cast sighting itemEntity to model .
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private MvmObservationVerbatim CastEntityToVerbatim(WebSpeciesObservation entity)
        {
            var observation = new MvmObservationVerbatim();
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
