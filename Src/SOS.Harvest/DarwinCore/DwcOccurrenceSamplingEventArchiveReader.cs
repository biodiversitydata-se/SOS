using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using DwC_A;
using DwC_A.Terms;
using Microsoft.Extensions.Logging;
using RecordParser.Builders.Reader;
using SOS.Harvest.DarwinCore.Factories;
using SOS.Harvest.DarwinCore.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Common;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.DataStewardship.Enums;

using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Harvest.DarwinCore
{
    /// <summary>
    ///     DwC-A reader for sampling event based DwC-A as DwcObservationVerbatim collection.
    /// </summary>
    public class DwcOccurrenceSamplingEventArchiveReader : IDwcArchiveReaderAsDwcObservation
    {
        private readonly ILogger<DwcArchiveReader> _logger;
        private int _idCounter;
        private int NextId => Interlocked.Increment(ref _idCounter);

        private IVariableLengthReaderBuilder<SamplingEventTaxonList> SamplingEventTaxonListMapping => new VariableLengthReaderBuilder<SamplingEventTaxonList>()
            .Map(t => t.EventID, indexColumn: 0)
            .Map(t => t.SamplingTaxonlistID, 1)
            .Map(t => t.SamplingEffortTime, 2)
            .Map(t => t.BasisOfRecord, 3)
            .Map(t => t.RecordedBy, 4)
            .Map(t => t.IdentificationVerificationStatus, 5);

        public DwcOccurrenceSamplingEventArchiveReader(ILogger<DwcArchiveReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idCounter = 0;
        }

        /// <summary>
        ///     Add data from DwC-A extensions
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="occurrenceRecords"></param>
        /// <returns></returns>
        private async Task AddDataFromExtensionsAsync(ArchiveReader archiveReader,
            List<DwcObservationVerbatim> occurrenceRecords, bool addOnlyOccurrenceData = false)
        {
            await Task.WhenAll(
              AddEventDataAsync(occurrenceRecords, archiveReader, addOnlyOccurrenceData),
              AddEmofExtensionDataAsync(occurrenceRecords, archiveReader, addOnlyOccurrenceData),
              AddMofExtensionDataAsync(occurrenceRecords, archiveReader, addOnlyOccurrenceData),
              AddMultimediaExtensionDataAsync(occurrenceRecords, archiveReader, addOnlyOccurrenceData),
              AddAudubonMediaExtensionDataAsync(occurrenceRecords, archiveReader, addOnlyOccurrenceData)
          );
        }

        /// <summary>
        ///     Add Measurement Or Fact extension data
        /// </summary>
        /// <param name="occurrenceRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddMofExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords,
            ArchiveReader archiveReader, bool addOnlyOccurrenceData = false)
        {
            try
            {
                var mofFileReader = archiveReader.GetAsyncFileReader(RowTypes.MeasurementOrFact);
                if (mofFileReader == null) return;
                var idIndex = mofFileReader.GetIdIndex();

                var observationsByRecordId =
                    occurrenceRecords
                        .GroupBy(observation => observation.RecordId)
                        .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());

                await foreach (var row in mofFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    AddEventMof(row, id, observationsByRecordId);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add MeasurementOrFact extension data");
                throw;
            }
        }

        /// <summary>
        ///     Add Simple Multimedia extension data
        /// </summary>
        /// <param name="occurrenceRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddMultimediaExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords,
            ArchiveReader archiveReader, bool addOnlyOccurrenceData = false)
        {
            try
            {
                var multimediaFileReader = archiveReader.GetAsyncFileReader(RowTypes.Multimedia);
                if (multimediaFileReader == null) return;
                var idIndex = multimediaFileReader.GetIdIndex();
                var observationsByRecordId =
                    occurrenceRecords
                        .GroupBy(observation => observation.RecordId)
                        .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());

                await foreach (var row in multimediaFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (!observationsByRecordId.TryGetValue(id, out var observations)) continue;
                    foreach (var observation in observations)
                    {
                        if (observation.EventMultimedia == null)
                        {
                            observation.EventMultimedia = new List<DwcMultimedia>();
                        }

                        var multimediaItem = DwcMultimediaFactory.Create(row);
                        observation.EventMultimedia.Add(multimediaItem);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add Multimedia extension data");
                throw;
            }
        }

        private async Task AddAudubonMediaExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords,
            ArchiveReader archiveReader, bool addOnlyOccurrenceData = false)
        {
            try
            {
                var audubonFileReader = archiveReader.GetAsyncFileReader(RowTypes.AudubonMediaDescription);
                if (audubonFileReader == null) return;
                var idIndex = audubonFileReader.GetIdIndex();
                var observationsByRecordId =
                    occurrenceRecords
                        .GroupBy(observation => observation.RecordId)
                        .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());

                await foreach (var row in audubonFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (!observationsByRecordId.TryGetValue(id, out var observations)) continue;
                    foreach (var observation in observations)
                    {
                        if (observation.EventAudubonMedia == null)
                        {
                            observation.EventAudubonMedia = new List<DwcAudubonMedia>();
                        }

                        var multimediaItem = DwcAudubonMediaFactory.Create(row);
                        observation.EventAudubonMedia.Add(multimediaItem);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add Audubon media description extension data");
                throw;
            }
        }

        /// <summary>
        ///     Add MeasureMentOrFact data
        /// </summary>
        /// <param name="row"></param>
        /// <param name="id"></param>
        /// <param name="observationsByRecordId"></param>
        private void AddEventMof(
            IRow row,
            string id,
            Dictionary<string, IEnumerable<DwcObservationVerbatim>> observationsByRecordId)
        {
            if (!observationsByRecordId.TryGetValue(id, out var observations)) return;
            foreach (var observation in observations)
            {
                if (observation.EventMeasurementOrFacts == null)
                {
                    observation.EventMeasurementOrFacts = new List<DwcMeasurementOrFact>();
                }

                var mofItem = DwcMeasurementOrFactFactory.Create(row);
                observation.EventMeasurementOrFacts.Add(mofItem);
            }
        }

        /// <summary>
        ///     Add event data to observations by reading the core file.
        /// </summary>
        /// <param name="occurrenceRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddEventDataAsync(List<DwcObservationVerbatim> occurrenceRecords,
            ArchiveReader archiveReader, bool addOnlyOccurrenceData = false)
        {
            if (addOnlyOccurrenceData) return;
            var eventFileReader = archiveReader.GetAsyncCoreFile();
            var idIndex = eventFileReader.GetIdIndex();
            var observationsByRecordId =
                occurrenceRecords
                    .GroupBy(observation => observation.RecordId)
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());

            await foreach (var row in eventFileReader.GetDataRowsAsync())
            {
                var id = row[idIndex];
                if (!observationsByRecordId.TryGetValue(id, out var observations)) continue;
                foreach (var observation in observations)
                {
                    foreach (var fieldType in row.FieldMetaData)
                    {
                        var val = row[fieldType.Index];
                        DwcTermValueMapper.MapValueByTerm(observation, fieldType.Term, val);
                    }
                }
            }
        }

        /// <summary>
        ///     Add Extended Measurement Or Fact data
        /// </summary>
        /// <param name="occurrenceRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddEmofExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords,
            ArchiveReader archiveReader, bool addOnlyOccurrenceData = false)
        {
            try
            {
                var emofFileReader = archiveReader.GetAsyncFileReader(RowTypes.ExtendedMeasurementOrFact);
                if (emofFileReader == null) return;
                var idIndex = emofFileReader.GetIdIndex();
                var occurrenceIdFieldMetaData = emofFileReader.TryGetFieldMetaData(Terms.occurrenceID);
                var observationsByRecordId =
                    occurrenceRecords
                        .GroupBy(observation => observation.RecordId)
                        .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());

                if (occurrenceIdFieldMetaData == null
                ) // If there is no occurrenceID field, then add only event measurements
                {
                    await foreach (var row in emofFileReader.GetDataRowsAsync())
                    {
                        var id = row[idIndex];
                        AddEventEmof(row, id, observationsByRecordId);
                    }
                }
                else // occurrenceID field exist, try to get both occurrence measurements and event measurements
                {
                    var observationByOccurrenceId =
                        occurrenceRecords.ToDictionary(v => v.OccurrenceID, v => v);
                    await foreach (var row in emofFileReader.GetDataRowsAsync())
                    {
                        var occurrenceId = row[occurrenceIdFieldMetaData.Index];
                        AddOccurrenceEmof(row, occurrenceId, observationByOccurrenceId);
                        if (string.IsNullOrEmpty(occurrenceId)) // Measurement for event
                        {
                            var id = row[idIndex];
                            AddEventEmof(row, id, observationsByRecordId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add ExtendedMeasurementOrFact extension data");
                throw;
            }
        }

        /// <summary>
        ///     Add event measurement
        /// </summary>
        /// <param name="row"></param>
        /// <param name="id"></param>
        /// <param name="observationsByRecordId"></param>
        private void AddEventEmof(
            IRow row,
            string id,
            Dictionary<string, IEnumerable<DwcObservationVerbatim>> observationsByRecordId)
        {
            if (!observationsByRecordId.TryGetValue(id, out var observations)) return;
            foreach (var observation in observations)
            {
                if (observation.EventExtendedMeasurementOrFacts == null)
                {
                    observation.EventExtendedMeasurementOrFacts = new List<DwcExtendedMeasurementOrFact>();
                }

                var emofItem = DwcExtendedMeasurementOrFactFactory.Create(row);
                observation.EventExtendedMeasurementOrFacts.Add(emofItem);
            }
        }

        /// <summary>
        ///     Add occurrence measurement.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="occurrenceId"></param>
        /// <param name="observationByOccurrenceId"></param>
        private void AddOccurrenceEmof(
            IRow row,
            string occurrenceId,
            Dictionary<string, DwcObservationVerbatim> observationByOccurrenceId)
        {
            if (string.IsNullOrEmpty(occurrenceId)) return;

            if (observationByOccurrenceId.TryGetValue(occurrenceId, out var obs))
            {
                if (obs.ObservationExtendedMeasurementOrFacts == null)
                {
                    obs.ObservationExtendedMeasurementOrFacts = new List<DwcExtendedMeasurementOrFact>();
                }

                var emofItem = DwcExtendedMeasurementOrFactFactory.Create(row);
                obs.ObservationExtendedMeasurementOrFacts.Add(emofItem);
            }
        }

        #region Event
        private async Task AddDataFromExtensionsAsync(ArchiveReader archiveReader,
            IEnumerable<DwcEventOccurrenceVerbatim> eventRecords)
        {
            var eventDictionary = eventRecords?.ToDictionary(e => e.RecordId, e => e);
            await AddOccurencesDataAsync(eventDictionary, archiveReader);
            await AddEmofExtensionDataAsync(eventDictionary, archiveReader);
            await AddMofExtensionDataAsync(eventDictionary, archiveReader);
            try
            {
                if (File.Exists(Path.Combine(archiveReader.OutputPath, "taxonlist.xml")))
                    await AddTaxonListDataAsync(eventDictionary, archiveReader.OutputPath);
            }
            catch (Exception) { };
        }

        private async Task AddDataFromExtensionsAsync(ArchiveReaderContext archiveReaderContext,
            IEnumerable<DwcEventOccurrenceVerbatim> eventRecords)
        {
            var archiveReader  = archiveReaderContext.ArchiveReader;
            var eventDictionary = eventRecords?.ToDictionary(e => e.RecordId, e => e);
            await AddOccurencesDataAsync(eventDictionary, archiveReader);
            await AddEmofExtensionDataAsync(eventDictionary, archiveReader);
            await AddMofExtensionDataAsync(eventDictionary, archiveReader);
            try
            {
                if (File.Exists(Path.Combine(archiveReader.OutputPath, "taxonlist.xml")))
                    await AddTaxonListDataAsync(eventDictionary, archiveReader.OutputPath);
            }
            catch (Exception) { };
        }

        /// <summary>
        /// Add occurrences to event
        /// </summary>
        /// <param name="eventRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddOccurencesDataAsync(IDictionary<string, DwcEventOccurrenceVerbatim> eventRecords,
            ArchiveReader archiveReader)
        {
            try
            {
                var occurrenceFileReader = archiveReader.GetAsyncFileReader(RowTypes.Occurrence);
                if (occurrenceFileReader == null)
                {
                    return;
                };
                var idIndex = occurrenceFileReader.GetIdIndex();

                await foreach (var row in occurrenceFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];

                    if (!eventRecords.TryGetValue(id, out var eventRecord))
                    {
                        continue;
                    }
                    var occurrenceRecord = DwcObservationVerbatimFactory.Create(NextId, row, null, idIndex);
                    eventRecord.Observations ??= new List<DwcObservationVerbatim>();
                    eventRecord.Observations.Add(occurrenceRecord);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add occurrence data");
                throw;
            }
        }

        private async Task AddTaxonListDataAsync(IDictionary<string, DwcEventOccurrenceVerbatim> eventRecords,
            string path)
        {
            try
            {
                await using var xmlFileStream = File.OpenRead(Path.Combine(path, "taxonlist.xml"));
                using var xmlReader = XmlReader.Create(xmlFileStream);
                var xmlDoc = XDocument.Load(xmlReader);
                var ns = xmlDoc.Root.GetDefaultNamespace();
                var taxonlistsElement = xmlDoc.Element(ns + "taxonlists");

                if (taxonlistsElement == null)
                {
                    return;
                }
                var taxonLists = new Dictionary<string, HashSet<DwcTaxon>>();

                foreach (var taxonListElement in taxonlistsElement.Elements(ns + "taxonlist"))
                {
                    var listId = taxonListElement.Element(ns + "samplingTaxonlistID")?.Value;

                    var taxaElement = taxonListElement.Element(ns + "taxa");

                    if (taxaElement == null)
                    {
                        continue;
                    }

                    foreach (var taxonElement in taxaElement.Elements(ns + "taxon"))
                    {
                        if (!taxonLists.TryGetValue(listId, out var taxonList))
                        {
                            taxonList = new HashSet<DwcTaxon>();
                            taxonLists.Add(listId, taxonList);
                        }

                        var taxonId = taxonElement.Element(ns + "taxonID").Value;
                        var scientificName = taxonElement.Element(ns + "scientificName").Value;
                        var taxonRank = taxonElement.Element(ns + "taxonRank").Value;
                        var kingdom = taxonElement.Element(ns + "kingdom").Value;
                        taxonList.Add(new DwcTaxon
                        {
                            TaxonID = taxonId,
                            ScientificName = scientificName,
                            TaxonRank = taxonRank,
                            Kingdom = kingdom
                        });
                    }
                }

                using var csvFileHelper = new CsvFileHelper();
                await using var csvFileStream = File.OpenRead(Path.Combine(path, "samplingEventTaxonList.txt"));
                csvFileHelper.InitializeRead(csvFileStream, "\t");

                var taxonListEventMapping = csvFileHelper.GetRecords(SamplingEventTaxonListMapping);

                csvFileHelper.FinishRead();
                csvFileStream.Close();

                if (!taxonListEventMapping?.Any() ?? true)
                {
                    return;
                }

                foreach (var listEventMapping in taxonListEventMapping)
                {
                    if (!eventRecords.TryGetValue(listEventMapping.EventID, out var eventRecord))
                    {
                        continue;
                    }

                    eventRecord.BasisOfRecord = listEventMapping.BasisOfRecord;
                    eventRecord.IdentificationVerificationStatus = listEventMapping.IdentificationVerificationStatus;
                    eventRecord.RecordedBy = listEventMapping.RecordedBy;
                    eventRecord.SamplingEffortTime = listEventMapping.SamplingEffortTime;

                    if (!taxonLists.TryGetValue(listEventMapping.SamplingTaxonlistID, out var taxonList))
                    {
                        continue;
                    }

                    eventRecord.Taxa = taxonList;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add taxon list data");
                throw;
            }
        }

        /// <summary>
        /// Read data stewardship datasets.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        public async Task<List<DwcVerbatimObservationDataset>> ReadDatasetsAsync(ArchiveReader archiveReader)
        {
            var datasets = await GetDatasetsFromJsonOrXmlAsync(archiveReader.OutputPath);
            return datasets;
        }

        private async Task<List<DwcVerbatimObservationDataset>?> GetDatasetsFromJsonOrXmlAsync(string path)
        {
            try
            {
                return (await GetDatasetsFromJsonAsync(path)) ?? (await GetDatasetsFromXmlAsync(path));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to read dataset xml/json");
                throw;
            }
        }

        private async Task<List<DwcVerbatimObservationDataset>?> GetDatasetsFromJsonAsync(string path)
        {
            // JSON
            string jsonFilePath = Path.Combine(path, "datastewardship.json");
            if (!File.Exists(jsonFilePath))
            {
                return null;
            }

            await using var jsonFileStream = File.OpenRead(Path.Combine(path, "datastewardship.json"));
            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var observationDatasets = JsonSerializer.Deserialize<List<DwcVerbatimObservationDataset>>(jsonFileStream, jsonSerializerOptions);
            for (int i = 0; i < (observationDatasets?.Count ?? 0); i++)
                observationDatasets![i].Id = i + 1;

            return observationDatasets;
        }

        private async Task<List<DwcVerbatimObservationDataset>?> GetDatasetsFromXmlAsync(string path)
        {
            // XML
            string xmlFilePath = Path.Combine(path, "datastewardship.xml");
            if (!File.Exists(xmlFilePath))
            {
                return null;
            }

            await using var xmlFileStream = File.OpenRead(xmlFilePath);
            using var xmlReader = XmlReader.Create(xmlFileStream);
            var xmlDoc = XDocument.Load(xmlReader);
            var ns = xmlDoc.Root.GetDefaultNamespace();
            var datasetsElement = xmlDoc.Element(ns + "datasets");

            if (datasetsElement == null)
            {
                return null;
            }
            var observationDatasets = new List<DwcVerbatimObservationDataset>();
            var id = 1;
            foreach (var datasetElement in datasetsElement.Elements(ns + "dataset"))
            {
                Enum.TryParse(typeof(AccessRights), datasetElement.Element(ns + "accessRights")?.Value, out var accessRights);
                Enum.TryParse(typeof(ProgrammeArea), datasetElement.Element(ns + "programmeArea")?.Value, out var programmeArea);
                Enum.TryParse(typeof(Purpose), datasetElement.Element(ns + "purpose")?.Value, out var purpose);
                var endDateExists = DateTime.TryParse(datasetElement.Element(ns + "endDate")?.Value, out var endDate);
                var startDateExists = DateTime.TryParse(datasetElement.Element(ns + "startDate")?.Value, out var startDate);
                var dataset = new DwcVerbatimObservationDataset
                {
                    AccessRights = accessRights == null ? null : (AccessRights)accessRights,
                    DataStewardship = datasetElement.Element(ns + "dataStewardship")?.Value,
                    Description = datasetElement.Element(ns + "description")?.Value,
                    DescriptionAccessRights = datasetElement.Element(ns + "descriptionAccessRights")?.Value,
                    EndDate = endDateExists ? endDate : null,
                    Identifier = datasetElement.Element(ns + "identifier")?.Value,
                    Language = datasetElement.Element(ns + "language")?.Value,
                    Metadatalanguage = datasetElement.Element(ns + "metadatalanguage")?.Value,
                    ProgrammeArea = programmeArea == null ? null : (ProgrammeArea)programmeArea,
                    Purpose = purpose == null ? null : (Purpose)purpose,
                    Spatial = datasetElement.Element(ns + "spatial")?.Value,
                    StartDate = startDateExists ? startDate : null,
                    Title = datasetElement.Element(ns + "title")?.Value,
                };

                var assigner = datasetElement.Element(ns + "assigner");
                if (assigner != null)
                {
                    dataset.Assigner = new Organisation
                    {
                        OrganisationCode = assigner.Element(ns + "organisationCode")?.Value,
                        OrganisationID = assigner.Element(ns + "organisationID")?.Value
                    };
                }

                var creator = datasetElement.Element(ns + "creator");
                if (creator != null)
                {
                    dataset.Creator = new List<Organisation>();
                    foreach (var organisation in creator.Elements())
                    {
                        dataset.Creator.Add(new Organisation
                        {
                            OrganisationCode = organisation.Element(ns + "organisationCode")?.Value,
                            OrganisationID = organisation.Element(ns + "organisationID")?.Value
                        });
                    }
                }

                var eventIds = datasetElement.Element(ns + "eventIds");
                if (eventIds != null)
                {
                    dataset.EventIds = new List<string>();
                    foreach (var eventId in eventIds.Elements())
                    {
                        dataset.EventIds.Add(eventId.Value);
                    }
                }

                var methodology = datasetElement.Element(ns + "methodology");
                if (methodology != null)
                {
                    dataset.Methodology = new List<Methodology>();
                    foreach (var method in methodology.Elements())
                    {
                        dataset.Methodology.Add(new Methodology
                        {
                            MethodologyDescription = method.Element(ns + "methodologyDescription")?.Value,
                            MethodologyLink = method.Element(ns + "methodologyLink")?.Value,
                            MethodologyName = method.Element(ns + "methodologyName")?.Value,
                            SpeciesList = method.Element(ns + "speciesList")?.Value
                        });
                    }
                }

                var ownerinstitutionCode = datasetElement.Element(ns + "ownerinstitutionCode");
                if (ownerinstitutionCode != null)
                {
                    dataset.OwnerinstitutionCode = new Organisation
                    {
                        OrganisationCode = ownerinstitutionCode.Element(ns + "organisationCode")?.Value,
                        OrganisationID = ownerinstitutionCode.Element(ns + "organisationID")?.Value
                    };
                }

                var projects = datasetElement.Element(ns + "project");
                if (projects != null)
                {
                    dataset.Project = new List<Project>();
                    foreach (var project in projects.Elements())
                    {
                        Enum.TryParse(typeof(ProjectType), project.Element(ns + "projectType")?.Value, out var projectType);
                        dataset.Project.Add(new Project
                        {
                            ProjectCode = project.Element(ns + "projectCode")?.Value,
                            ProjectId = project.Element(ns + "projectID")?.Value,
                            ProjectType = projectType == null ? null : (ProjectType)projectType
                        });
                    }
                }

                var publisher = datasetElement.Element(ns + "publisher");
                if (publisher != null)
                {
                    dataset.Publisher = new Organisation
                    {
                        OrganisationCode = publisher.Element(ns + "organisationCode")?.Value,
                        OrganisationID = publisher.Element(ns + "organisationID")?.Value
                    };
                }
                observationDatasets.Add(dataset);
                id++;
            }

            return observationDatasets;
        }

        private Dictionary<string, DwcVerbatimObservationDataset> CreateEventObservationDatasetDictionary(IEnumerable<DwcVerbatimObservationDataset> observationDatasets)
        {
            if (observationDatasets == null || !observationDatasets.Any()) return null;
            var observationDatasetByEventId = new Dictionary<string, DwcVerbatimObservationDataset>();
            foreach (var observationDataset in observationDatasets)
            {
                if (observationDataset.EventIds == null) continue;

                foreach (var eventId in observationDataset.EventIds)
                {
                    observationDatasetByEventId[eventId] = observationDataset;
                }
            }

            return observationDatasetByEventId;
        }

        /// <summary>
        ///     Add Measurement Or Fact extension data
        /// </summary>
        /// <param name="eventRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddMofExtensionDataAsync(IDictionary<string, DwcEventOccurrenceVerbatim> eventRecords,
            ArchiveReader archiveReader)
        {
            try
            {
                var mofFileReader = archiveReader.GetAsyncFileReader(RowTypes.MeasurementOrFact);
                if (mofFileReader == null)
                {
                    return;
                };
                var idIndex = mofFileReader.GetIdIndex();

                await foreach (var row in mofFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];

                    if (!eventRecords.TryGetValue(id, out var eventRecord))
                    {
                        continue;
                    }

                    eventRecord.MeasurementOrFacts ??= new List<DwcMeasurementOrFact>();
                    eventRecord.MeasurementOrFacts.Add(DwcMeasurementOrFactFactory.Create(row));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add MeasurementOrFact extension data");
                throw;
            }
        }

        /// <summary>
        ///     Add Extended Measurement Or Fact extension data
        /// </summary>
        /// <param name="eventRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddEmofExtensionDataAsync(IDictionary<string, DwcEventOccurrenceVerbatim> eventRecords,
            ArchiveReader archiveReader)
        {
            try
            {
                var emofFileReader = archiveReader.GetAsyncFileReader(RowTypes.ExtendedMeasurementOrFact);
                if (emofFileReader == null)
                {
                    return;
                };
                var idIndex = emofFileReader.GetIdIndex();

                await foreach (var row in emofFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];

                    if (!eventRecords.TryGetValue(id, out var eventRecord))
                    {
                        continue;
                    }

                    eventRecord.ExtendedMeasurementOrFacts ??= new List<DwcExtendedMeasurementOrFact>();
                    eventRecord.ExtendedMeasurementOrFacts.Add(DwcExtendedMeasurementOrFactFactory.Create(row));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add MeasurementOrFact extension data");
                throw;
            }
        }


        /// <summary>
        /// Modify input archive to export file
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        private async Task AddNotPresentTaxaToArchive(ArchiveReader archiveReader, IEnumerable<DwcEventOccurrenceVerbatim> events)
        {
            var occurrenceFileReader = archiveReader.GetAsyncFileReader(RowTypes.Occurrence);
            if (occurrenceFileReader == null)
            {
                return;
            };

            var csvHelper = new CsvFileHelper();
            await using var streamWriter = File.AppendText(occurrenceFileReader.FileName);
            csvHelper.InitializeWrite(streamWriter, occurrenceFileReader.FileMetaData.FieldsTerminatedBy);
            // Make sure we have a new line
            csvHelper.NextRecord();
            foreach (var eve in events)
            {
                var notFoundTaxa = eve.Taxa?.Where(t => !(eve.Observations?.Any(o => t.TaxonID.Equals(o.TaxonID)) ?? false)).ToList();                
                if (notFoundTaxa == null)
                {
                    continue;
                }
                foreach (var taxon in notFoundTaxa)
                {
                    foreach (var field in occurrenceFileReader.FileMetaData.Fields)
                    {
                        var value = field.Term switch
                        {
                            "http://rs.tdwg.org/dwc/terms/basisOfRecord" => eve.BasisOfRecord,
                            "http://rs.tdwg.org/dwc/terms/eventID" => eve.EventID,
                            "http://rs.tdwg.org/dwc/terms/occurrenceID" => Guid.NewGuid().ToString(),
                            "http://rs.tdwg.org/dwc/terms/identificationVerificationStatus" => eve.IdentificationVerificationStatus,
                            "http://rs.tdwg.org/dwc/terms/occurrenceStatus" => "absent",
                            "http://rs.tdwg.org/dwc/terms/recordedBy" => eve.RecordedBy,
                            "http://rs.tdwg.org/dwc/terms/taxonID" => taxon.TaxonID,
                            "http://rs.tdwg.org/dwc/terms/scientificName" => taxon.ScientificName,
                            "http://rs.tdwg.org/dwc/terms/taxonRank" => taxon.TaxonRank,
                            "http://rs.tdwg.org/dwc/terms/kingdom" => taxon.Kingdom,
                            _ => string.Empty
                        };

                        csvHelper.WriteField(value);
                    }
                    csvHelper.NextRecord();
                }
            }

            csvHelper.FinishWrite();
            streamWriter.Close();
        }
        #endregion Event        

        /// <summary>
        ///     Reads a sampling event based DwC-A, and returns observations in batches.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="idIdentifierTuple"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int batchSize)
        {
            var occurrenceFileReader = archiveReader.GetAsyncFileReader(RowTypes.Occurrence);
            if (occurrenceFileReader == null) yield break;
            var occurrenceRecords = new List<DwcObservationVerbatim>();
            var idIndex = occurrenceFileReader.GetIdIndex();
            var observationDatasets = await GetDatasetsFromJsonOrXmlAsync(archiveReader.OutputPath);
            var observationDatasetByEventId = CreateEventObservationDatasetDictionary(observationDatasets);

            await foreach (var row in occurrenceFileReader.GetDataRowsAsync())
            {
                var occurrenceRecord = DwcObservationVerbatimFactory.Create(NextId, row, idIdentifierTuple, idIndex);
                occurrenceRecords.Add(occurrenceRecord);

                if (occurrenceRecords.Count % batchSize == 0)
                {
                    await AddDataFromExtensionsAsync(archiveReader, occurrenceRecords);
                    AddDatasetInformation(occurrenceRecords, observationDatasetByEventId, observationDatasets.FirstOrDefault());
                    yield return occurrenceRecords;
                    occurrenceRecords.Clear();
                }
            }

            await AddDataFromExtensionsAsync(archiveReader, occurrenceRecords);
            AddDatasetInformation(occurrenceRecords, observationDatasetByEventId, observationDatasets.FirstOrDefault());
            yield return occurrenceRecords;
        }        

        private void AddDatasetInformation(List<DwcObservationVerbatim> occurrenceRecords, 
            Dictionary<string, DwcVerbatimObservationDataset> observationDatasetByEventId, 
            DwcVerbatimObservationDataset defaultDataset)
        {
            if (observationDatasetByEventId == null && defaultDataset == null) return;
            
            foreach (var occurrenceRecord in occurrenceRecords)
            {
                if (string.IsNullOrEmpty(occurrenceRecord.EventID)) continue;

                if (observationDatasetByEventId != null && observationDatasetByEventId.TryGetValue(occurrenceRecord.EventID, out var observationDataset))
                {
                    occurrenceRecord.DataStewardshipDatasetId = observationDataset.Identifier;
                }
                else if (defaultDataset != null)
                {
                    occurrenceRecord.DataStewardshipDatasetId = defaultDataset.Identifier;
                }
            }
        }

        private void AddDatasetInformation(List<DwcEventOccurrenceVerbatim> events,
            Dictionary<string, DwcVerbatimObservationDataset> observationDatasetByEventId,
            DwcVerbatimObservationDataset defaultDataset)
        {
            if (observationDatasetByEventId == null && defaultDataset == null) return;

            foreach (var ev in events)
            {
                if (string.IsNullOrEmpty(ev.EventID)) continue;

                if (observationDatasetByEventId != null && observationDatasetByEventId.TryGetValue(ev.EventID, out var observationDataset))
                {
                    ev.DataStewardshipDatasetId = observationDataset.Identifier;
                }
                else if (defaultDataset != null)
                {
                    ev.DataStewardshipDatasetId = defaultDataset.Identifier;
                }
            }

            if (defaultDataset != null && (defaultDataset.EventIds == null || !defaultDataset.EventIds.Any()))
            {
                defaultDataset.EventIds = events.Select(e => e.EventID).ToList();
            }
        }

        public async Task<List<DwcObservationVerbatim>> ReadArchiveAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple)
        {
            const int batchSize = 100000;
            var observationsBatches = ReadArchiveInBatchesAsync(
                archiveReader,
                idIdentifierTuple,
                batchSize);
            var observations = new List<DwcObservationVerbatim>();
            await foreach (var observationsBatch in observationsBatches)
            {
                observations.AddRange(observationsBatch);
            }

            return observations;
        }


        public async Task<IEnumerable<DwcEventOccurrenceVerbatim>> ReadEvents(ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple)
        {
            var eventFileReader = archiveReader.GetAsyncFileReader(RowTypes.Event);
            if (eventFileReader == null)
            {
                return null;
            }
            
            var idIndex = eventFileReader.GetIdIndex();
            var events = new List<DwcEventOccurrenceVerbatim>();

            await foreach (var row in eventFileReader.GetDataRowsAsync())
            {
                var eventRecord = DwcEventOccurrenceVerbatimFactory.Create(NextId, row, idIdentifierTuple, idIndex);
                events.Add(eventRecord); ;
            }

            await AddDataFromExtensionsAsync(archiveReader, events);
            if (events.Any(e => e.Taxa != null && e.Taxa.Count > 0))
            {
                await AddNotPresentTaxaToArchive(archiveReader, events);
            }

            return events;
        }

        public async Task<List<DwcVerbatimObservationDataset>> ReadDatasetsAsync(ArchiveReaderContext archiveReaderContext)
        {            
            var datasets = await GetDatasetsFromJsonOrXmlAsync(archiveReaderContext.ArchiveReader.OutputPath);
            archiveReaderContext.ObservationDatasetByEventId = CreateEventObservationDatasetDictionary(datasets);
            archiveReaderContext.Datasets = datasets;
            return datasets;
        }

        private void AddOccurrenceEventData(ArchiveReaderContext archiveReaderContext, DwcObservationVerbatim occurrence)
        {
            if (string.IsNullOrEmpty(occurrence.EventID)) return;

            if (!archiveReaderContext.OccurrenceIdsByEventId.ContainsKey(occurrence.EventID))
            {                
                archiveReaderContext.OccurrenceIdsByEventId.Add(occurrence.EventID, new List<string>());
            }

            archiveReaderContext.OccurrenceIdsByEventId[occurrence.EventID].Add(occurrence.OccurrenceID);
        }

        public async IAsyncEnumerable<List<DwcObservationVerbatim>> ReadOccurrencesInBatchesAsync(ArchiveReaderContext archiveReaderContext)
        {
            var occurrenceFileReader = archiveReaderContext.ArchiveReader.GetAsyncFileReader(RowTypes.Occurrence);
            if (occurrenceFileReader == null) yield break;
            var occurrenceRecords = new List<DwcObservationVerbatim>();
            var idIndex = occurrenceFileReader.GetIdIndex();
            if (archiveReaderContext.OccurrenceIdsByEventId == null) archiveReaderContext.OccurrenceIdsByEventId = new Dictionary<string, List<string>>();
            if (archiveReaderContext.ObservationDatasetByEventId == null) await ReadDatasetsAsync(archiveReaderContext);
            
            await foreach (var row in occurrenceFileReader.GetDataRowsAsync())
            {
                var occurrenceRecord = DwcObservationVerbatimFactory.Create(NextId, row, archiveReaderContext.DataProvider, idIndex);
                AddOccurrenceEventData(archiveReaderContext, occurrenceRecord);
                occurrenceRecords.Add(occurrenceRecord);

                if (occurrenceRecords.Count % archiveReaderContext.BatchSize == 0)
                {
                    await AddDataFromExtensionsAsync(archiveReaderContext.ArchiveReader, occurrenceRecords);
                    AddDatasetInformation(occurrenceRecords, archiveReaderContext.ObservationDatasetByEventId, archiveReaderContext.Datasets.FirstOrDefault());
                    yield return occurrenceRecords;
                    occurrenceRecords.Clear();
                }
            }

            await AddDataFromExtensionsAsync(archiveReaderContext.ArchiveReader, occurrenceRecords);
            AddDatasetInformation(occurrenceRecords, archiveReaderContext.ObservationDatasetByEventId, archiveReaderContext.Datasets.FirstOrDefault());
            yield return occurrenceRecords;
        }

        public async Task<IEnumerable<DwcEventOccurrenceVerbatim>> ReadEvents(ArchiveReaderContext archiveReaderContext)
        {
            var eventFileReader = archiveReaderContext.ArchiveReader.GetAsyncFileReader(RowTypes.Event);
            if (eventFileReader == null)
            {
                return null;
            }

            var idIndex = eventFileReader.GetIdIndex();
            var events = new List<DwcEventOccurrenceVerbatim>();

            await foreach (var row in eventFileReader.GetDataRowsAsync())
            {
                var eventRecord = DwcEventOccurrenceVerbatimFactory.Create(NextId, row, archiveReaderContext.DataProvider, idIndex);
                if (archiveReaderContext.OccurrenceIdsByEventId != null) // Add occurrenceIds
                {
                    if (archiveReaderContext.OccurrenceIdsByEventId.TryGetValue(eventRecord.EventID, out var occurrenceIds))
                    {
                        eventRecord.OccurrenceIds = occurrenceIds;
                    }
                }
                events.Add(eventRecord);
            }

            await AddDataFromExtensionsAsync(archiveReaderContext, events);
            AddDatasetInformation(events, archiveReaderContext.ObservationDatasetByEventId, archiveReaderContext.Datasets.FirstOrDefault());
            foreach (var eve in events)
            {
                //eve.Observations = null;
                eve.NotFoundTaxa = eve.Taxa?.Where(t => !(eve.Observations?.Any(o => t.TaxonID.Equals(o.TaxonID)) ?? false)).ToList();
                eve.NotFoundTaxonIds = eve?.NotFoundTaxa?.Select(m => m.TaxonID).ToList();
            }

            //if (events.Any(e => e.Taxa != null && e.Taxa.Count > 0))
            //{
            //    await AddNotPresentTaxaToArchive(archiveReaderContext.ArchiveReader, events);
            //}

            return events;
        }      

        private class SamplingEventTaxonList
        {
            public string EventID { get; set; }
            public string SamplingTaxonlistID { get; set; }
            public string SamplingEffortTime { get; set; }
            public string BasisOfRecord { get; set; }
            public string RecordedBy { get; set; }
            public string IdentificationVerificationStatus { get; set; }
        }
    }
}