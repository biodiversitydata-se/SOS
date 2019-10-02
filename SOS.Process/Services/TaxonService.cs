using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SOS.Process.Configuration;
using SOS.Process.Models.Processed;

namespace SOS.Process.Services
{
    public class TaxonService : Interfaces.ITaxonService
    {
        private readonly string _taxonDwcUrl;

        public TaxonService(IOptions<AppSettings> settings)
        {
            if (settings?.Value == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _taxonDwcUrl = settings.Value.TaxonDwcUrl;
        }
        /// <inheritdoc />
        public async Task<IEnumerable<DarwinCoreTaxon>> GetTaxaAsync()
        {
            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(_taxonDwcUrl))
                {
                    if (!result.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    using (var zipFileContentStream = await result.Content.ReadAsStreamAsync())
                    {
                        using (var zipArchive = new ZipArchive(zipFileContentStream, ZipArchiveMode.Read, false))
                        {
                            foreach (var file in zipArchive.Entries)
                            {
                                var x = file.Name;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
