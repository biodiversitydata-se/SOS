
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Xml.Linq;
using FluentAssertions;
using SOS.Api.Contracts.DataStewardship.Responses;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class DtoDocumentationTests
{
    [Fact]
    public void EveryPublicTypePropertyAndEnumValue_HasAnXmlSummary()
    {
        var assembly = typeof(DsDatasetDto).Assembly;
        var documentedMembers = ReadDocumentedMembers(assembly);
        var types = assembly.GetExportedTypes();
        var typeMembers = types.Select(type => $"T:{type.FullName!.Replace('+', '.')}");
        var propertyMembers = types.SelectMany(type => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(property => $"P:{type.FullName!.Replace('+', '.')}.{property.Name}"));
        var enumMembers = types.Where(type => type.IsEnum).SelectMany(type => type
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => $"F:{type.FullName!.Replace('+', '.')}.{field.Name}"));

        typeMembers.Concat(propertyMembers).Concat(enumMembers)
            .Where(member => !documentedMembers.Contains(member))
            .ToArray()
            .Should().BeEmpty("API consumers need descriptions of public types, properties and enum values");
    }

    [Fact]
    public void Contracts_DoNotDependOnServerLibraries()
    {
        var forbidden = new[] { "SOS.Lib", "NetTopologySuite", "MongoDB", "Swashbuckle" };
        typeof(DsDatasetDto).Assembly.GetReferencedAssemblies()
            .Select(assembly => assembly.Name!)
            .Where(name => forbidden.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
            .Should().BeEmpty("API DTOs must not depend on server implementation libraries");
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("SLU", true)]
    public void OrganisationCode_RequiresANonBlankValue(string? code, bool expectedValid)
    {
        var organisation = new DsOrganisationDto { OrganisationCode = code! };
        var errors = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(organisation, new ValidationContext(organisation), errors, validateAllProperties: true);

        valid.Should().Be(expectedValid);
        if (!expectedValid)
            errors.Should().ContainSingle(error => error.MemberNames.Contains(nameof(DsOrganisationDto.OrganisationCode)));
    }

    private static HashSet<string> ReadDocumentedMembers(Assembly assembly)
    {
        var path = Path.ChangeExtension(assembly.Location, ".xml");
        return XDocument.Load(path).Descendants("member")
            .Where(member => !string.IsNullOrWhiteSpace(member.Element("summary")?.Value))
            .Select(member => member.Attribute("name")?.Value)
            .Where(name => name is not null)
            .Cast<string>()
            .ToHashSet();
    }

}
