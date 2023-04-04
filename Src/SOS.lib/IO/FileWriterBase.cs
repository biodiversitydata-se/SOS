using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Lib.IO
{
    public class FileWriterBase
    {
        protected readonly TelemetryClient _telemetry;
     
        protected FileWriterBase(TelemetryClient telemetry)
        {
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }
        /// <summary>
        /// Store filter in folder o zip
        /// </summary>
        /// <param name="temporaryZipExportFolderPath"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        protected async Task StoreFilterAsync(string temporaryZipExportFolderPath, SearchFilter filter)
        {
            try
            {
                await using var fileStream = File.Create(Path.Combine(temporaryZipExportFolderPath, "filter.json"));
                await using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                var serializeOptions = new JsonSerializerOptions { IgnoreNullValues = true };
                serializeOptions.Converters.Add(new JsonStringEnumConverter());

                var filterString = JsonSerializer.Serialize(filter, serializeOptions);
                await streamWriter.WriteAsync(filterString);
                streamWriter.Close();
                fileStream.Close();
            }
            catch
            {
                return;
            }
        }
    }
}
