using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// Search filter extensions
    /// </summary>
    public static class SearchFilterExtensions
    {
        /// <summary>
        /// Populate output fields based on property set
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="outputFieldSet"></param>
        public static void PopulateOutputFields(this SearchFilter filter, OutputFieldSet? outputFieldSet)
        {
            if (filter.OutputFields?.Any() == true && outputFieldSet == null) return;
            const OutputFieldSet defaultFieldSet = OutputFieldSet.Minimum;
            var fieldSet = outputFieldSet == null ? defaultFieldSet : (OutputFieldSet)outputFieldSet;
            if (fieldSet == OutputFieldSet.All)
            {
                filter.OutputFields = null;
                return;
            }

            var outputFields = new List<string>
            {
                "DatasetName",
                "Identification.Validated",
                "Identification.UncertainIdentification",
                "Location.County",
                "Location.Municipality",
                "Location.DecimalLongitude",
                "Location.DecimalLatitude",
                "Location.CoordinateUncertaintyInMeters",
                "Occurrence.OccurrenceStatus",
                "Occurrence.ReportedBy",
                "Occurrence.RecordedBy",
                "Taxon.Attributes.OrganismGroup",
                "Taxon.ScientificName",
                "Taxon.VernacularName"
            };

            if (fieldSet == OutputFieldSet.Extended)
            {
                outputFields.AddRange(new[]
                {
                    "CollectionCode",
                    "InstitutionCode",
                    "OwnerInstitutionCode",
                    "BasisOfRecord",
                    "Event.Habitat",
                    "Event.EventRemarks",
                    "Event.SamplingEffort",
                    "Event.SamplingProtocol",
                    "Event.SampleSizeUnit",
                    "Event.SampleSizeValue",
                    "Location.Locality",
                    "Location.Province.Name",
                    "Location.Parish.Name",
                    "Location.GeodeticDatum",
                    "Occurrence.ReportedBy",
                    "Occurrence.Url",
                    "Occurrence.AssociatedMedia",
                    "Occurrence.OccurrenceRemarks",
                    "Occurrence.Activity.Value",
                    "Occurrence.Behavior.Value",
                    "Occurrence.LifeStage.Value",
                    "Occurrence.ReproductiveCondition.Value",
                    "Occurrence.Sex.Value",
                    "Occurrence.Biotope.Value",
                    "Occurrence.BiotopeDescription",
                    "Occurrence.ProtectionLevel",
                    "Occurrence.IsNeverFoundObservation",
                    "Occurrence.IsNotRediscoveredObservation",
                    "Occurrence.IsNaturalOccurrence",
                    "Occurrence.IsPositiveObservation",
                    "Occurrence.Substrate.Name.Value",
                    "Occurrence.individualCount",
                    "Occurrence.OrganismQuantity",
                    "Occurrence.OrganismQuantityInt",
                    "Occurrence.OrganismQuantityUnit",
                    "Identification.ValidationStatus",
                    "Identification.ConfirmedBy",
                    "Identification.IdentifiedBy",
                    "Identification.VerifiedBy",
                    "Identification.DeterminationMethod.Value",
                    "Taxon.Kingdom",
                    "Taxon.Phylum",
                    "Taxon.Class",
                    "Taxon.Order",
                    "Taxon.Family",
                    "Taxon.Genus",
                    "Taxon.TaxonId",
                    "Taxon.Attributes.DyntaxaTaxonId",
                    "Taxon.Attributes.ProtectionLevel.Value",
                    "Taxon.Attributes.RedlistCategory",
                    "Taxon.Attributes.ProtectedByLaw",
                    "Project.Id",
                    "Project.Name"
                });
            }

            // Order fields by name
            outputFields = outputFields.OrderBy(s => s).ToList();

            // Make sure some selected fields occurs first
            outputFields.InsertRange(0, new[]
            {
                "Occurrence.OccurrenceId",
                "Event.StartDate",
                "Event.EndDate"
            });

            if (filter.OutputFields?.Any() ?? false)
            {
                outputFields.AddRange(filter.OutputFields.Where(of => !outputFields.Contains(of, StringComparer.CurrentCultureIgnoreCase)));
            }
           
            filter.OutputFields = outputFields;
        }
    }
}
