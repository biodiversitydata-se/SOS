using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using SOS.Export.Extensions;
using SOS.Export.Mappings;
using SOS.Export.Models.DarwinCore;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Extensions;

namespace SOS.Export.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class SightingFactory : Interfaces.ISightingFactory
    {
        private readonly IProcessedDarwinCoreRepository _processedDarwinCoreRepository;
        private readonly IFileService _fileService;
        private readonly string _exportPath;
        private readonly ILogger<SightingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedDarwinCoreRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="fileDestination"></param>
        /// <param name="logger"></param>
        public SightingFactory(
            IProcessedDarwinCoreRepository processedDarwinCoreRepository,
            IFileService fileService,
            FileDestination fileDestination,
            ILogger<SightingFactory> logger)
        {
            _processedDarwinCoreRepository = processedDarwinCoreRepository ?? throw new ArgumentNullException(nameof(processedDarwinCoreRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _exportPath = fileDestination?.Path ?? throw new ArgumentNullException(nameof(fileDestination));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> ExportAllAsync()
        {
            try
            {
                // Create temp folder
                var folder = Guid.NewGuid().ToString();
                _fileService.CreateFolder(_exportPath, folder);

                var skip = 0;
                const int take = 1000000;
                var processedDarwinCore = await _processedDarwinCoreRepository.GetChunkAsync(skip, take);

                while (processedDarwinCore.Any())
                {
                    var dwC = new List<DwC>();
                    var dwCOccurrence = new List<DwCOccurrence>();
                    var dwCMaterialSample = new List<DwCMaterialSample>();
                    var dwCEvent = new List<DwCEvent>();
                    var dwCLocation = new List<DwCLocation>();
                    var dwCGeologicalContext = new List<DwCGeologicalContext>();
                    var dwCIdentification = new List<DwCIdentification>();
                    var dwCTaxon = new List<DwCTaxon>();
                    var dwCMeasurementOrFact = new List<DwCMeasurementOrFact>();
                    var dwCResourceRelationship = new List<DwCResourceRelationship>();

                    var createFile = skip == 0;
                    foreach (var record in processedDarwinCore)
                    {
                        dwC.Add(record.ToDarwinCoreArchive());

                        if (record.Occurrence?.HasData() ?? false)
                        {
                            dwCOccurrence.Add(record.Occurrence.ToDarwinCoreArchive(record.DatasetID));
                        }

                        if (record.MaterialSample?.HasData() ?? false)
                        {
                            dwCMaterialSample.Add(record.MaterialSample.ToDarwinCoreArchive(record.DatasetID));
                        }

                        if (record.Event?.HasData() ?? false)
                        {
                            dwCEvent.Add(record.Event.ToDarwinCoreArchive(record.DatasetID));
                        }

                        if (record.Location?.HasData() ?? false)
                        {
                            dwCLocation.Add(record.Location.ToDarwinCoreArchive(record.DatasetID));
                        }

                        if (record.GeologicalContext?.HasData() ?? false)
                        {
                            dwCGeologicalContext.Add(record.GeologicalContext.ToDarwinCoreArchive(record.DatasetID));
                        }

                        if (record.Identification?.HasData() ?? false)
                        {
                            dwCIdentification.Add(record.Identification.ToDarwinCoreArchive(record.DatasetID));
                        }

                        if (record.Taxon?.HasData() ?? false)
                        {
                            dwCTaxon.Add(record.Taxon.ToDarwinCoreArchive(record.DatasetID));
                        }

                        if (record.MeasurementOrFact?.HasData() ?? false)
                        {
                            dwCMeasurementOrFact.Add(record.MeasurementOrFact.ToDarwinCoreArchive(record.DatasetID));
                        }

                        if (record.ResourceRelationship?.HasData() ?? false)
                        {
                            dwCResourceRelationship.Add(record.ResourceRelationship.ToDarwinCoreArchive(record.DatasetID));
                        }
                    }

                    var writeTasks = new[]
                    {
                        _fileService.WriteToCsvFileAsync($@"{_exportPath}\{folder}\Records.csv", createFile, dwC, new DwCMap() ),
                        _fileService.WriteToCsvFileAsync($@"{_exportPath}\{folder}\Occurrence.csv", createFile, dwCOccurrence, new DwCOccurrenceMap() ),
                        _fileService.WriteToCsvFileAsync($@"{_exportPath}\{folder}\MaterialSample.csv", createFile, dwCMaterialSample, new DwCMaterialSampleMap() ),
                        _fileService.WriteToCsvFileAsync($@"{_exportPath}\{folder}\Event.csv", createFile, dwCEvent, new DwCEventMap() ),
                        _fileService.WriteToCsvFileAsync($@"{_exportPath}\{folder}\Location.csv", createFile, dwCLocation, new DwCLocationMap() ),
                        _fileService.WriteToCsvFileAsync($@"{_exportPath}\{folder}\GeologicalContext.csv", createFile, dwCGeologicalContext, new DwCGeologicalContextMap() ),
                        _fileService.WriteToCsvFileAsync($@"{_exportPath}\{folder}\Identification.csv", createFile, dwCIdentification, new DwCIdentificationMap() ),
                        _fileService.WriteToCsvFileAsync($@"{_exportPath}\{folder}\Taxon.csv", createFile, dwCTaxon, new DwCTaxonMap() ),
                        _fileService.WriteToCsvFileAsync($@"{_exportPath}\{folder}\MeasurementOrFact.csv", createFile, dwCMeasurementOrFact, new DwCMeasurementOrFactMap() ),
                        _fileService.WriteToCsvFileAsync($@"{_exportPath}\{folder}\ResourceRelationship.csv", createFile, dwCResourceRelationship, new DwCResourceRelationshipMap() )
                    };

                    await Task.WhenAll(writeTasks);
 
                    skip += take;
                    processedDarwinCore = await _processedDarwinCoreRepository.GetChunkAsync(skip, take);
                }

                // Add metadata, compress files and remove temp directory
                AddMeta($@"{_exportPath}\{folder}");
                _fileService.CopyFiles(@".\DarwinCore", new []{ "eml.xml"}, $@"{_exportPath}\{folder}");
                _fileService.CompressFolder(_exportPath, folder);
                _fileService.DeleteFolder($@"{_exportPath}\{folder}");

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export all sightings");
                return false;
            }
        }

        /// <summary>
        /// Get meta.xml, remove unused extensions and copy file to output directory
        /// </summary>
        /// <param name="path"></param>
        private void AddMeta(string path)
        {
            // Get names of files created
            var files = _fileService.GetFolderFiles(path)?.Select(f => f.Substring(f.LastIndexOf(@"\", StringComparison.CurrentCultureIgnoreCase)+1)).ToArray() ?? new string[0];
            
            // Get meta.xml file
            var meta = _fileService.GetXmlDocument(@".\DarwinCore\meta.xml");
            
            // Get all files elements i xml file
            var namespaceManager = new XmlNamespaceManager(meta.NameTable);
            namespaceManager.AddNamespace("rs", "http://rs.tdwg.org/dwc/text/");
            var filesElements = meta.SelectNodes(@"//rs:files", namespaceManager);

            foreach (XmlNode filesElement in filesElements)
            {
                // If file not created, remove element from xml
                if (!files.Contains(filesElement.InnerText, StringComparer.CurrentCultureIgnoreCase))
                {
                    filesElement.ParentNode.ParentNode.RemoveChild(filesElement.ParentNode);
                }
            }

            // Save meta.xml to output directory
            meta.Save($@"{path}\meta.xml");
        }
    }
}
