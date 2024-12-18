﻿using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Filters;
using System.Collections.Generic;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions
{
    public class SearchFilterExtensionsTests
    {
        /// <remarks>
        /// The assert may be necessary to be update when new fields are added to the data model.
        /// </remarks>>
        [Fact]
        public void Test_PopulateOutputFields_with_minimum_fieldset()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilter(0);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            searchFilter.Output.PopulateFields(OutputFieldSet.Minimum);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            searchFilter.Output.Fields.Should().BeEquivalentTo(
                "Occurrence.OccurrenceId",
                "Event.StartDate",
                "Event.EndDate",
                "DatasetName",
                "Identification.Verified",
                "Identification.UncertainIdentification",
                "Location.County",
                "Location.Municipality",
                "Location.DecimalLongitude",
                "Location.DecimalLatitude",
                "Location.Sweref99TmX",
                "Location.Sweref99TmY",
                "Location.CoordinateUncertaintyInMeters",
                "Occurrence.OccurrenceStatus",
                "Occurrence.ReportedBy",
                "Occurrence.RecordedBy",
                "Occurrence.IndividualCount",
                "Occurrence.OrganismQuantity",
                "Occurrence.OrganismQuantityInt",
                "Occurrence.OrganismQuantityUnit",
                "Taxon.Id",
                "Taxon.ScientificName",
                "Taxon.VernacularName",
                "Taxon.Attributes.OrganismGroup");
        }

        [Fact]
        public void Test_PopulateOutputFields_with_specific_fields()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilter(0)
            {
                Output = new OutputFilter
                {
                    Fields = new List<string> { "Occurrence.OccurrenceId", "Event.StartDate", "Location.Municipality" }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------

            searchFilter.Output.PopulateFields(null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            searchFilter.Output.Fields.Should().BeEquivalentTo(
                "Occurrence.OccurrenceId",
                "Event.StartDate",
                "Location.Municipality");
        }

        /// <remarks>
        /// The assert may be necessary to be update when new fields are added to the data model.
        /// </remarks>>
        [Fact]
        public void Test_PopulateOutputFields_with_minimum_fieldset_and_one_additional_unique_field()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilter(0)
            {
                Output = new OutputFilter
                {
                    Fields = new List<string> { "Occurrence.OccurrenceId", "Event.PlainStartTime" }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            searchFilter.Output.PopulateFields(OutputFieldSet.Minimum);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            searchFilter.Output.Fields.Should().BeEquivalentTo(
                "Occurrence.OccurrenceId",
                "Event.StartDate",
                "Event.EndDate",
                "DatasetName",
                "Identification.Verified",
                "Identification.UncertainIdentification",
                "Location.County",
                "Location.Municipality",
                "Location.DecimalLongitude",
                "Location.DecimalLatitude",
                "Location.Sweref99TmX",
                "Location.Sweref99TmY",
                "Location.CoordinateUncertaintyInMeters",
                "Occurrence.OccurrenceStatus",
                "Occurrence.ReportedBy",
                "Occurrence.RecordedBy",
                "Occurrence.IndividualCount",
                "Occurrence.OrganismQuantity",
                "Occurrence.OrganismQuantityInt",
                "Occurrence.OrganismQuantityUnit",
                "Taxon.Id",
                "Taxon.ScientificName",
                "Taxon.VernacularName",
                "Taxon.Attributes.OrganismGroup",
                "Event.PlainStartTime");
        }

        /// <remarks>
        /// The assert may be necessary to be update when new fields are added to the data model.
        /// </remarks>>
        [Fact]
        public void Test_PopulateOutputFields_with_extended_fieldset()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilter(0);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            searchFilter.Output.PopulateFields(OutputFieldSet.Extended);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            searchFilter.Output.Fields.Should().BeEquivalentTo(
                "Occurrence.OccurrenceId",
                "Event.StartDate",
                "Event.EndDate",
                "BasisOfRecord",
                "CollectionCode",
                "DataProviderId",
                "DatasetName",
                "Event.DiscoveryMethod",
                "Event.EventRemarks",
                "Event.Habitat",
                "Event.MeasurementOrFacts.MeasurementID",
                "Event.MeasurementOrFacts.MeasurementType",
                "Event.MeasurementOrFacts.MeasurementUnit",
                "Event.MeasurementOrFacts.MeasurementValue",
                "Event.PlainEndDate",
                "Event.PlainEndTime",
                "Event.PlainStartDate",
                "Event.PlainStartTime",
                "Event.SampleSizeUnit",
                "Event.SampleSizeValue",
                "Event.SamplingEffort",
                "Event.SamplingProtocol",
                "Identification.ConfirmedBy",
                "Identification.DeterminationMethod",
                "Identification.IdentifiedBy",
                "Identification.UncertainIdentification",
                "Identification.VerificationStatus",
                "Identification.Verified",
                "Identification.VerifiedBy",
                "InstitutionCode",
                "Location.CoordinateUncertaintyInMeters",
                "Location.County",
                "Location.DecimalLatitude",
                "Location.DecimalLongitude",
                "Location.Sweref99TmX",
                "Location.Sweref99TmY",
                "Location.GeodeticDatum",
                "Location.Locality",
                "Location.LocationId",
                "Location.Municipality",
                "Location.Parish",
                "Location.Province",
                "MeasurementOrFacts.MeasurementID",
                "MeasurementOrFacts.MeasurementType",
                "MeasurementOrFacts.MeasurementUnit",
                "MeasurementOrFacts.MeasurementValue",
                "Modified",
                "Occurrence.Activity",
                "Occurrence.AssociatedMedia",
                "Occurrence.Behavior",
                "Occurrence.Biotope",
                "Occurrence.BiotopeDescription",
                "Occurrence.IndividualCount",
                "Occurrence.IsNaturalOccurrence",
                "Occurrence.IsNeverFoundObservation",
                "Occurrence.IsNotRediscoveredObservation",
                "Occurrence.IsPositiveObservation",
                "Occurrence.Length",
                "Occurrence.LifeStage",
                "Occurrence.OccurrenceRemarks",
                "Occurrence.OccurrenceStatus",
                "Occurrence.OrganismQuantity",
                "Occurrence.OrganismQuantityInt",
                "Occurrence.OrganismQuantityUnit",
                "Occurrence.RecordedBy",
                "Occurrence.ReportedBy",
                "Occurrence.ReproductiveCondition",
                "Occurrence.SensitivityCategory",
                "Occurrence.Sex",
                "Occurrence.Substrate.Name",
                "Occurrence.Url",
                "Occurrence.Weight",
                "OwnerInstitutionCode",
                "Projects.Id",
                "Projects.Name",
                "Projects.Owner",
                "Projects.ProjectParameters",
                "RightsHolder",
                "Taxon.Attributes.IsInvasiveAccordingToEuRegulation",
                "Taxon.Attributes.IsInvasiveInSweden",
                "Taxon.Attributes.InvasiveRiskAssessmentCategory",
                "Taxon.Attributes.IsRedlisted",
                "Taxon.Attributes.OrganismGroup",
                "Taxon.Attributes.ProtectedByLaw",
                "Taxon.Attributes.RedlistCategory",
                "Taxon.Attributes.SensitivityCategory",
                "Taxon.Attributes.TaxonCategory",
                "Taxon.Class",
                "Taxon.Family",
                "Taxon.Genus",
                "Taxon.Id",
                "Taxon.Kingdom",
                "Taxon.Order",
                "Taxon.Phylum",
                "Taxon.ScientificName",
                "Taxon.TaxonId",
                "Taxon.VernacularName");
        }

        [Fact]
        public void Test_PopulateOutputFields_with_allWithValues_fieldset()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var searchFilter = new SearchFilter(0);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------

            searchFilter.Output.PopulateFields(OutputFieldSet.AllWithValues);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            searchFilter.Output.Fields.Should().BeNull();
        }
    }
}