using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Import.Services
{
    public class TaxonServiceProxy : Interfaces.ITaxonServiceProxy
    {
        /// <summary>
        /// Gets a checklist DwC-A file from a web service.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<Stream> GetDwcaFileAsync(string url)
        {
            using var client = new HttpClient();
            using var result = await client.GetAsync(url);

            if (!result.IsSuccessStatusCode)
            {
                return null;
            }

            await using Stream zipFileContentStream = await result.Content.ReadAsStreamAsync();
            return zipFileContentStream;
        }
    }
}
