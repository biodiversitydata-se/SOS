using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DwC_A.Terms;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateDwcTermMappingTool
    {
        [Fact]
        public void CreateDwcMapperMapValueByTermSwitchStatements()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var fieldValues = GetFieldValues(typeof(Terms));
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("switch (term)");
            sb.AppendLine("{");
            foreach (var pair in fieldValues)
            {
                sb.AppendLine($"   case \"{pair.Value}\":");
                sb.AppendLine($"        observation.{CapitalizeFirstChar(pair.Key)} = val;");
                sb.AppendLine("        break;");
            }

            sb.AppendLine("}");

            var result = sb.ToString();
        }

        private string CapitalizeFirstChar(string input)
        {
            try
            {
                return input.First().ToString().ToUpper() + input.Substring(1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static Dictionary<string, string> GetFieldValues(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string))
                .ToDictionary(f => f.Name,
                    f => (string)f.GetValue(null));
        }
    }
}
