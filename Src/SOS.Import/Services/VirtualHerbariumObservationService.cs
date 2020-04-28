using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Services
{
    public class VirtualHerbariumObservationService : IVirtualHerbariumObservationService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly VirtualHerbariumServiceConfiguration _virtualHerbariumServiceConfiguration;
        private readonly ILogger<VirtualHerbariumObservationService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="virtualHerbariumServiceConfiguration"></param>
        public VirtualHerbariumObservationService(
            IHttpClientService httpClientService,
            VirtualHerbariumServiceConfiguration virtualHerbariumServiceConfiguration,
            ILogger<VirtualHerbariumObservationService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _virtualHerbariumServiceConfiguration = virtualHerbariumServiceConfiguration ?? throw new ArgumentNullException(nameof(virtualHerbariumServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<XDocument> GetAsync(DateTime from, int pageIndex, int pageSize)
        {
            var xmlString = string.Empty;
            try
            {
                await using var fileStream = await _httpClientService.GetFileStreamAsync(
                    new Uri(
                        $"{_virtualHerbariumServiceConfiguration.BaseAddress}/admin/lifewatche.php?datum={from}&page={pageIndex}&pagesize={pageSize}"),
                    new Dictionary<string, string>{{ "Accept", "application/xml" } });
                
                if (!fileStream?.CanRead ?? true)
                {
                    return null;
                }

                var enc = Encoding.UTF8;
                using var streamReader = new StreamReader(fileStream, enc, true);
                
                xmlString = await streamReader.ReadToEndAsync();
                fileStream.Close();

                // Dropping the BOM
                var _byteOrderMarkUtf8 = enc.GetString(enc.GetPreamble());
                if (xmlString.StartsWith(_byteOrderMarkUtf8))
                {
                    xmlString = xmlString.Remove(0, _byteOrderMarkUtf8.Length).Replace("&gt50", "");
                }

                var xDocument = XDocument.Parse(xmlString);

                return xDocument;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get data from Virtual Herbarium", e);
                return null;
            }
        }
    }
}

