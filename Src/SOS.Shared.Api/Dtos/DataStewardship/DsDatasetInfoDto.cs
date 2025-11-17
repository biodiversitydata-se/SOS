using Swashbuckle.AspNetCore.Annotations;

namespace SOS.Shared.Api.Dtos.DataStewardship;

[SwaggerSchema("Dataset info")]
public class DsDatasetInfoDto
{
    [SwaggerSchema("Identifier")]
    public string Identifier { get; set; }

    [SwaggerSchema("Title")]
    public string Title { get; set; }
}

