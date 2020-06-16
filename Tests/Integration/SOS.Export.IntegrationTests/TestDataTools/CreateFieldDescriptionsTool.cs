using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using SOS.Export.Mappings;
using SOS.Export.Models;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using Xunit;

namespace SOS.Export.IntegrationTests.TestDataTools
{
    public class CreateFieldDescriptionsTool
    {
        [Fact]
        [Trait("Category", "Tool")]
        public void CreateWriteCsvFieldsSourceCode()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dic = DarwinCoreDynamicMap.CreateFieldMappingDictionary();
            var sb = new StringBuilder();
            var fieldDescriptions = FieldDescriptionHelper.GetAllFieldDescriptions();
            var dwcFieldDescriptions = FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions();
            var orderedDictionary = dic.OrderBy(f => fieldDescriptions.First(d => d.FieldDescriptionId == f.Key).Id);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            foreach (var pair in orderedDictionary)
            {
                sb.AppendLine($"if (writeField[(int)FieldDescriptionId.{pair.Key}]) csvWriter.WriteField(dwcObservation.{pair.Value.ToString().Replace("m => m.","")});");
            }
            var sourceCode = sb.ToString();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            sourceCode.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Category", "Tool")]
        public void CreateFieldDescriptionsJsonFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            List<FieldDescription> fieldDescriptions = new List<FieldDescription>();
            fieldDescriptions.Add(new FieldDescription
            {
                Class = "Occurrence",
                DwcIdentifier = "http://rs.tdwg.org/dwc/terms/occurrenceID",
                Importance = 1,
                IncludedByDefaultInDwcExport = true,
                Name = "occurrenceID",
                IsDwC = true
            });
            // todo - add all other fields.

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            // todo - create the JSON file.
        }

    }
}
