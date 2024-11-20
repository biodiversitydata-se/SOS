using SOS.DataStewardship.Api.Contracts.Enums;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Tools;
public class CreateRegionsMarkdownTool
{
    [Fact(Skip = "Intended to run on demand")]    
    [Trait("Category", "DataUtil")]
    public void Create_regions_markdown()
    {                
        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var countyEnums = GetEnumInfos(typeof(County));
        var provinceEnums = GetEnumInfos(typeof(Province));
        var municipalityEnums = GetEnumInfos(typeof(Municipality));        
        var parishEnums = GetEnumInfos(typeof(Parish));

        var countyMarkdown = CreateMarkdown(countyEnums.OrderBy(m => m.Title));
        var provinceMarkdown = CreateMarkdown(provinceEnums.OrderBy(m => m.Title));
        var municipalityMarkdown = CreateMarkdown(municipalityEnums.OrderBy(m => m.Title));
        var parishMarkdown = CreateMarkdown(parishEnums.OrderBy(m => m.Title));

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        countyMarkdown.Should().NotBeEmpty();        
    }

    private List<EnumInfo> GetEnumInfos(Type enumType)
    {        
        List<EnumInfo> enumInfos = new List<EnumInfo>();
        FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (FieldInfo field in fields)
        {
            //object enumValue = field.GetValue(null)!; // null används för att hämta statiska fält
            string name = field.Name;
            string enumMemberValue = GetEnumMemberValue(field);

            enumInfos.Add(new EnumInfo
            {
                Title = enumMemberValue,
                EnumValue = name
            });            
        }

        return enumInfos;
    }

    static string GetEnumMemberValue(FieldInfo field)
    {
        EnumMemberAttribute enumMemberAttribute = field.GetCustomAttribute<EnumMemberAttribute>();
        if (enumMemberAttribute != null)
        {
            return enumMemberAttribute.Value;
        }
        return null;
    }

    private class EnumInfo
    {
        public string Title { get; set; }
        public string EnumValue { get; set; }
    }


    private string CreateMarkdown(IEnumerable<EnumInfo> enumInfos)
    {
        var sb = new StringBuilder();
        sb.AppendLine("| Title 	| Enum value |");
        sb.AppendLine("|---	|--- |");
        foreach (var enumInfo in enumInfos)
        {
            sb.AppendLine($"| {enumInfo.Title} | {enumInfo.EnumValue} |");
        }        

        return sb.ToString();
    }
}
