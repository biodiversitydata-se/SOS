using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Managers.Interfaces;
using SOS.Administration.Api.Models.Ipt;
using SOS.Lib.Services.Interfaces;

namespace SOS.Administration.Api.Managers
{
    /// <summary>
    /// IPT manager
    /// </summary>
    public class IptManager : IIptManager
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger<IptManager> _logger;

        /// <summary>
        /// Get first index of string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="searchFrom"></param>
        /// <param name="searchFor"></param>
        /// <returns></returns>
        private int GetIndex(string text, int searchFrom, string searchFor)
        {
           return text?.IndexOf(searchFor, searchFrom) ?? -1;
        }

        /// <summary>
        /// Try to scrape item from string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="searchFrom"></param>
        /// <param name="startChar"></param>
        /// <param name="endChar"></param>
        /// <returns></returns>
        private (string Value, int CurrentIndex) GetValue(string text, int searchFrom, string startChar, string endChar)
        {
            var startIndex = GetIndex(text, searchFrom, startChar);

            if (startIndex == -1)
            {
                return (null, startIndex);
            }

            var endIndex = GetIndex(text, startIndex + 1, endChar);

            if (endIndex == -1)
            {
                return (null, startIndex);
            }

            var value = text.Substring(startIndex + 1, endIndex - startIndex - 1);

            return (value, endIndex + 1);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="logger"></param>
        public IptManager(IHttpClientService httpClientService,
            ILogger<IptManager> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IptResource>> GetResourcesAsync()
        {
            try
            {
                var resources = new List<IptResource>();
                await using var stream = await _httpClientService.GetFileStreamAsync(new Uri("https://www.gbif.se/ipt/"));
                using var reader = new StreamReader(stream);
                var html = await reader.ReadToEndAsync();

                var arrayStartIndex = GetIndex(html, 0, "var aDataSet");
                arrayStartIndex = GetIndex(html, arrayStartIndex, "[");
                var arrayText = GetValue(html, arrayStartIndex, "[", "];");
                arrayText.Value = arrayText.Value.Replace("<if>", "");

                var arrayItem = GetValue(arrayText.Value, 0, "[", "]");
                while (arrayItem.CurrentIndex > 0)
                {
                    var firstCol = GetValue(arrayItem.Value, 0, "'", "'");
                    var secondCol = GetValue(arrayItem.Value, firstCol.CurrentIndex, "\"", "\"");
                    var thirdCol = GetValue(arrayItem.Value, secondCol.CurrentIndex, "'", "'");
                    var fourthCol = GetValue(arrayItem.Value, thirdCol.CurrentIndex, "'", "'");
                    var fifthCol = GetValue(arrayItem.Value, fourthCol.CurrentIndex, "'", "'");
                    var sixthCol = GetValue(arrayItem.Value, fifthCol.CurrentIndex, "'", "'");
                    var seventhCol = GetValue(arrayItem.Value, sixthCol.CurrentIndex, "'", "'");
                    var eighthCol = GetValue(arrayItem.Value, seventhCol.CurrentIndex, "'", "'");
                    var ninthCol = GetValue(arrayItem.Value, eighthCol.CurrentIndex, "'", "'");

                    if (fourthCol.Value.Equals("Occurrence", StringComparison.CurrentCultureIgnoreCase) &&
                        fifthCol.Value.Equals("Specimen", StringComparison.CurrentCultureIgnoreCase))
                    {
                        resources.Add(new IptResource
                        {
                            Name = GetValue(secondCol.Value, 0, ">", "<").Value,
                            Organization = thirdCol.Value,
                            Type = fourthCol.Value,
                            SubType = fifthCol.Value,
                            Records = int.Parse(GetValue(sixthCol.Value, 0, ">", "<").Value.Replace(",", "")),
                            LastModified = DateTime.Parse(seventhCol.Value),
                            LastPublication = DateTime.Parse(eighthCol.Value),
                            NextPublication = ninthCol.Value.Length > 10 ? DateTime.Parse(ninthCol.Value) : null,
                        });
                    }

                    arrayItem = GetValue(arrayText.Value, arrayItem.CurrentIndex, "[", "]");
                }
                
                return resources;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get resources from IPT");
                throw;
            }
        }
    }
}
