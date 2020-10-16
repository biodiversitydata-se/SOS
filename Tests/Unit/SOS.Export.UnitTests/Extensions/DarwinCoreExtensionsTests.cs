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
            List<Observation> processedObservations = new List<Observation>
            {
                new Observation {Projects = null, Taxon = new Taxon(), Occurrence = new Occurrence {OccurrenceId = "1"}},
                new Observation {Projects = new List<Project>(), Taxon = new Taxon(), Occurrence = new Occurrence {OccurrenceId = "2"}},
                new Observation {Projects = new List<Project> { new Project {ProjectParameters = null}}, Taxon = new Taxon(), Occurrence = new Occurrence {OccurrenceId = "3"}}
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
            List<Observation> processedObservations = new List<Observation>
            {
                new Observation {Projects = null, Taxon = new Taxon(), Occurrence = new Occurrence {OccurrenceId = "1"}},
                new Observation {Projects = new List<Project>(), Taxon = new Taxon(), Occurrence = new Occurrence {OccurrenceId = "2"}},
                new Observation {Projects = new List<Project> {new Project {ProjectParameters = null}}, Taxon = new Taxon(), Occurrence = new Occurrence {OccurrenceId = "3"}},
                new Observation
                {
                    Projects = new List<Project>
                    {
                        new Project
                        {
                            ProjectParameters = new List<ProjectParameter>
                            {
                                new ProjectParameter {Name = "Row1"},
                                new ProjectParameter {Name = "Row2"}
                            }
                        }
                    },
                    Taxon = new Taxon(),
                    Occurrence = new Occurrence {OccurrenceId = "4"}
                },
                new Observation
                {
                    Projects = new List<Project>
                    {
                        new Project
                        {
                            ProjectParameters = new List<ProjectParameter>
                            {
                                new ProjectParameter {Name = "Row3"}
                            }
                        }
                    },
                    Taxon = new Taxon(),
                    Occurrence = new Occurrence {OccurrenceId = "5"}
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
