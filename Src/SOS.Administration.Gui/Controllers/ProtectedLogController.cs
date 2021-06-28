using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NReco.Csv;
using SOS.Administration.Gui.Managers.Interfaces;
using SOS.Lib.Models.ApplicationInsights;

namespace SOS.Administration.Gui.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ProtectedLogController : ControllerBase
    {
        private IProtectedLogManager _protectedLogManager;
        private readonly ILogger<ApplicationInsightsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="protectedLogManager"></param>
        /// <param name="logger"></param>
        public ProtectedLogController(IProtectedLogManager protectedLogManager, ILogger<ApplicationInsightsController> logger)
        {
            _protectedLogManager = protectedLogManager ?? throw new ArgumentNullException(nameof(protectedLogManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ApiLogRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLogDataAsync([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            try
            {
                var logData = await _protectedLogManager.SearchAsync(from, to);

                var outputStream = new MemoryStream();
                var streamWriter = new StreamWriter(outputStream, Encoding.UTF8);

                var csvWriter = new CsvWriter(streamWriter, "\t");

                csvWriter.WriteField("Log date");
                csvWriter.WriteField("Application Identifier");
                csvWriter.WriteField("IP");
                csvWriter.WriteField("User Id");
                csvWriter.WriteField("User Name");
                csvWriter.WriteField("Observation date");
                csvWriter.WriteField("County");
                csvWriter.WriteField("Province");
                csvWriter.WriteField("Municipality");
                csvWriter.WriteField("Locality");
                csvWriter.WriteField("Latitude");
                csvWriter.WriteField("Longitude");
                csvWriter.WriteField("Taxon Id");
                csvWriter.WriteField("Taxon Name");
                csvWriter.WriteField("Taxon Scientific Name");
                csvWriter.WriteField("Taxon Protection Level");
                csvWriter.NextRecord();

                foreach (var logRow in logData)
                {
                    foreach (var obs in logRow.Observations)
                    {
                        csvWriter.WriteField(logRow.IssueDate.ToShortDateString());
                        csvWriter.WriteField(logRow.ApplicationIdentifier);
                        csvWriter.WriteField(logRow.Ip);
                        csvWriter.WriteField(logRow.UserId);
                        csvWriter.WriteField(logRow.User);
                        csvWriter.WriteField(obs.IssueDate.HasValue ? obs.IssueDate.Value.ToShortDateString() : null);
                        csvWriter.WriteField(obs.County);
                        csvWriter.WriteField(obs.Province);
                        csvWriter.WriteField(obs.Municipality);
                        csvWriter.WriteField(obs.Locality);
                        csvWriter.WriteField(obs.Latitude?.ToString());
                        csvWriter.WriteField(obs.Longitude?.ToString());
                        csvWriter.WriteField(obs.TaxonId?.ToString());
                        csvWriter.WriteField(obs.TaxonCommonName);
                        csvWriter.WriteField(obs.TaxonScientificName);
                        csvWriter.WriteField(obs.TaxonProtectionLevel?.ToString());
                        csvWriter.NextRecord();
                    }
                }
                await streamWriter.FlushAsync();
                outputStream.Position = 0;
                return File(outputStream, "application/octet-stream", "logdata.csv");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
