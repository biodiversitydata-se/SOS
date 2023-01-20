using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using SOS.Harvest.Services.Taxon.Interfaces;

namespace SOS.Harvest.Services.Taxon
{
    public class TaxonServiceProxy : ITaxonServiceProxy
    {
        /// <summary>
        ///     Gets a checklist DwC-A file from a web service.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<Stream> GetDwcaFileAsync(string url)
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(300);
            using var result = await client.GetAsync(url);

            if (!result.IsSuccessStatusCode)
            {
                return null!;
            }

            var bytes = await result.Content.ReadAsByteArrayAsync();
            return new MemoryStream(bytes);
        }

        public async Task<string> GetTaxonAsync(string url, IEnumerable<int> taxonIds)
        {
            using var client = new HttpClient();
            var body = new GetTaxonBody();
            body.TaxonIds = taxonIds;
            var filterJson = JsonConvert.SerializeObject(body);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var postBody = new StringContent(filterJson, Encoding.UTF8, "application/json");

            using var result = await client.PostAsync(url, postBody);

            if (!result.IsSuccessStatusCode)
            {
                return null;
            }

            return await result.Content.ReadAsStringAsync();
        }

        private class GetTaxonBody
        {
            public IEnumerable<int> TaxonIds { get; set; }
        }
    }
}