﻿using MvmService;
using SOS.Harvest.Extensions;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Models.Verbatim.Mvm;

namespace SOS.Harvest.Harvesters.Mvm
{
    public class MvmHarvestFactory : HarvestBaseFactory, IHarvestFactory<IEnumerable<WebSpeciesObservation>, MvmObservationVerbatim>
    {
        /// <inheritdoc />
        public async Task<IEnumerable<MvmObservationVerbatim>?> CastEntitiesToVerbatimsAsync(IEnumerable<WebSpeciesObservation> entities)
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
        private MvmObservationVerbatim? CastEntityToVerbatim(WebSpeciesObservation entity)
        {
            if (entity == null)
            {
                return null;
            };

            var observation = new MvmObservationVerbatim
            {
                Id = NextId
            };

            for (var i = 0; i < entity.Fields.Length; i++)
            {
                var field = entity.Fields[i];

                switch (field.Property.Id)
                {
                    case SpeciesObservationPropertyId.ReportedBy:
                        observation.ReportedBy = field.Value;
                        break;
                    case SpeciesObservationPropertyId.Modified:
                        if (DateTime.TryParse(field.Value, out var modified))
                        {
                            observation.Modified = modified;
                        }
                        break;
                    case SpeciesObservationPropertyId.Owner:
                        observation.Owner = field.Value;
                        break;
                    case SpeciesObservationPropertyId.IndividualID:
                        observation.IndividualId = field.Value;
                        break;
                    case SpeciesObservationPropertyId.RecordedBy:
                        observation.RecordedBy = field.Value;
                        break;
                    case SpeciesObservationPropertyId.OccurrenceID:
                        observation.OccurrenceId = field.Value;
                        break;
                    case SpeciesObservationPropertyId.DecimalLongitude:
                        observation.DecimalLongitude = field.Value.WebParseDouble();
                        break;
                    case SpeciesObservationPropertyId.DecimalLatitude:
                        observation.DecimalLatitude = field.Value.WebParseDouble();
                        break;
                    case SpeciesObservationPropertyId.CoordinateUncertaintyInMeters:
                        observation.CoordinateUncertaintyInMeters = field.Value?.WebParseInt32();
                        break;
                    case SpeciesObservationPropertyId.Start:
                        observation.Start = field.Value.WebParseDateTime();
                        break;
                    case SpeciesObservationPropertyId.End:
                        observation.End = field.Value.WebParseDateTime();
                        break;
                    case SpeciesObservationPropertyId.ScientificName:
                        observation.ScientificName = field.Value;
                        break;
                    case SpeciesObservationPropertyId.DyntaxaTaxonID:
                        observation.DyntaxaTaxonId = field.Value.WebParseInt32();
                        break;
                    case SpeciesObservationPropertyId.Municipality:
                        observation.Municipality = field.Value;
                        break;
                    case SpeciesObservationPropertyId.County:
                        observation.County = field.Value;
                        break;
                    case SpeciesObservationPropertyId.Locality:
                        observation.Locality = field.Value;
                        break;
                    case SpeciesObservationPropertyId.LocationId:
                        observation.LocationId = field.Value;
                        break;
                    case SpeciesObservationPropertyId.DynamicProperties:
                        observation.ProductName = field.Value;
                        break;
                    case SpeciesObservationPropertyId.CatalogNumber:
                        observation.CatalogNumber = field.Value;
                        break;
                    case SpeciesObservationPropertyId.OccurrenceStatus:
                        observation.OccurrenceStatus = field.Value;
                        break;
                    case SpeciesObservationPropertyId.IsPositiveObservation:
                        observation.IsPositiveObservation = field.Value.WebParseBoolean();
                        break;
                    case SpeciesObservationPropertyId.IndividualCount:
                        observation.IndividualCount = field.Value;
                        break;
                    case SpeciesObservationPropertyId.LocationRemarks:
                        observation.LocationRemarks = field.Value;
                        break;
                    case SpeciesObservationPropertyId.Habitat:
                        observation.Habitat = field.Value;
                        break;
                    case SpeciesObservationPropertyId.ReportedDate:
                        observation.ReportedDate = field.Value.WebParseDateTime();
                        break;
                    case SpeciesObservationPropertyId.ProjectID:
                        observation.ProjectID = field.Value.WebParseInt32();
                        break;
                    case SpeciesObservationPropertyId.ProjectName:
                        observation.ProductName = field.Value;
                        break;
                    case SpeciesObservationPropertyId.IsPublic:
                        observation.IsPublic = field.Value.WebParseBoolean();
                        break;
                    case SpeciesObservationPropertyId.Quantity:
                        observation.Quantity = field.Value;
                        break;
                    case SpeciesObservationPropertyId.QuantityUnit:
                        observation.QuantityUnit = field.Value;
                        break;
                    default:
                        var unhandled = field.Property.Id;
                        break;
                }
            }

            return observation;
        }
    }
}
