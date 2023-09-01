using DwC_A;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NetTopologySuite.Features;
using Xunit;
using SOS.Lib.Models.Shared;

namespace SOS.Import.IntegrationTests.GeoJson
{
    public class ReadGeoJsonTests
    {
        [Fact]
        public async Task Compare_SHARK_verbatim_and_processed_data()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string filePath = @"<path>"; // add path to geojson file.
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
            var jsonReader = new JsonTextReader(streamReader);
            var serializer = GeoJsonSerializer.CreateDefault();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            FeatureCollection featureCollection = serializer.Deserialize<FeatureCollection>(jsonReader);
            List<string> ids = new List<string>();
            foreach (var feature in featureCollection)
            {
                string id = (string)feature.Attributes["Occurrence.OccurrenceId"];
                ids.Add(id);
            }

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            featureCollection.Count.Should().BeGreaterThan(1);
            var duplicates = ids.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();
            duplicates.Count.Should().Be(0);
        }
    }
}