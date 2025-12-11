namespace SOS.Observations.Api.IntegrationTests.Extensions;

internal static class HttpResponseMessageExtensions
{
    extension(HttpResponseMessage response)
    {
        public string GetHeaderValue(string headerName)
        {
            if (!response.Headers.Contains(headerName))
            {
                throw new Xunit.Sdk.XunitException($"Response does not contain header '{headerName}'.");
            }

            return response.Headers.GetValues(headerName).FirstOrDefault();
        }
    }
}
