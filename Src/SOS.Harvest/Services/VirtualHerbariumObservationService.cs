using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Services.Interfaces;

namespace SOS.Harvest.Services
{
    public class VirtualHerbariumObservationService : IVirtualHerbariumObservationService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger<VirtualHerbariumObservationService> _logger;
        private readonly VirtualHerbariumServiceConfiguration _virtualHerbariumServiceConfiguration;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="virtualHerbariumServiceConfiguration"></param>
        public VirtualHerbariumObservationService(
            IHttpClientService httpClientService,
            VirtualHerbariumServiceConfiguration virtualHerbariumServiceConfiguration,
            ILogger<VirtualHerbariumObservationService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _virtualHerbariumServiceConfiguration = virtualHerbariumServiceConfiguration ??
                                                    throw new ArgumentNullException(
                                                        nameof(virtualHerbariumServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<XDocument?> GetAsync(DateTime from, int pageIndex, int pageSize)
        {
            try
            {
                return await GetXDocuemnt($"admin/lifewatche.php?datum={from}&page={pageIndex}&pagesize={pageSize}");
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get data from Virtual Herbarium", e);
               throw;
            }
        }

        /// <inheritdoc />
        public async Task<XDocument?> GetLocalitiesAsync()
        {
            try
            {
                return await GetXDocuemnt("download_locality.php");
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get localities from Virtual Herbarium", e);
                throw;
            }
        }

        /// <summary>
        ///     Get XDocument from stream
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<XDocument?> GetXDocuemnt(string path)
        {
            await using var fileStream = await _httpClientService.GetFileStreamAsync(new Uri(
                    $"{_virtualHerbariumServiceConfiguration.BaseAddress}/{path}"),
                new Dictionary<string, string> {{"Accept", "application/xml"}});

            if (!fileStream?.CanRead ?? true)
            {
                return null;
            }

            var encoding = new UTF8Encoding(true, true);
            using var streamReader = new StreamReader(fileStream!, encoding, true);

            var xmlString = await streamReader.ReadToEndAsync();
            fileStream!.Close();

            var xDocument = XDocument.Parse(CleanXml(xmlString, encoding));

            return xDocument;
        }

        /// <summary>
        ///     Remove undesirable content from XML string
        /// </summary>
        /// <param name="xmlString">XML string.</param>
        /// <param name="encoding">string encoding</param>
        /// <returns>Same XML string with undesirable content removed.</returns>
        private string CleanXml(string xmlString, Encoding encoding)
        {
            // Dropping the BOM
          /*  var _byteOrderMarkUtf8 = encoding.GetString(encoding.GetPreamble());
            if (xmlString.StartsWith(_byteOrderMarkUtf8))
            {
                xmlString = xmlString.Remove(0, _byteOrderMarkUtf8.Length).Replace("&gt50", "").Replace("&", "");
            }*/

            // Replace bad data
            //  xmlString = xmlString.Replace("1920-0&-21", "1920-06-21");
            //  xmlString = xmlString.Replace("den 21/&amp;", "den 21/6");

            // Change wrong Excel XML encoding.
            //xmlString = xmlString.Replace("&", "&amp;");
            var regularExpression = @"&(?!amp;)";

            xmlString = Regex.Replace(xmlString, regularExpression, "&amp;");

            regularExpression = @"[\x00-\x1F]";
            return Regex.Replace(xmlString, regularExpression, string.Empty);
        }
    }
}