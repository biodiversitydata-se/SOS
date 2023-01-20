using System.Text.Json;
using SOS.DataStewardship.Api.IntegrationTests.Extensions;

namespace SOS.DataStewardship.Api.IntegrationTests.Extensions;

public static class HttpClientJsonExtensions
{
    public static async Task<TResult> PostAsJsonAsync<TResult, TBody>(
        this HttpClient client,
        string requestUri,
        TBody value,
        JsonSerializerOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var postResponse = await client.PostAsJsonAsync(requestUri, value, options, cancellationToken);
        var result = await postResponse.Content.ReadFromJsonAsync<TResult>(options);
        return result;
    }
}