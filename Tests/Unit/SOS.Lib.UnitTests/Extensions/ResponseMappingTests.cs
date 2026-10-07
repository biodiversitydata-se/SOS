using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Converters;
using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Api.Contracts.Lookups.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Extensions.Dto.DataStewardship;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models;
using SOS.Lib.Models.Processed.DataStewardship.Enums;
using SOS.Lib.Models.Processed.Observation;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Lib.UnitTests.Extensions;

public class ResponseMappingTests
{
    [Fact]
    public void ToLocationDto_PreservesGeoJsonCoordinatesAndDistinctBuffers()
    {
        var location = new Location
        {
            LocationId = "test:location",
            Point = new Point(new CoordinateZ(18, 59, 120.5)),
            PointWithBuffer = new WKTReader().Read("POLYGON ((16 60, 19 60, 19 63, 16 60))"),
            PointWithDisturbanceBuffer = new WKTReader().Read("POLYGON ((15 59, 20 59, 20 64, 15 59))")
        };

        var dto = location.ToLocationDto();

        dto.LocationId.Should().Be(location.LocationId);
        var expectedPoint = JsonNode.Parse("""{"type":"Point","coordinates":[18,59,120.5]}""");
        var expectedBuffer = JsonNode.Parse("""{"type":"Polygon","coordinates":[[[16,60],[19,60],[19,63],[16,60]]]}""");
        var expectedDisturbanceBuffer = JsonNode.Parse("""{"type":"Polygon","coordinates":[[[15,59],[20,59],[20,64],[15,59]]]}""");
        JsonNode.DeepEquals(expectedPoint, JsonSerializer.SerializeToNode(dto.Point)).Should().BeTrue();
        JsonNode.DeepEquals(expectedBuffer, JsonSerializer.SerializeToNode(dto.PointWithBuffer)).Should().BeTrue();
        JsonNode.DeepEquals(expectedDisturbanceBuffer, JsonSerializer.SerializeToNode(dto.PointWithDisturbanceBuffer)).Should().BeTrue();
    }

    [Fact]
    public void ToLocationDto_PreservesNullGeometries()
    {
        var dto = new Location().ToLocationDto();

        dto.Point.Should().BeNull();
        dto.PointWithBuffer.Should().BeNull();
        dto.PointWithDisturbanceBuffer.Should().BeNull();
    }

    [Fact]
    public void ToLocationDto_PreservesNullLocation()
    {
        Location? location = null;

        location.ToLocationDto().Should().BeNull();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(ProjectType.Artportalenprojekt, DsProjectType.Artportalenprojekt)]
    [InlineData(ProjectType.Delprogram, DsProjectType.Delprogram)]
    [InlineData(ProjectType.Delsystem, DsProjectType.Delsystem)]
    [InlineData(ProjectType.GemensamtDelprogram, DsProjectType.GemensamtDelprogram)]
    public void ProjectToDto_PreservesDataStewardshipFields(ProjectType? projectType, DsProjectType? expectedType)
    {
        var project = new SOS.Lib.Models.Processed.DataStewardship.Common.Project
        {
            ProjectId = "project:1",
            ProjectCode = "Monitoring",
            ProjectType = projectType
        };

        var dto = project.ToDto();

        dto.ProjectID.Should().Be(project.ProjectId);
        dto.ProjectCode.Should().Be(project.ProjectCode);
        dto.ProjectType.Should().Be(expectedType);
    }

    [Fact]
    public void ProjectToDto_PreservesNullForBothProjectModels()
    {
        SOS.Lib.Models.Processed.DataStewardship.Common.Project? project = null;
        ProjectInfo? projectInfo = null;

        project.ToDto().Should().BeNull();
        projectInfo.ToDto().Should().BeNull();
    }

    [Fact]
    public void TaxonToDto_PreservesDataStewardshipFields()
    {
        var taxon = new Taxon
        {
            Id = 103040,
            ScientificName = "Test species",
            TaxonRank = "species",
            VernacularName = "Test name",
            VerbatimId = "source:taxon",
            VerbatimName = "Source name"
        };

        var dto = taxon.ToDto();

        dto.TaxonID.Should().Be("103040");
        dto.ScientificName.Should().Be(taxon.ScientificName);
        dto.TaxonRank.Should().Be(taxon.TaxonRank);
        dto.VernacularName.Should().Be(taxon.VernacularName);
        dto.VerbatimTaxonID.Should().Be(taxon.VerbatimId);
        dto.VerbatimName.Should().Be(taxon.VerbatimName);
    }

    [Theory]
    [InlineData(18.123456789, 59.123456789, null, 4326)]
    [InlineData(18.123456789, 59.123456789, 120.5, 4326)]
    [InlineData(674032.1, 6580821.2, null, 3006)]
    public void ToGeoJsonPoint_PreservesServerJsonAndCoordinates(double x, double y, double? z, int srid)
    {
        var point = new Point(z.HasValue ? new CoordinateZ(x, y, z.Value) : new Coordinate(x, y));
        point.SRID = srid;
        var serverOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        serverOptions.Converters.Add(new GeoJsonConverterFactory());
        var expected = JsonSerializer.SerializeToNode(point, serverOptions);

        var geoJsonPoint = point.ToGeoJsonPoint();
        var json = JsonSerializer.Serialize(geoJsonPoint, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var restored = JsonSerializer.Deserialize<GeoJSON.Text.Geometry.Point>(json)!;

        JsonNode.DeepEquals(expected, JsonNode.Parse(json)).Should().BeTrue();
        restored.Coordinates.Longitude.Should().Be(x);
        restored.Coordinates.Latitude.Should().Be(y);
        restored.Coordinates.Altitude.Should().Be(z);
    }

    [Fact]
    public void ToGeoJsonPoint_PreservesNull()
    {
        Point? point = null;
        point.ToGeoJsonPoint().Should().BeNull();
    }

    [Fact]
    public void OccurrenceCsv_PreservesLongitudeLatitudeOrder()
    {
        var occurrence = new DsOccurrenceDto
        {
            OccurrenceID = "test:occurrence",
            ObservationPoint = new Point(18, 59).ToGeoJsonPoint()!
        };

        var csv = Encoding.UTF8.GetString(occurrence.ToCsv());

        csv.Should().Contain("18/59");
        csv.Should().NotContain("59/18");
    }

    [Fact]
    public void PropertyFieldMapping_PreservesAllEnumNamesAndValues()
    {
        foreach (var dataType in Enum.GetValues<PropertyFieldDataType>())
        {
            var field = new PropertyFieldDescription
            {
                DataTypeEnum = dataType,
                FieldSetEnum = OutputFieldSet.Minimum,
                FieldSets = Enum.GetValues<OutputFieldSet>().ToList()
            };

            var dto = field.ToPropertyFieldDescriptionDto();

            dto.DataType.Should().Be(dataType);
            dto.FieldSet.Should().Be(OutputFieldSet.Minimum);
            dto.PartOfFieldSets.Select(value => (int)value).Should().Equal(field.FieldSets.Select(value => (int)value));
            dto.PartOfFieldSets.Select(value => value.ToString()).Should().Equal(field.FieldSets.Select(value => value.ToString()));
        }
    }
}
