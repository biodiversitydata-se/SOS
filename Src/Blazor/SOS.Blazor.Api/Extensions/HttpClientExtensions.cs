public static class HttpClientExtensions
{
    internal static HttpClient AddBearerToken(this HttpClient client, string value)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);
        return client;
    }
}