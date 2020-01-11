using System;
using System.Collections.Generic;
using System.Text;
using SOS.Export.Mappings;
using Xunit;

namespace SOS.Export.Test.Tools
{
    public class CreateCsvMappingSourceCodeTool
    {
        [Fact]
        [Trait("Category", "Tool")]
        public void CreateCsvMappingSourceCode()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dic = DarwinCoreDynamicMap.CreateFieldMappingDictionary();
            StringBuilder sb = new StringBuilder();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            foreach (var pair in dic)
            {
                sb.AppendLine($"Map({pair.Value})");
                sb.AppendLine($"\t.Name(mappingbyId[FieldDescriptionId.{pair.Key}].Name)");
                sb.AppendLine($"\t.Index(mappingbyId[FieldDescriptionId.{pair.Key}].Index)");
                sb.AppendLine($"\t.Ignore(mappingbyId[FieldDescriptionId.{pair.Key}].Ignore);");
                sb.AppendLine();
            }

            var sourceCode = sb.ToString();
        }
    }
}
