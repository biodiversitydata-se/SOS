using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using SOS.Export.Extensions;
using SOS.Lib.Models.Processed.Observation;
using Xunit;

namespace SOS.Export.UnitTests.Extensions
{
    public class DarwinCoreExtensionsTests
    {
        [Fact]
        public void Enumeration_of_Emof_rows_should_never_be_null()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            List<ProcessedObservation> processedObservations = new List<ProcessedObservation>
            {
                new ProcessedObservation {Projects = null, Taxon = new ProcessedTaxon(), Occurrence = new ProcessedOccurrence {OccurrenceId = "1"}},
                new ProcessedObservation {Projects = new List<ProcessedProject>(), Taxon = new ProcessedTaxon(), Occurrence = new ProcessedOccurrence {OccurrenceId = "2"}},
                new ProcessedObservation {Projects = new List<ProcessedProject> { new ProcessedProject {ProjectParameters = null}}, Taxon = new ProcessedTaxon(), Occurrence = new ProcessedOccurrence {OccurrenceId = "3"}}
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var emofRows = processedObservations.ToExtendedMeasurementOrFactRows();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            emofRows.Should()
                .NotBeNull().And
                .BeEmpty();
        }

        [Fact]
        public void Enumeration_of_Emof_rows_with_three_valid_projectparameters_should_return_three_rows()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            List<ProcessedObservation> processedObservations = new List<ProcessedObservation>
            {
                new ProcessedObservation {Projects = null, Taxon = new ProcessedTaxon(), Occurrence = new ProcessedOccurrence {OccurrenceId = "1"}},
                new ProcessedObservation {Projects = new List<ProcessedProject>(), Taxon = new ProcessedTaxon(), Occurrence = new ProcessedOccurrence {OccurrenceId = "2"}},
                new ProcessedObservation {Projects = new List<ProcessedProject> {new ProcessedProject {ProjectParameters = null}}, Taxon = new ProcessedTaxon(), Occurrence = new ProcessedOccurrence {OccurrenceId = "3"}},
                new ProcessedObservation
                {
                    Projects = new List<ProcessedProject>
                    {
                        new ProcessedProject
                        {
                            ProjectParameters = new List<ProcessedProjectParameter>
                            {
                                new ProcessedProjectParameter {Name = "Row1"},
                                new ProcessedProjectParameter {Name = "Row2"}
                            }
                        }
                    },
                    Taxon = new ProcessedTaxon(),
                    Occurrence = new ProcessedOccurrence {OccurrenceId = "4"}
                },
                new ProcessedObservation
                {
                    Projects = new List<ProcessedProject>
                    {
                        new ProcessedProject
                        {
                            ProjectParameters = new List<ProcessedProjectParameter>
                            {
                                new ProcessedProjectParameter {Name = "Row3"}
                            }
                        }
                    },
                    Taxon = new ProcessedTaxon(),
                    Occurrence = new ProcessedOccurrence {OccurrenceId = "5"}
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var emofRows = processedObservations.ToExtendedMeasurementOrFactRows();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            emofRows.Count().Should().Be(3, "because there exist 3 project parameters");
        }

    }
}
