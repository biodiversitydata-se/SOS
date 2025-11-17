namespace SOS.DataStewardship.Api.IntegrationTests.Core.Extensions;

public static class HttpClientJsonExtensions
{
    extension(HttpClient client)
    {
        public async Task<TResult> PostAndReturnAsJsonAsync<TResult, TBody>(
        string requestUri,
        TBody value,
        JsonSerializerOptions options = null!,
        CancellationToken cancellationToken = default)
        {
            var postResponse = await client.PostAsJsonAsync(requestUri, value, options, cancellationToken);
            var result = await postResponse.Content.ReadFromJsonAsync<TResult>(options);
            return result!;
        }
    }
}