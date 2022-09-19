namespace SOS.Blazor.Api.Extensions
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadFromJsonAsync<T>(this HttpContent httpContent)
        {
            return await httpContent.ReadFromJsonAsync<T>(options: new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            });
        }
    }
}
