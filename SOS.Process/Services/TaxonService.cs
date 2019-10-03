using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using CsvHelper;
using Microsoft.Extensions.Logging;
using SOS.Process.Configuration;
using SOS.Process.Mappings;
using SOS.Process.Models.Processed;

namespace SOS.Process.Services
{
    public class TaxonService : Interfaces.ITaxonService
    {
        private readonly string _taxonDwcUrl;
        private readonly ILogger<TaxonService> _logger;

        public TaxonService(IOptions<AppSettings> settings, ILogger<TaxonService> logger)
        {
            if (settings?.Value == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _taxonDwcUrl = settings.Value.TaxonDwcUrl;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        /// <inheritdoc />
        public async Task<IEnumerable<DarwinCoreTaxon>> GetTaxaAsync()
        {
            try
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
                                var delimiter = "\t";

                                // try to get meta data file
                                var metaFile = zipArchive.Entries.FirstOrDefault(f =>
                                    f.Name.Equals("meta.xml", StringComparison.CurrentCultureIgnoreCase));

                                // If we found the meta data file, try to get some settings from it
                                if (metaFile != null)
                                {
                                    using (var reader = XmlReader.Create(metaFile.Open()))
                                    {
                                        var xmlDoc = XDocument.Load(reader);
                                        delimiter = xmlDoc.Root?.Element("archive")?.Element("core")?.Attribute("fieldsTerminatedBy")?.Value ?? delimiter;
                                    }
                                }

                                // Try to get the taxon data file
                                var taxonFile = zipArchive.Entries.FirstOrDefault(f =>
                                    f.Name.Equals("taxon.csv", StringComparison.CurrentCultureIgnoreCase));

                                // If no taxon data file found, we can't do anything more
                                if (taxonFile == null)
                                {
                                    return null;
                                }

                                // Read taxon data
                                using (var reader = new StreamReader(taxonFile.Open(), Encoding.UTF8))
                                {
                                    using (var csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration
                                    {
                                        Delimiter = delimiter,
                                        Encoding = Encoding.UTF8,
                                        HasHeaderRecord = true
                                    }))
                                    {
                                        // Get all taxa from file
                                        csv.Configuration.RegisterClassMap<TaxonMapper>();
                                        var taxa = csv.GetRecords<DarwinCoreTaxon>();

                                        return taxa.ToArray();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
           
            return null;
        }
    }
}
