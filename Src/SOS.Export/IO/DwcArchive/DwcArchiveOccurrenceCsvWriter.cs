using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Enums;
using SOS.Export.Helpers;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Managers.Interfaces;
using SOS.Export.Models;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Export.IO.DwcArchive
{
    public class DwcArchiveOccurrenceCsvWriter : IDwcArchiveOccurrenceCsvWriter
    {
        private readonly ILogger<DwcArchiveOccurrenceCsvWriter> _logger;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;
        private readonly ITaxonManager _taxonManager;

        public DwcArchiveOccurrenceCsvWriter(
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            ITaxonManager taxonManager,
            ILogger<DwcArchiveOccurrenceCsvWriter> logger)
        {
            _processedFieldMappingRepository = processedFieldMappingRepository ??
                                               throw new ArgumentNullException(nameof(processedFieldMappingRepository));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> CreateOccurrenceCsvFileAsync(
            FilterBase filter,
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedObservationRepository processedObservationRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var elasticRetrievalStopwatch = new Stopwatch();
                var csvWritingStopwatch = new Stopwatch();
                bool[] fieldsToWriteArray = FieldDescriptionHelper.CreateWriteFieldsArray(fieldDescriptions);
                filter = PrepareFilter(filter);
                var fieldMappings = await _processedFieldMappingRepository.GetFieldMappingsAsync();
                var valueMappingDictionaries = fieldMappings.ToDictionary(m => m.Id, m => m.CreateValueDictionary());
                elasticRetrievalStopwatch.Start();
                var scrollResult = await processedObservationRepository.TypedScrollObservationsAsync(filter, null);
                elasticRetrievalStopwatch.Stop();
                await using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
                var csvWriter = new NReco.Csv.CsvWriter(streamWriter,"\t");
                
                // Write header row
                WriteHeaderRow(csvWriter, fieldDescriptions);

                while (scrollResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    elasticRetrievalStopwatch.Start();
                    var processedObservations = scrollResult.Records.ToArray();
                    elasticRetrievalStopwatch.Stop();
                    
                    // Convert observations to DwC format.
                    ResolveFieldMappedValues(processedObservations, valueMappingDictionaries);
                    var dwcObservations = processedObservations.ToDarwinCore().ToArray();

                    // Write occurrence rows to CSV file.
                    csvWritingStopwatch.Start();
                    foreach (var dwcObservation in dwcObservations)
                    {
                        WriteOccurrenceRow(csvWriter, dwcObservation, fieldsToWriteArray);
                    }
                    await streamWriter.FlushAsync();
                    csvWritingStopwatch.Stop();
                    
                    // Get next batch of observations.
                    elasticRetrievalStopwatch.Start();
                    scrollResult = await processedObservationRepository.TypedScrollObservationsAsync(filter, scrollResult.ScrollId);
                    elasticRetrievalStopwatch.Stop();
                }

                stopwatch.Stop();
                _logger.LogInformation($"Occurrence CSV file created. Total time elapsed: {stopwatch}. Elapsed time for CSV writing: {csvWritingStopwatch}. Elapsed time for reading data from ElasticSearch: {elasticRetrievalStopwatch}");
                return true;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation($"{nameof(CreateOccurrenceCsvFileAsync)} was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create occurrence CSV file.");
                throw;
            }
        }

        /// <summary>
        /// Write occurrence record to CSV file.
        /// </summary>
        /// <param name="csvWriter"></param>
        /// <param name="dwcObservation"></param>
        /// <param name="writeField"></param>
        /// <remarks>The fields must be written in correct order. FieldDescriptionId sorted ascending.</remarks>
        private static void WriteOccurrenceRow(
            NReco.Csv.CsvWriter csvWriter, 
            DarwinCore dwcObservation,
            bool[] writeField)
        {
            if (writeField[(int)FieldDescriptionId.OccurrenceID]) csvWriter.WriteField(dwcObservation.Occurrence.OccurrenceID);
            if (writeField[(int)FieldDescriptionId.BasisOfRecord]) csvWriter.WriteField(dwcObservation.BasisOfRecord);
            if (writeField[(int)FieldDescriptionId.BibliographicCitation]) csvWriter.WriteField(dwcObservation.BibliographicCitation);
            if (writeField[(int)FieldDescriptionId.CollectionCode]) csvWriter.WriteField(dwcObservation.CollectionCode);
            if (writeField[(int)FieldDescriptionId.CollectionID]) csvWriter.WriteField(dwcObservation.CollectionID);
            if (writeField[(int)FieldDescriptionId.DataGeneralizations]) csvWriter.WriteField(dwcObservation.DataGeneralizations);
            if (writeField[(int)FieldDescriptionId.DatasetID]) csvWriter.WriteField(dwcObservation.DatasetID);
            if (writeField[(int)FieldDescriptionId.DatasetName]) csvWriter.WriteField(dwcObservation.DatasetName);
            if (writeField[(int)FieldDescriptionId.DynamicProperties]) csvWriter.WriteField(dwcObservation.DynamicProperties);
            if (writeField[(int)FieldDescriptionId.InformationWithheld]) csvWriter.WriteField(dwcObservation.InformationWithheld);
            if (writeField[(int)FieldDescriptionId.InstitutionCode]) csvWriter.WriteField(dwcObservation.InstitutionCode);
            if (writeField[(int)FieldDescriptionId.InstitutionID]) csvWriter.WriteField(dwcObservation.InstitutionID);
            if (writeField[(int)FieldDescriptionId.Language]) csvWriter.WriteField(dwcObservation.Language);
            if (writeField[(int)FieldDescriptionId.License]) csvWriter.WriteField(dwcObservation.License);
            if (writeField[(int)FieldDescriptionId.Modified]) csvWriter.WriteField(dwcObservation.Modified?.ToString("s"));
            if (writeField[(int)FieldDescriptionId.OwnerInstitutionCode]) csvWriter.WriteField(dwcObservation.OwnerInstitutionCode);
            if (writeField[(int)FieldDescriptionId.References]) csvWriter.WriteField(dwcObservation.References);
            if (writeField[(int)FieldDescriptionId.RightsHolder]) csvWriter.WriteField(dwcObservation.RightsHolder);
            if (writeField[(int)FieldDescriptionId.Type]) csvWriter.WriteField(dwcObservation.Type);
            if (writeField[(int)FieldDescriptionId.Day]) csvWriter.WriteField(dwcObservation.Event.Day.HasValue ? dwcObservation.Event.Day.ToString() : null);
            if (writeField[(int)FieldDescriptionId.EndDayOfYear]) csvWriter.WriteField(dwcObservation.Event.EndDayOfYear.HasValue ? dwcObservation.Event.EndDayOfYear.ToString() : null);
            if (writeField[(int)FieldDescriptionId.EventDate]) csvWriter.WriteField(dwcObservation.Event.EventDate);
            if (writeField[(int)FieldDescriptionId.EventID]) csvWriter.WriteField(dwcObservation.Event.EventID);
            if (writeField[(int)FieldDescriptionId.EventRemarks]) csvWriter.WriteField(dwcObservation.Event.EventRemarks);
            if (writeField[(int)FieldDescriptionId.EventTime]) csvWriter.WriteField(dwcObservation.Event.EventTime);
            if (writeField[(int)FieldDescriptionId.FieldNotes]) csvWriter.WriteField(dwcObservation.Event.FieldNotes);
            if (writeField[(int)FieldDescriptionId.FieldNumber]) csvWriter.WriteField(dwcObservation.Event.FieldNumber);
            if (writeField[(int)FieldDescriptionId.Habitat]) csvWriter.WriteField(dwcObservation.Event.Habitat);
            if (writeField[(int)FieldDescriptionId.Month]) csvWriter.WriteField(dwcObservation.Event.Month.HasValue ? dwcObservation.Event.Month.ToString() : null);
            if (writeField[(int)FieldDescriptionId.ParentEventID]) csvWriter.WriteField(dwcObservation.Event.ParentEventID);
            if (writeField[(int)FieldDescriptionId.SampleSizeValue]) csvWriter.WriteField(dwcObservation.Event.SampleSizeValue);
            if (writeField[(int)FieldDescriptionId.SampleSizeUnit]) csvWriter.WriteField(dwcObservation.Event.SampleSizeUnit);
            if (writeField[(int)FieldDescriptionId.SamplingEffort]) csvWriter.WriteField(dwcObservation.Event.SamplingEffort);
            if (writeField[(int)FieldDescriptionId.SamplingProtocol]) csvWriter.WriteField(dwcObservation.Event.SamplingProtocol);
            if (writeField[(int)FieldDescriptionId.StartDayOfYear]) csvWriter.WriteField(dwcObservation.Event.StartDayOfYear.HasValue ? dwcObservation.Event.StartDayOfYear.ToString() : null);
            if (writeField[(int)FieldDescriptionId.VerbatimEventDate]) csvWriter.WriteField(dwcObservation.Event.VerbatimEventDate);
            if (writeField[(int)FieldDescriptionId.Year]) csvWriter.WriteField(dwcObservation.Event.Year.HasValue ? dwcObservation.Event.Year.ToString() : null);
            if (writeField[(int)FieldDescriptionId.DateIdentified]) csvWriter.WriteField(dwcObservation.Identification.DateIdentified);
            if (writeField[(int)FieldDescriptionId.IdentificationID]) csvWriter.WriteField(dwcObservation.Identification.IdentificationID);
            if (writeField[(int)FieldDescriptionId.IdentificationQualifier]) csvWriter.WriteField(dwcObservation.Identification.IdentificationQualifier);
            if (writeField[(int)FieldDescriptionId.IdentificationReferences]) csvWriter.WriteField(dwcObservation.Identification.IdentificationReferences);
            if (writeField[(int)FieldDescriptionId.IdentificationRemarks]) csvWriter.WriteField(dwcObservation.Identification.IdentificationRemarks);
            if (writeField[(int)FieldDescriptionId.IdentificationVerificationStatus]) csvWriter.WriteField(dwcObservation.Identification.IdentificationVerificationStatus);
            if (writeField[(int)FieldDescriptionId.IdentifiedBy]) csvWriter.WriteField(dwcObservation.Identification.IdentifiedBy);
            if (writeField[(int)FieldDescriptionId.TypeStatus]) csvWriter.WriteField(dwcObservation.Identification.TypeStatus);
            if (writeField[(int)FieldDescriptionId.Continent]) csvWriter.WriteField(dwcObservation.Location.Continent);
            if (writeField[(int)FieldDescriptionId.CoordinatePrecision]) csvWriter.WriteField(dwcObservation.Location.CoordinatePrecision);
            if (writeField[(int)FieldDescriptionId.CoordinateUncertaintyInMeters]) csvWriter.WriteField(GetCoordinateUncertaintyInMetersValue(dwcObservation.Location.CoordinateUncertaintyInMeters));
            if (writeField[(int)FieldDescriptionId.Country]) csvWriter.WriteField(dwcObservation.Location.Country);
            if (writeField[(int)FieldDescriptionId.CountryCode]) csvWriter.WriteField(dwcObservation.Location.CountryCode);
            if (writeField[(int)FieldDescriptionId.County]) csvWriter.WriteField(dwcObservation.Location.County);
            if (writeField[(int)FieldDescriptionId.DecimalLatitude]) csvWriter.WriteField(dwcObservation.Location.DecimalLatitude?.ToString("F5"));
            if (writeField[(int)FieldDescriptionId.DecimalLongitude]) csvWriter.WriteField(dwcObservation.Location.DecimalLongitude?.ToString("F5"));
            if (writeField[(int)FieldDescriptionId.FootprintSpatialFit]) csvWriter.WriteField(dwcObservation.Location.FootprintSpatialFit);
            if (writeField[(int)FieldDescriptionId.FootprintSRS]) csvWriter.WriteField(dwcObservation.Location.FootprintSRS);
            if (writeField[(int)FieldDescriptionId.FootprintWKT]) csvWriter.WriteField(dwcObservation.Location.FootprintWKT);
            if (writeField[(int)FieldDescriptionId.GeodeticDatum]) csvWriter.WriteField(dwcObservation.Location.GeodeticDatum);
            if (writeField[(int)FieldDescriptionId.GeoreferencedBy]) csvWriter.WriteField(dwcObservation.Location.GeoreferencedBy);
            if (writeField[(int)FieldDescriptionId.GeoreferencedDate]) csvWriter.WriteField(dwcObservation.Location.GeoreferencedDate);
            if (writeField[(int)FieldDescriptionId.GeoreferenceProtocol]) csvWriter.WriteField(dwcObservation.Location.GeoreferenceProtocol);
            if (writeField[(int)FieldDescriptionId.GeoreferenceRemarks]) csvWriter.WriteField(dwcObservation.Location.GeoreferenceRemarks);
            if (writeField[(int)FieldDescriptionId.GeoreferenceSources]) csvWriter.WriteField(dwcObservation.Location.GeoreferenceSources);
            if (writeField[(int)FieldDescriptionId.GeoreferenceVerificationStatus]) csvWriter.WriteField(dwcObservation.Location.GeoreferenceVerificationStatus);
            if (writeField[(int)FieldDescriptionId.HigherGeography]) csvWriter.WriteField(dwcObservation.Location.HigherGeography);
            if (writeField[(int)FieldDescriptionId.HigherGeographyID]) csvWriter.WriteField(dwcObservation.Location.HigherGeographyID);
            if (writeField[(int)FieldDescriptionId.Island]) csvWriter.WriteField(dwcObservation.Location.Island);
            if (writeField[(int)FieldDescriptionId.IslandGroup]) csvWriter.WriteField(dwcObservation.Location.IslandGroup);
            if (writeField[(int)FieldDescriptionId.Locality]) csvWriter.WriteField(dwcObservation.Location.Locality);
            if (writeField[(int)FieldDescriptionId.LocationAccordingTo]) csvWriter.WriteField(dwcObservation.Location.LocationAccordingTo);
            if (writeField[(int)FieldDescriptionId.LocationID]) csvWriter.WriteField(dwcObservation.Location.LocationID);
            if (writeField[(int)FieldDescriptionId.LocationRemarks]) csvWriter.WriteField(dwcObservation.Location.LocationRemarks);
            if (writeField[(int)FieldDescriptionId.MaximumDepthInMeters]) csvWriter.WriteField(dwcObservation.Location.MaximumDepthInMeters);
            if (writeField[(int)FieldDescriptionId.MaximumDistanceAboveSurfaceInMeters]) csvWriter.WriteField(dwcObservation.Location.MaximumDistanceAboveSurfaceInMeters);
            if (writeField[(int)FieldDescriptionId.MaximumElevationInMeters]) csvWriter.WriteField(dwcObservation.Location.MaximumElevationInMeters);
            if (writeField[(int)FieldDescriptionId.MinimumDepthInMeters]) csvWriter.WriteField(dwcObservation.Location.MinimumDepthInMeters);
            if (writeField[(int)FieldDescriptionId.MinimumDistanceAboveSurfaceInMeters]) csvWriter.WriteField(dwcObservation.Location.MinimumDistanceAboveSurfaceInMeters);
            if (writeField[(int)FieldDescriptionId.MinimumElevationInMeters]) csvWriter.WriteField(dwcObservation.Location.MinimumElevationInMeters);
            if (writeField[(int)FieldDescriptionId.Municipality]) csvWriter.WriteField(dwcObservation.Location.Municipality);
            if (writeField[(int)FieldDescriptionId.PointRadiusSpatialFit]) csvWriter.WriteField(dwcObservation.Location.PointRadiusSpatialFit);
            if (writeField[(int)FieldDescriptionId.StateProvince]) csvWriter.WriteField(dwcObservation.Location.StateProvince);
            if (writeField[(int)FieldDescriptionId.WaterBody]) csvWriter.WriteField(dwcObservation.Location.WaterBody);
            if (writeField[(int)FieldDescriptionId.VerbatimCoordinates]) csvWriter.WriteField(dwcObservation.Location.VerbatimCoordinates);
            if (writeField[(int)FieldDescriptionId.VerbatimCoordinateSystem]) csvWriter.WriteField(dwcObservation.Location.VerbatimCoordinateSystem);
            if (writeField[(int)FieldDescriptionId.VerbatimDepth]) csvWriter.WriteField(dwcObservation.Location.VerbatimDepth);
            if (writeField[(int)FieldDescriptionId.VerbatimElevation]) csvWriter.WriteField(dwcObservation.Location.VerbatimElevation);
            if (writeField[(int)FieldDescriptionId.VerbatimLatitude]) csvWriter.WriteField(dwcObservation.Location.VerbatimLatitude);
            if (writeField[(int)FieldDescriptionId.VerbatimLocality]) csvWriter.WriteField(dwcObservation.Location.VerbatimLocality);
            if (writeField[(int)FieldDescriptionId.VerbatimLongitude]) csvWriter.WriteField(dwcObservation.Location.VerbatimLongitude);
            if (writeField[(int)FieldDescriptionId.VerbatimSRS]) csvWriter.WriteField(dwcObservation.Location.VerbatimSRS);
            if (writeField[(int)FieldDescriptionId.AssociatedMedia]) csvWriter.WriteField(dwcObservation.Occurrence.AssociatedMedia);
            if (writeField[(int)FieldDescriptionId.AssociatedReferences]) csvWriter.WriteField(dwcObservation.Occurrence.AssociatedReferences);
            if (writeField[(int)FieldDescriptionId.AssociatedSequences]) csvWriter.WriteField(dwcObservation.Occurrence.AssociatedSequences);
            if (writeField[(int)FieldDescriptionId.AssociatedTaxa]) csvWriter.WriteField(dwcObservation.Occurrence.AssociatedTaxa);
            if (writeField[(int)FieldDescriptionId.Behavior]) csvWriter.WriteField(dwcObservation.Occurrence.Behavior);
            if (writeField[(int)FieldDescriptionId.CatalogNumber]) csvWriter.WriteField(dwcObservation.Occurrence.CatalogNumber);
            if (writeField[(int)FieldDescriptionId.Disposition]) csvWriter.WriteField(dwcObservation.Occurrence.Disposition);
            if (writeField[(int)FieldDescriptionId.EstablishmentMeans]) csvWriter.WriteField(dwcObservation.Occurrence.EstablishmentMeans);
            if (writeField[(int)FieldDescriptionId.IndividualCount]) csvWriter.WriteField(dwcObservation.Occurrence.IndividualCount);
            if (writeField[(int)FieldDescriptionId.LifeStage]) csvWriter.WriteField(dwcObservation.Occurrence.LifeStage);
            if (writeField[(int)FieldDescriptionId.AccessRights]) csvWriter.WriteField(dwcObservation.AccessRights);
            if (writeField[(int)FieldDescriptionId.OccurrenceRemarks]) csvWriter.WriteField(dwcObservation.Occurrence.OccurrenceRemarks);
            if (writeField[(int)FieldDescriptionId.OccurrenceStatus]) csvWriter.WriteField(dwcObservation.Occurrence.OccurrenceStatus);
            if (writeField[(int)FieldDescriptionId.OrganismQuantity]) csvWriter.WriteField(dwcObservation.Occurrence.OrganismQuantity);
            if (writeField[(int)FieldDescriptionId.OrganismQuantityType]) csvWriter.WriteField(dwcObservation.Occurrence.OrganismQuantityType);
            if (writeField[(int)FieldDescriptionId.OtherCatalogNumbers]) csvWriter.WriteField(dwcObservation.Occurrence.OtherCatalogNumbers);
            if (writeField[(int)FieldDescriptionId.Preparations]) csvWriter.WriteField(dwcObservation.Occurrence.Preparations);
            if (writeField[(int)FieldDescriptionId.RecordedBy]) csvWriter.WriteField(dwcObservation.Occurrence.RecordedBy);
            if (writeField[(int)FieldDescriptionId.RecordNumber]) csvWriter.WriteField(dwcObservation.Occurrence.RecordNumber);
            if (writeField[(int)FieldDescriptionId.ReproductiveCondition]) csvWriter.WriteField(dwcObservation.Occurrence.ReproductiveCondition);
            if (writeField[(int)FieldDescriptionId.Sex]) csvWriter.WriteField(dwcObservation.Occurrence.Sex);
            if (writeField[(int)FieldDescriptionId.AssociatedOccurrences]) csvWriter.WriteField(dwcObservation.Organism.AssociatedOccurrences);
            if (writeField[(int)FieldDescriptionId.AssociatedOrganisms]) csvWriter.WriteField(dwcObservation.Organism.AssociatedOrganisms);
            if (writeField[(int)FieldDescriptionId.OrganismID]) csvWriter.WriteField(dwcObservation.Organism.OrganismID);
            if (writeField[(int)FieldDescriptionId.OrganismName]) csvWriter.WriteField(dwcObservation.Organism.OrganismName);
            if (writeField[(int)FieldDescriptionId.OrganismRemarks]) csvWriter.WriteField(dwcObservation.Organism.OrganismRemarks);
            if (writeField[(int)FieldDescriptionId.OrganismScope]) csvWriter.WriteField(dwcObservation.Organism.OrganismScope);
            if (writeField[(int)FieldDescriptionId.PreviousIdentifications]) csvWriter.WriteField(dwcObservation.Organism.PreviousIdentifications);
            if (writeField[(int)FieldDescriptionId.AcceptedNameUsage]) csvWriter.WriteField(dwcObservation.Taxon.AcceptedNameUsage);
            if (writeField[(int)FieldDescriptionId.AcceptedNameUsageID]) csvWriter.WriteField(dwcObservation.Taxon.AcceptedNameUsageID);
            if (writeField[(int)FieldDescriptionId.Class]) csvWriter.WriteField(dwcObservation.Taxon.Class);
            if (writeField[(int)FieldDescriptionId.Family]) csvWriter.WriteField(dwcObservation.Taxon.Family);
            if (writeField[(int)FieldDescriptionId.Genus]) csvWriter.WriteField(dwcObservation.Taxon.Genus);
            if (writeField[(int)FieldDescriptionId.HigherClassification]) csvWriter.WriteField(dwcObservation.Taxon.HigherClassification);
            if (writeField[(int)FieldDescriptionId.InfraspecificEpithet]) csvWriter.WriteField(dwcObservation.Taxon.InfraspecificEpithet);
            if (writeField[(int)FieldDescriptionId.Kingdom]) csvWriter.WriteField(dwcObservation.Taxon.Kingdom);
            if (writeField[(int)FieldDescriptionId.NameAccordingTo]) csvWriter.WriteField(dwcObservation.Taxon.NameAccordingTo);
            if (writeField[(int)FieldDescriptionId.NameAccordingToID]) csvWriter.WriteField(dwcObservation.Taxon.NameAccordingToID);
            if (writeField[(int)FieldDescriptionId.NamePublishedIn]) csvWriter.WriteField(dwcObservation.Taxon.NamePublishedIn);
            if (writeField[(int)FieldDescriptionId.NamePublishedInID]) csvWriter.WriteField(dwcObservation.Taxon.NamePublishedInID);
            if (writeField[(int)FieldDescriptionId.NamePublishedInYear]) csvWriter.WriteField(dwcObservation.Taxon.NamePublishedInYear);
            if (writeField[(int)FieldDescriptionId.NomenclaturalCode]) csvWriter.WriteField(dwcObservation.Taxon.NomenclaturalCode);
            if (writeField[(int)FieldDescriptionId.NomenclaturalStatus]) csvWriter.WriteField(dwcObservation.Taxon.NomenclaturalStatus);
            if (writeField[(int)FieldDescriptionId.Order]) csvWriter.WriteField(dwcObservation.Taxon.Order);
            if (writeField[(int)FieldDescriptionId.OriginalNameUsage]) csvWriter.WriteField(dwcObservation.Taxon.OriginalNameUsage);
            if (writeField[(int)FieldDescriptionId.OriginalNameUsageID]) csvWriter.WriteField(dwcObservation.Taxon.OriginalNameUsageID);
            if (writeField[(int)FieldDescriptionId.ParentNameUsage]) csvWriter.WriteField(dwcObservation.Taxon.ParentNameUsage);
            if (writeField[(int)FieldDescriptionId.ParentNameUsageID]) csvWriter.WriteField(dwcObservation.Taxon.ParentNameUsageID);
            if (writeField[(int)FieldDescriptionId.Phylum]) csvWriter.WriteField(dwcObservation.Taxon.Phylum);
            if (writeField[(int)FieldDescriptionId.ScientificName]) csvWriter.WriteField(dwcObservation.Taxon.ScientificName);
            if (writeField[(int)FieldDescriptionId.ScientificNameAuthorship]) csvWriter.WriteField(dwcObservation.Taxon.ScientificNameAuthorship);
            if (writeField[(int)FieldDescriptionId.ScientificNameID]) csvWriter.WriteField(dwcObservation.Taxon.ScientificNameID);
            if (writeField[(int)FieldDescriptionId.SpecificEpithet]) csvWriter.WriteField(dwcObservation.Taxon.SpecificEpithet);
            if (writeField[(int)FieldDescriptionId.Subgenus]) csvWriter.WriteField(dwcObservation.Taxon.Subgenus);
            if (writeField[(int)FieldDescriptionId.TaxonConceptID]) csvWriter.WriteField(dwcObservation.Taxon.TaxonConceptID);
            if (writeField[(int)FieldDescriptionId.TaxonID]) csvWriter.WriteField(dwcObservation.Taxon.TaxonID);
            if (writeField[(int)FieldDescriptionId.TaxonomicStatus]) csvWriter.WriteField(dwcObservation.Taxon.TaxonomicStatus);
            if (writeField[(int)FieldDescriptionId.TaxonRank]) csvWriter.WriteField(dwcObservation.Taxon.TaxonRank);
            if (writeField[(int)FieldDescriptionId.TaxonRemarks]) csvWriter.WriteField(dwcObservation.Taxon.TaxonRemarks);
            if (writeField[(int)FieldDescriptionId.VerbatimTaxonRank]) csvWriter.WriteField(dwcObservation.Taxon.VerbatimTaxonRank);
            if (writeField[(int)FieldDescriptionId.VernacularName]) csvWriter.WriteField(dwcObservation.Taxon.VernacularName);
            if (writeField[(int)FieldDescriptionId.RelatedResourceID]) csvWriter.WriteField(dwcObservation.ResourceRelationship.RelatedResourceID);
            if (writeField[(int)FieldDescriptionId.RelationshipAccordingTo]) csvWriter.WriteField(dwcObservation.ResourceRelationship.RelationshipAccordingTo);
            if (writeField[(int)FieldDescriptionId.RelationshipEstablishedDate]) csvWriter.WriteField(dwcObservation.ResourceRelationship.RelationshipEstablishedDate);
            if (writeField[(int)FieldDescriptionId.RelationshipOfResource]) csvWriter.WriteField(dwcObservation.ResourceRelationship.RelationshipOfResource);
            if (writeField[(int)FieldDescriptionId.RelationshipRemarks]) csvWriter.WriteField(dwcObservation.ResourceRelationship.RelationshipRemarks);
            if (writeField[(int)FieldDescriptionId.ResourceID]) csvWriter.WriteField(dwcObservation.ResourceRelationship.ResourceID);
            if (writeField[(int)FieldDescriptionId.ResourceRelationshipID]) csvWriter.WriteField(dwcObservation.ResourceRelationship.ResourceRelationshipID);
            if (writeField[(int)FieldDescriptionId.MeasurementAccuracy]) csvWriter.WriteField(dwcObservation.MeasurementOrFact.MeasurementAccuracy);
            if (writeField[(int)FieldDescriptionId.MeasurementDeterminedBy]) csvWriter.WriteField(dwcObservation.MeasurementOrFact.MeasurementDeterminedBy);
            if (writeField[(int)FieldDescriptionId.MeasurementDeterminedDate]) csvWriter.WriteField(dwcObservation.MeasurementOrFact.MeasurementDeterminedDate);
            if (writeField[(int)FieldDescriptionId.MeasurementID]) csvWriter.WriteField(dwcObservation.MeasurementOrFact.MeasurementID);
            if (writeField[(int)FieldDescriptionId.MeasurementMethod]) csvWriter.WriteField(dwcObservation.MeasurementOrFact.MeasurementMethod);
            if (writeField[(int)FieldDescriptionId.MeasurementRemarks]) csvWriter.WriteField(dwcObservation.MeasurementOrFact.MeasurementRemarks);
            if (writeField[(int)FieldDescriptionId.MeasurementType]) csvWriter.WriteField(dwcObservation.MeasurementOrFact.MeasurementType);
            if (writeField[(int)FieldDescriptionId.MeasurementUnit]) csvWriter.WriteField(dwcObservation.MeasurementOrFact.MeasurementUnit);
            if (writeField[(int)FieldDescriptionId.MeasurementValue]) csvWriter.WriteField(dwcObservation.MeasurementOrFact.MeasurementValue);
            if (writeField[(int)FieldDescriptionId.Bed]) csvWriter.WriteField(dwcObservation.GeologicalContext.Bed);
            if (writeField[(int)FieldDescriptionId.EarliestAgeOrLowestStage]) csvWriter.WriteField(dwcObservation.GeologicalContext.EarliestAgeOrLowestStage);
            if (writeField[(int)FieldDescriptionId.EarliestEonOrLowestEonothem]) csvWriter.WriteField(dwcObservation.GeologicalContext.EarliestEonOrLowestEonothem);
            if (writeField[(int)FieldDescriptionId.EarliestEpochOrLowestSeries]) csvWriter.WriteField(dwcObservation.GeologicalContext.EarliestEpochOrLowestSeries);
            if (writeField[(int)FieldDescriptionId.EarliestEraOrLowestErathem]) csvWriter.WriteField(dwcObservation.GeologicalContext.EarliestEraOrLowestErathem);
            if (writeField[(int)FieldDescriptionId.EarliestPeriodOrLowestSystem]) csvWriter.WriteField(dwcObservation.GeologicalContext.EarliestPeriodOrLowestSystem);
            if (writeField[(int)FieldDescriptionId.Formation]) csvWriter.WriteField(dwcObservation.GeologicalContext.Formation);
            if (writeField[(int)FieldDescriptionId.GeologicalContextID]) csvWriter.WriteField(dwcObservation.GeologicalContext.GeologicalContextID);
            if (writeField[(int)FieldDescriptionId.Group]) csvWriter.WriteField(dwcObservation.GeologicalContext.Group);
            if (writeField[(int)FieldDescriptionId.HighestBiostratigraphicZone]) csvWriter.WriteField(dwcObservation.GeologicalContext.HighestBiostratigraphicZone);
            if (writeField[(int)FieldDescriptionId.LatestAgeOrHighestStage]) csvWriter.WriteField(dwcObservation.GeologicalContext.LatestAgeOrHighestStage);
            if (writeField[(int)FieldDescriptionId.LatestEonOrHighestEonothem]) csvWriter.WriteField(dwcObservation.GeologicalContext.LatestEonOrHighestEonothem);
            if (writeField[(int)FieldDescriptionId.LatestEpochOrHighestSeries]) csvWriter.WriteField(dwcObservation.GeologicalContext.LatestEpochOrHighestSeries);
            if (writeField[(int)FieldDescriptionId.LatestEraOrHighestErathem]) csvWriter.WriteField(dwcObservation.GeologicalContext.LatestEraOrHighestErathem);
            if (writeField[(int)FieldDescriptionId.LatestPeriodOrHighestSystem]) csvWriter.WriteField(dwcObservation.GeologicalContext.LatestPeriodOrHighestSystem);
            if (writeField[(int)FieldDescriptionId.LithostratigraphicTerms]) csvWriter.WriteField(dwcObservation.GeologicalContext.LithostratigraphicTerms);
            if (writeField[(int)FieldDescriptionId.LowestBiostratigraphicZone]) csvWriter.WriteField(dwcObservation.GeologicalContext.LowestBiostratigraphicZone);
            if (writeField[(int)FieldDescriptionId.Member]) csvWriter.WriteField(dwcObservation.GeologicalContext.Member);
            if (writeField[(int)FieldDescriptionId.MaterialSampleID]) csvWriter.WriteField(dwcObservation.MaterialSample.MaterialSampleID);

            csvWriter.NextRecord();
        }

        private static string GetCoordinateUncertaintyInMetersValue(int? coordinateUncertaintyInMeters)
        {
            if (!coordinateUncertaintyInMeters.HasValue) return null;
            if (coordinateUncertaintyInMeters < 0)
            {
                return null;
            }
            if (coordinateUncertaintyInMeters.Value == 0)
            {
                return 1.ToString();
            }

            return coordinateUncertaintyInMeters.Value.ToString();
        }

        private static void WriteHeaderRow(NReco.Csv.CsvWriter csvWriter, IEnumerable<FieldDescription> fieldDescriptions)
        {
            foreach (var fieldDescription in fieldDescriptions)
            {
                csvWriter.WriteField(fieldDescription.Name);
            }

            csvWriter.NextRecord();
        }


        private FilterBase PrepareFilter(FilterBase filter)
        {
            if (filter.IncludeUnderlyingTaxa && filter.TaxonIds != null && filter.TaxonIds.Any())
            {
                if (filter.TaxonIds.Contains(0)) // If Biota, then clear taxon filter
                {
                    filter.TaxonIds = new List<int>();
                }
                else
                {
                    filter.TaxonIds = _taxonManager.TaxonTree.GetUnderlyingTaxonIds(filter.TaxonIds, true);
                }
            }

            return filter;
        }

        private void ResolveFieldMappedValues(
            IEnumerable<ProcessedObservation> processedObservations,
            Dictionary<FieldMappingFieldId, Dictionary<int, string>> valueMappingDictionaries)
        {
            foreach (var observation in processedObservations)
            {
                ResolveFieldMappedValue(observation.BasisOfRecord,
                    valueMappingDictionaries[FieldMappingFieldId.BasisOfRecord]);
                ResolveFieldMappedValue(observation.Type, valueMappingDictionaries[FieldMappingFieldId.Type]);
                ResolveFieldMappedValue(observation.AccessRights,
                    valueMappingDictionaries[FieldMappingFieldId.AccessRights]);
                ResolveFieldMappedValue(observation.InstitutionId,
                    valueMappingDictionaries[FieldMappingFieldId.Institution]);
                ResolveFieldMappedValue(observation.Location?.County,
                    valueMappingDictionaries[FieldMappingFieldId.County]);
                ResolveFieldMappedValue(observation.Location?.Municipality,
                    valueMappingDictionaries[FieldMappingFieldId.Municipality]);
                ResolveFieldMappedValue(observation.Location?.Parish,
                    valueMappingDictionaries[FieldMappingFieldId.Parish]);
                ResolveFieldMappedValue(observation.Location?.Province,
                    valueMappingDictionaries[FieldMappingFieldId.Province]);
                ResolveFieldMappedValue(observation.Location?.Country,
                    valueMappingDictionaries[FieldMappingFieldId.Country]);
                ResolveFieldMappedValue(observation.Location?.Continent,
                    valueMappingDictionaries[FieldMappingFieldId.Continent]);
                ResolveFieldMappedValue(observation.Event?.Biotope,
                    valueMappingDictionaries[FieldMappingFieldId.Biotope]);
                ResolveFieldMappedValue(observation.Event?.Substrate,
                    valueMappingDictionaries[FieldMappingFieldId.Substrate]);
                ResolveFieldMappedValue(observation.Identification?.ValidationStatus,
                    valueMappingDictionaries[FieldMappingFieldId.ValidationStatus]);
                ResolveFieldMappedValue(observation.Occurrence?.LifeStage,
                    valueMappingDictionaries[FieldMappingFieldId.LifeStage]);
                ResolveFieldMappedValue(observation.Occurrence?.Activity,
                    valueMappingDictionaries[FieldMappingFieldId.Activity]);
                ResolveFieldMappedValue(observation.Occurrence?.Gender,
                    valueMappingDictionaries[FieldMappingFieldId.Gender]);
                ResolveFieldMappedValue(observation.Occurrence?.OrganismQuantityUnit,
                    valueMappingDictionaries[FieldMappingFieldId.Unit]);
                ResolveFieldMappedValue(observation.Occurrence?.EstablishmentMeans,
                    valueMappingDictionaries[FieldMappingFieldId.EstablishmentMeans]);
                ResolveFieldMappedValue(observation.Occurrence?.OccurrenceStatus,
                    valueMappingDictionaries[FieldMappingFieldId.OccurrenceStatus]);
            }
        }

        private void ResolveFieldMappedValue(
            ProcessedFieldMapValue fieldMapValue,
            Dictionary<int, string> valueById)
        {
            if (fieldMapValue == null) return;
            if (fieldMapValue.Id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId
                && valueById.TryGetValue(fieldMapValue.Id, out var translatedValue))
            {
                fieldMapValue.Value = translatedValue;
            }
        }

    }
}