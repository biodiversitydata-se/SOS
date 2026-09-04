namespace SOS.Status.Web.HttpClients;

public class IpifyClient
{
    private readonly HttpClient _httpClient;

    public IpifyClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetOutboundIpAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            string.Empty,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var outboundIp = await response.Content.ReadAsStringAsync(cancellationToken);
        return outboundIp.Trim();
    }
}
