using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.IO.Excel
{
    public class GeoJsonFileWriter : FileWriterBase, IGeoJsonFileWriter
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IFileService _fileService;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly ILogger<GeoJsonFileWriter> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="logger"></param>
        public GeoJsonFileWriter(IProcessedObservationRepository processedObservationRepository,
            IFileService fileService,
            IVocabularyValueResolver vocabularyValueResolver, 
            ILogger<GeoJsonFileWriter> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                                    throw new ArgumentNullException(
                                                        nameof(processedObservationRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _vocabularyValueResolver = vocabularyValueResolver ??
                                       throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<string> CreateFileAync(SearchFilter filter, string exportPath, 
            string fileName, string culture, bool flatOut,
            IJobCancellationToken cancellationToken)
        {
            string temporaryZipExportFolderPath = null;

            try
            {
                temporaryZipExportFolderPath = Path.Combine(exportPath, fileName);
                if (!Directory.Exists(temporaryZipExportFolderPath))
                {
                    Directory.CreateDirectory(temporaryZipExportFolderPath);
                }
                await using var fileStream = File.Create(Path.Combine(temporaryZipExportFolderPath, "Observations.geojson"));
                await using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                await streamWriter.WriteAsync("{\"type\":\"FeatureCollection\", \"crs\":\"EPSG:4326\", \"features\":[");

                var scrollResult = await _processedObservationRepository.ScrollObservationsAsync(filter, null);

                var objectFlattenerHelper = new ObjectFlattenerHelper();
                while (scrollResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    var processedObservations = scrollResult.Records.ToArray();
                    
                    // Convert observations to DwC format.
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, culture, true);
                    var numberFormatInfo = new NumberFormatInfo {CurrencyDecimalSeparator = "."};
                    var firstFeature = true;
                    // Write occurrence rows to CSV file.
                    foreach (var observation in processedObservations)
                    {
                        await streamWriter.WriteAsync($"{(firstFeature ? "" : ",")} {{\"type\":\"Feature\", \"geometry\":{{\"type\":\"Point\", \"coordinates\":[{observation.Location.DecimalLongitude.Value.ToString("0.0#######", numberFormatInfo)},{observation.Location.DecimalLatitude.Value.ToString("0.0#######", numberFormatInfo)}]}}");
                        firstFeature = false;

                        var objectProperties = objectFlattenerHelper.Execute(observation);
                        if (objectProperties?.Any() ?? false)
                        {
                            await streamWriter.WriteAsync(", \"properties\":{");
                            var firstProperty = true;
                            var prevPropertyParts = new string[0];
                            var openObjects = 0;
                            foreach (var objectProperty in objectProperties.OrderBy(p => p.Key))
                            {
                                // Check if property is included (OutputFields empty = all)
                                if (!((!filter.OutputFields?.Any() ?? true) || filter.OutputFields.Contains(objectProperty.Key, StringComparer.CurrentCultureIgnoreCase)))
                                {
                                    continue;
                                }

                                // Split property parts to array
                                var propertyParts = objectProperty.Key.Split('.');

                                if (flatOut)
                                {
                                    // Join property parts in camel case
                                    var propertyName = string.Join('.', propertyParts.Select(pp => pp.ToCamelCase()));
                                    await streamWriter.WriteAsync($"{(firstProperty ? "" : ",")} \"{propertyName}\": \"{objectProperty.Value}\"");
                                    firstProperty = false;
                                    continue;
                                }

                                // Check if we have any open sub objects that should be closed
                                for (var i = 0; i < prevPropertyParts.Length -1; i++)
                                {
                                    if (openObjects == 0 || (i < propertyParts.Length - 1 &&
                                                             prevPropertyParts[i].Equals(propertyParts[i], StringComparison.CurrentCultureIgnoreCase)))
                                    {
                                        continue;
                                    }
                                    await streamWriter.WriteAsync("}");
                                    openObjects--;
                                }

                                for (var i = 0; i < propertyParts.Length; i++)
                                {
                                    var propertyPart = propertyParts[i];

                                    if (i == propertyParts.Length - 1)
                                    {
                                        // Last part, write value
                                        await streamWriter.WriteAsync($"{(firstProperty ? "" : ",")} \"{propertyPart.ToCamelCase()}\": \"{objectProperty.Value}\"");
                                        continue;
                                    }

                                    // Are we still in the same sub object? continue
                                    if (i < prevPropertyParts.Length - 1 && propertyParts[i].Equals(prevPropertyParts[i], StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        continue;
                                    }

                                    // Open new sub object
                                    await streamWriter.WriteAsync($"{(firstProperty ? "" : ",")} \"{propertyPart.ToCamelCase()}\": {{");
                                    openObjects++;
                                    firstProperty = true;
                                }

                                firstProperty = false;
                                prevPropertyParts = propertyParts;
                            }

                            // Close sub objects if any
                            while (openObjects > 0)
                            {
                                await streamWriter.WriteAsync("}");
                                openObjects--;
                            }

                            await streamWriter.WriteAsync("}");
                        }

                        await streamWriter.WriteAsync("}");
                    }
                    
                    // Get next batch of observations.
                    scrollResult = await _processedObservationRepository.ScrollObservationsAsync(filter, scrollResult.ScrollId);
                }
                await streamWriter.WriteAsync("] }");
                streamWriter.Close();
                fileStream.Close();

                await StoreFilterAsync(temporaryZipExportFolderPath, filter);

                var zipFilePath = _fileService.CompressFolder(exportPath, fileName);
                
                return zipFilePath;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create GeoJson File.");
                throw;
            }
            finally
            {
                _fileService.DeleteFolder(temporaryZipExportFolderPath);
            }
        }
    }
}
