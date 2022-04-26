using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class ObjectHelperTests
    {
        [Fact]
        public void TestIdentifyGarbageChars()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var objectHelper = new ObjectHelper();
            var observation = new Observation
            {
                Projects = new List<Project>
                {
                    new Project
                    {
                        ProjectParameters = new List<ProjectParameter>
                        {
                            new ProjectParameter
                            {
                                Value = "test\twith tab"
                            }
                        }
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = objectHelper.GetPropertiesWithGarbageChars(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.First().Should().Be("Projects.ProjectParameters.Value", "because that property contains a tab character");
        }
    }
}