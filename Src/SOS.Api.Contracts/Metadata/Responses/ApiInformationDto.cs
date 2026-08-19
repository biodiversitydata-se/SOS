namespace SOS.Api.Contracts.Metadata.Responses;

public sealed class ApiInformationDto
{
    public string? ApiName { get; set; }
    public string? ApiVersion { get; set; }
    public DateTimeOffset ApiReleased { get; set; }
    public Uri? ApiDocumentation { get; set; }
    public Uri? ApiChangelog { get; set; }
    public string? ApiStatus { get; set; }
}
