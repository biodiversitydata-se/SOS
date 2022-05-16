using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Services
{
    public class FileDownloadService : IFileDownloadService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger<FileDownloadService> _logger;

       /// <summary>
       /// Download file
       /// </summary>
       /// <param name="url"></param>
       /// <param name="acceptHeaderContentType"></param>
       /// <returns></returns>
        private async Task<Stream> GetFileStreamAsync(string url, string acceptHeaderContentType)
        {
            try
            {
               return await _httpClientService.GetFileStreamAsync(
                    new Uri(url),
                    new Dictionary<string, string>(new[]
                        {
                            new KeyValuePair<string, string>("Accept", acceptHeaderContentType),
                        }
                    )
                );
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to get file: { url }", e);
                return null;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="logger"></param>
        public FileDownloadService(
            IHttpClientService httpClientService,
            ILogger<FileDownloadService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<XDocument> GetXmlFileAsync(string url)
        {
            try
            {
                await using var xmlStream = await GetFileStreamAsync(url, "application/xml");
                if (!xmlStream?.CanRead ?? true)
                {
                    throw new NullReferenceException(nameof(xmlStream));
                }
                
                xmlStream.Seek(0, SeekOrigin.Begin);

                var xDocument = await XDocument.LoadAsync(xmlStream, LoadOptions.None, CancellationToken.None);
                xmlStream.Close();

                return xDocument;
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to get XML-file ({url})", e);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> GetFileAndStoreAsync(string url, string path, string acceptHeaderContentType = "application/xml")
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return false;
                }

                await using var fileStream = File.Create(path);
                if (!fileStream?.CanWrite ?? true)
                {
                    throw new NullReferenceException(nameof(fileStream));
                }

                await using var dwcAStream = await GetFileStreamAsync(url, acceptHeaderContentType);
                dwcAStream.Seek(0, SeekOrigin.Begin);
                dwcAStream.CopyTo(fileStream);
                dwcAStream.Close();
                fileStream.Close();
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}