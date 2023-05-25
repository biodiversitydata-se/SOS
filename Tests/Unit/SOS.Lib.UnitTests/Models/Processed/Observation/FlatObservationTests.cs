using System.Collections.Generic;
using AutoBogus;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models;
using SOS.Lib.Models.Processed.Observation;
using Xunit;

namespace SOS.Lib.UnitTests.Models.Processed.Observation
{
    public class FlatObservationTests
    {
        [Fact]
        public void Test_FlatObservation_Mappings()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            var propertyFields = ObservationPropertyFieldDescriptionHelper.FieldsByFieldSet[OutputFieldSet.All];
            var observation = AutoFaker.Generate<Lib.Models.Processed.Observation.Observation>();
            var flatObservation = new FlatObservation(observation);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var stringValueByProperty = new Dictionary<string, string>();
            var propertiesWithErrors = new List<PropertyFieldDescription>();
            foreach (var field in propertyFields)
            {
                try
                {
                    string stringValue = flatObservation.GetStringValue(field);
                    stringValueByProperty.Add(field.PropertyPath, stringValue);
                }
                catch 
                {
                    propertiesWithErrors.Add(field);
                }
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            //string mappingCode = string.Join(",\r\n", propertiesWithErrors.Select(m => $"\"{m.PropertyPath.ToLower()}\" => \"N/A\""));
            propertiesWithErrors.Should().BeEmpty();
            stringValueByProperty.Should().NotBeEmpty();
        }
    }
}