﻿using DwC_A.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace SOS.Import.LiveIntegrationTests.TestDataTools
{
    public class CreateDwcTermMappingTool
    {
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

        [Fact]
        public void CreateDwcMapperMapValueByTermSwitchStatements()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var fieldValues = GetFieldValues(typeof(Terms));
            var sb = new StringBuilder();
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
    }
}