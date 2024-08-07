﻿using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Processed.Observation;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace SOS.Export.UnitTests.TestHelpers.Factories
{
    public static class DarwinCoreObservationFactory
    {
        public static Observation CreateDefaultObservation()
        {
            var fileName = @"Resources/DefaultTestObservation.json";
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, fileName);
            var str = File.ReadAllText(filePath, Encoding.UTF8);

            var serializeOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
            serializeOptions.Converters.Add(new ObjectIdConverter());

            var observation = JsonSerializer.Deserialize<Observation>(str, serializeOptions);

            return observation;
        }
    }
}