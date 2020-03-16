﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using DwC_A.Terms;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Enums.FieldMappingValues;
using SOS.Lib.Models.Processed.Sighting;
using Xunit;

namespace SOS.Import.IntegrationTests.DwcImport
{
    public class ImportDwcFileIntegrationTests
    {
        private const string ArchiveFileName = "./resources/psophus-stridulus-lifewatch-occurrences-dwca.zip";

        [Fact]
        public void ShouldOpenCoreFile()
        {
            using (var archive = new ArchiveReader(ArchiveFileName))
            {
                foreach (var row in archive.CoreFile.Rows)
                {
                    Assert.NotNull(row[0]);
                }
            }
        }

        [Fact]
        public async Task ShouldOpenCoreFileAsync()
        {
            using (var archive = new ArchiveReader(ArchiveFileName))
            {
                await foreach (IRow row in archive.GetAsyncCoreFile().GetDataRowsAsync())
                {
                    
                    Assert.NotNull(row[0]);
                }
            }
        }

        [Fact]
        public void TestValidateFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var valid = TryValidateDwCACoreFile(ArchiveFileName, out var nrRows, out _);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            valid.Should().BeTrue();
            nrRows.Should().Be(2158);
        }

        [Fact]
        public void TestReadFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var records = ReadArchive(ArchiveFileName);
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            records.Should().NotBeNull();
            records.Count.Should().Be(2158);
        }

        private List<HarvestDarwinCore> ReadArchive(string archiveFileName)
        {
            using var archive = new ArchiveReader(archiveFileName);
            bool dwcIndexSpecified = archive.CoreFile.FileMetaData.Id.IndexSpecified;
            var dataRows = archive.CoreFile.DataRows;
            List<HarvestDarwinCore> verbatimRecords = new List<HarvestDarwinCore>();

            foreach (var row in dataRows)
            {
                HarvestDarwinCore verbatimRecord = HarvestDarwinCoreFactory.Create(row);
                string catalogNumber = null;

                if (dwcIndexSpecified)
                {
                    catalogNumber = row[archive.CoreFile.FileMetaData.Id.Index];
                }
                else
                {
                    catalogNumber = null; //verbatimRecord.CatalogNumber;
                }

                if (string.IsNullOrEmpty(catalogNumber))
                {
                    throw new Exception("Could not parse the catalog number for the verbatimRecord.");
                }

                verbatimRecords.Add(verbatimRecord);
            }

            return verbatimRecords;
        }

        private bool TryValidateDwCACoreFile(string filePath, out long nrRows, out string message)
        {
            nrRows = 0;
            message = null;

            try
            {
                using var archive = new ArchiveReader(filePath);
                nrRows = archive.CoreFile.DataRows.LongCount();

                if (archive.CoreFile.FileMetaData.Id.IndexSpecified == false)
                {
                    message = "Core file is missing index of id.";
                    return false;
                }

                if (nrRows == 0)
                {
                    message = "No data rows in core file";
                    return false;
                }
            }
            catch (Exception e)
            {
                message = e.Message;
                return false;
            }

            return true;
        }
    }

    public class ProcessedSightingFactory
    {
        public static ProcessedSighting CreateFromDwcRow(IRow row)
        {
            if (row == null)
            {
                return null;
            }

            var obs = new ProcessedSighting(DataProvider.Dwc);
            
            if (row.TryGetValue(Terms.accessRights, out string accessRights))
            {
                obs.AccessRightsId = ProcessedFieldMapValue.Create(accessRights); // todo try parse field mapped value.
                //AccessRightsId.FreeUsage
                //AccessRightsId.NotForPublicUsage
            }

            if (row.TryGetValue(Terms.basisOfRecord, out string basisOfRecord))
            {
                obs.BasisOfRecordId = ProcessedFieldMapValue.Create(basisOfRecord); // todo try parse field mapped value.
                //BasisOfRecordId.Occurrence
                //BasisOfRecordId.PreservedSpecimen
                //BasisOfRecordId.MachineObservation
            }
            
            if (row.TryGetValue(Terms.collectionCode, out string collectionCode))
            {
                obs.CollectionCode = collectionCode;
            }

            if (row.TryGetValue(Terms.collectionID, out string collectionId))
            {
                obs.CollectionId = collectionId;
            }

            if (row.TryGetValue(Terms.datasetID, out string datasetId))
            {
                obs.DatasetId = datasetId;
            }

            if (row.TryGetValue(Terms.datasetName, out string datasetName))
            {
                obs.DatasetName = datasetName;
            }

            //-----------------------------------------------------------------------------------------------------------
            // Event
            //-----------------------------------------------------------------------------------------------------------
            // Not handled:
            //obs.Event.BiotopeDescription = verbatim.BiotopeDescription;
            //obs.Event.QuantityOfSubstrate = verbatim.QuantityOfSubstrate;


            obs.Event = new ProcessedEvent();
            if (row.TryGetValue(Terms.samplingProtocol, out string samplingProtocol))
            {
                obs.Event.SamplingProtocol = samplingProtocol;
            }

            
            //obs.Event.EndDate = verbatim.EndDate?.ToUniversalTime();
            //obs.Event.StartDate = verbatim.StartDate?.ToUniversalTime();
            //obs.Event.SubstrateSpeciesDescription = verbatim.SubstrateSpeciesDescription;
            //obs.Event.SubstrateDescription = GetSubstrateDescription(verbatim, taxa);
            //obs.Event.VerbatimEndDate = verbatim.EndDate;
            //obs.Event.VerbatimStartDate = verbatim.StartDate;



            //var taxonId = verbatim.TaxonId ?? -1;

            //var hasPosition = (verbatim.Site?.XCoord ?? 0) > 0 && (verbatim.Site?.YCoord ?? 0) > 0;

            //if (taxa.TryGetValue(taxonId, out var taxon))
            //{
            //    taxon.IndividualId = verbatim.URL;
            //}

            //var obs = new ProcessedSighting(DataProvider.Dwc);
            //obs.Event = new ProcessedEvent();
            //obs.Event.BiotopeDescription = verbatim.BiotopeDescription;
            //obs.Event.EndDate = verbatim.EndDate?.ToUniversalTime();
            //obs.Event.QuantityOfSubstrate = verbatim.QuantityOfSubstrate;
            //obs.Event.SamplingProtocol = GetSamplingProtocol(verbatim.Projects);
            //obs.Event.StartDate = verbatim.StartDate?.ToUniversalTime();
            //obs.Event.SubstrateSpeciesDescription = verbatim.SubstrateSpeciesDescription;
            //obs.Event.SubstrateDescription = GetSubstrateDescription(verbatim, taxa);
            //obs.Event.VerbatimEndDate = verbatim.EndDate;
            //obs.Event.VerbatimStartDate = verbatim.StartDate;
            //obs.Identification = new ProcessedIdentification();
            //obs.Identification.IdentifiedBy = verbatim.VerifiedBy;
            //obs.Identification.Validated = new[] { 60, 61, 62, 63, 64, 65 }.Contains(verbatim.ValidationStatus?.Id ?? 0);
            //obs.Identification.UncertainDetermination = verbatim.UnsureDetermination;
            //obs.InformationWithheld = null;
            //obs.IsInEconomicZoneOfSweden = hasPosition;
            //obs.Language = Language.Swedish;
            //obs.Location = new ProcessedLocation();
            //obs.Location.ContinentId = new ProcessedFieldMapValue { Id = (int)ContinentId.Europe };
            //obs.Location.CoordinateUncertaintyInMeters = verbatim.Site?.Accuracy;
            //obs.Location.CountryId = new ProcessedFieldMapValue { Id = (int)CountryId.Sweden };
            //obs.Location.CountryCode = CountryCode.Sweden;
            //obs.Location.DecimalLatitude = verbatim.Site?.Point?.Coordinates?.Latitude ?? 0;
            //obs.Location.DecimalLongitude = verbatim.Site?.Point?.Coordinates?.Longitude ?? 0;
            //obs.Location.GeodeticDatum = GeodeticDatum.Wgs84;
            //obs.Location.Locality = verbatim.Site?.Name;
            //obs.Location.Id = $"urn:lsid:artportalen.se:site:{verbatim.Site?.Id}";
            //obs.Location.MaximumDepthInMeters = verbatim.MaxDepth;
            //obs.Location.MaximumElevationInMeters = verbatim.MaxHeight;
            //obs.Location.MinimumDepthInMeters = verbatim.MinDepth;
            //obs.Location.MinimumElevationInMeters = verbatim.MinHeight;
            //obs.Location.Point = verbatim.Site?.Point;
            //obs.Location.PointWithBuffer = verbatim.Site?.PointWithBuffer;
            //obs.Location.VerbatimLatitude = hasPosition ? verbatim.Site.YCoord : 0;
            //obs.Location.VerbatimLongitude = hasPosition ? verbatim.Site.XCoord : 0;
            //obs.Location.VerbatimCoordinateSystem = "EPSG:3857";
            //obs.Modified = verbatim.EndDate ?? verbatim.ReportedDate;
            //obs.Occurrence = new ProcessedOccurrence();
            //obs.Occurrence.AssociatedMedia = verbatim.HasImages
            //    ? $"http://www.artportalen.se/sighting/{verbatim.Id}#SightingDetailImages"
            //    : "";
            //obs.Occurrence.AssociatedReferences = GetAssociatedReferences(verbatim);
            //obs.Occurrence.BirdNestActivityId = GetBirdNestActivityId(verbatim, taxon);
            //obs.Occurrence.CatalogNumber = verbatim.Id.ToString();
            //obs.Occurrence.Id = $"urn:lsid:artportalen.se:Sighting:{verbatim.Id}";
            //obs.Occurrence.IndividualCount = verbatim.Quantity?.ToString() ?? "";
            //obs.Occurrence.IsNaturalOccurrence = !verbatim.Unspontaneous;
            //obs.Occurrence.IsNeverFoundObservation = verbatim.NotPresent;
            //obs.Occurrence.IsNotRediscoveredObservation = verbatim.NotRecovered;
            //obs.Occurrence.IsPositiveObservation = !(verbatim.NotPresent || verbatim.NotRecovered);
            //obs.Occurrence.OrganismQuantity = verbatim.Quantity;
            //obs.Occurrence.RecordedBy = verbatim.Observers;
            //obs.Occurrence.RecordNumber = verbatim.Label;
            //obs.Occurrence.Remarks = verbatim.Comment;
            //obs.Occurrence.OccurrenceStatusId = verbatim.NotPresent || verbatim.NotRecovered
            //    ? new ProcessedFieldMapValue { Id = (int)OccurrenceStatusId.Absent }
            //    : new ProcessedFieldMapValue { Id = (int)OccurrenceStatusId.Present };
            //obs.Occurrence.URL = $"http://www.artportalen.se/sighting/{verbatim.Id}";
            //obs.OwnerInstitutionCode = verbatim.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "Artdatabanken";
            //obs.Projects = verbatim.Projects?.ToProcessedProjects();
            //obs.ProtectionLevel = CalculateProtectionLevel(taxon, verbatim.HiddenByProvider, verbatim.ProtectedBySystem);
            //obs.ReportedBy = verbatim.ReportedBy;
            //obs.ReportedDate = verbatim.ReportedDate;
            //obs.RightsHolder = verbatim.RightsHolder ?? verbatim.OwnerOrganization?.Translate(Cultures.en_GB, Cultures.sv_SE) ?? "Data saknas";
            //obs.Taxon = taxon;
            //obs.TypeId = null;

            //// Get field mapping values
            //obs.Occurrence.GenderId = GetSosId(verbatim.Gender?.Id, fieldMappings[FieldMappingFieldId.Gender]);
            //obs.Occurrence.ActivityId = GetSosId(verbatim.Activity?.Id, fieldMappings[FieldMappingFieldId.Activity]);
            //obs.Location.CountyId = GetSosId(verbatim.Site?.County?.Id, fieldMappings[FieldMappingFieldId.County]);
            //obs.Location.MunicipalityId = GetSosId(verbatim.Site?.Municipality?.Id, fieldMappings[FieldMappingFieldId.Municipality]);
            //obs.Location.ProvinceId = GetSosId(verbatim.Site?.Province?.Id, fieldMappings[FieldMappingFieldId.Province]);
            //obs.Location.ParishId = GetSosId(verbatim.Site?.Parish?.Id, fieldMappings[FieldMappingFieldId.Parish]);
            //obs.Event.BiotopeId = GetSosId(verbatim?.Bioptope?.Id, fieldMappings[FieldMappingFieldId.Biotope]);
            //obs.Event.SubstrateId = GetSosId(verbatim?.Bioptope?.Id, fieldMappings[FieldMappingFieldId.Substrate]);
            //obs.Identification.ValidationStatusId = GetSosId(verbatim?.ValidationStatus?.Id, fieldMappings[FieldMappingFieldId.ValidationStatus]);
            //obs.Occurrence.LifeStageId = GetSosId(verbatim?.Stage?.Id, fieldMappings[FieldMappingFieldId.LifeStage]);
            //obs.OrganizationId = GetSosId(verbatim?.OwnerOrganization?.Id, fieldMappings[FieldMappingFieldId.Organization]);
            //obs.Occurrence.OrganismQuantityUnitId = GetSosId(verbatim?.Unit?.Id, fieldMappings[FieldMappingFieldId.Unit]);
            return obs;
        }
    }



    public class HarvestDarwinCoreFactory
    {
        public static HarvestDarwinCore Create(IRow row)
        {
            var result = new HarvestDarwinCore();

            if (row.TryGetValue(Terms.acceptedNameUsage, out string acceptedNameUsage))
            {
                result.AcceptedNameUsage = acceptedNameUsage;
            }

            if (row.TryGetValue(Terms.basisOfRecord, out string basisOfRecord))
            {
                result.BasisOfRecord = basisOfRecord;
            }

            if (row.TryGetValue(Terms.occurrenceID, out string occurrenceId))
            {
                result.OccurrenceID = occurrenceId;
            }

            if (row.TryGetValue(Terms.catalogNumber, out string catalogNumber))
            {
                result.CatalogNumber = catalogNumber;
            }

            return result;

            // todo - add more mappings

            //result.AcceptedNameUsage = row[Terms.acceptedNameUsage];
            //result.AcceptedNameUsageID = row[Terms.acceptedNameUsageID];
            //result.AccessRights = row[Terms.accessRights];
            //result.AssociatedMedia = row[Terms.associatedMedia];
            //result.AssociatedOccurrences = row[Terms.associatedOccurrences];
            ////Terms.associatedOrganisms
            //result.AssociatedReferences = row[Terms.associatedReferences];
            //result.AssociatedSequences = row[Terms.associatedSequences];
            //result.AssociatedTaxa = row[Terms.associatedTaxa];
            //result.BasisOfRecord = row[Terms.basisOfRecord];
            //result.Bed = row[Terms.bed];
            //result.Behavior = row[Terms.behavior];
            //result.BibliographicCitation = row[Terms.bibliographicCitation];
            //result.CatalogNumber = row[Terms.catalogNumber];
            //result.Class = row[Terms.@class];
            //result.CollectionCode = row[Terms.collectionCode];
            //result.CollectionID = row[Terms.collectionID];
            //result.Continent = row[Terms.continent];
            //result.CoordinatePrecision = row[Terms.coordinatePrecision];
            //result.CoordinateUncertaintyInMeters = row[Terms.coordinateUncertaintyInMeters];
            //result.Country = row[Terms.country];
            //result.CountryCode = row[Terms.countryCode];
            //result.County = row[Terms.country];
            //result.DataGeneralizations = row[Terms.dataGeneralizations];
            //result.DatasetID = row[Terms.datasetID];
            //result.DatasetName = row[Terms.datasetName];
            //result.DateIdentified = row[Terms.dateIdentified];
            //result.Day = row[Terms.day];
            //result.DecimalLatitude = row[Terms.decimalLatitude];
            //result.DecimalLongitude = row[Terms.decimalLongitude];
            //result.Disposition = row[Terms.disposition];
            //result.DynamicProperties = row[Terms.dynamicProperties];
            //result.EarliestAgeOrLowestStage = row[Terms.earliestAgeOrLowestStage];
            //result.EarliestEonOrLowestEonothem = row[Terms.earliestEonOrLowestEonothem];
            //result.EarliestEpochOrLowestSeries = row[Terms.earliestEpochOrLowestSeries];
            //result.EarliestEraOrLowestErathem = row[Terms.earliestEraOrLowestErathem];
            //result.EarliestPeriodOrLowestSystem = row[Terms.earliestPeriodOrLowestSystem];
            ////Tems.earliestGeochronologicalEra
            //result.EndDayOfYear = row[Terms.endDayOfYear];
            //result.EstablishmentMeans = row[Terms.establishmentMeans];
            //result.EventID = row[Terms.eventID];
            //result.EventRemarks = row[Terms.eventRemarks];
            //result.EventTime = row[Terms.eventTime];
            //result.Family = row[Terms.family];
            //result.FieldNotes = row[Terms.fieldNotes];
            //result.FieldNumber = row[Terms.fieldNumber];
            //result.FootprintSpatialFit = row[Terms.footprintSpatialFit];
            //result.FootprintSRS = row[Terms.footprintSRS];
            //result.FootprintWKT = row[Terms.footprintWKT];
            //result.Genus = row[Terms.genus];
            //result.GeodeticDatum = row[Terms.geodeticDatum];
            ////Terms.geologicalContext
            //result.GeologicalContextID = row[Terms.geologicalContextID];
            //result.GeoreferencedBy = row[Terms.georeferencedBy];
            //result.GeoreferencedDate = row[Terms.georeferencedDate];
            //result.GeoreferenceProtocol = row[Terms.georeferenceProtocol];
            //result.GeoreferenceRemarks = row[Terms.georeferenceRemarks];
            //result.GeoreferenceSources = row[Terms.georeferenceSources];
            //result.GeoreferenceVerificationStatus = row[Terms.georeferenceVerificationStatus];
            //result.Group = row[Terms.group];
            //result.Habitat = row[Terms.habitat];
            //result.HigherClassification = row[Terms.higherClassification];
            //result.HigherGeography = row[Terms.higherGeography];
            //result.HigherGeographyID = row[Terms.higherGeographyID];
            //result.HighestBiostratigraphicZone = row[Terms.highestBiostratigraphicZone];
            //result.IdentificationID = row[Terms.identificationID];
            //result.IdentificationQualifier = row[Terms.identificationQualifier];
            //result.IdentificationReferences = row[Terms.identificationReferences];
            //result.IdentificationRemarks = row[Terms.identificationRemarks];
            //result.IdentificationVerificationStatus = row[Terms.identificationVerificationStatus];
            //result.IdentifiedBy = row[Terms.identifiedBy];
            //result.IndividualCount = row[Terms.individualCount];
            ////-->>result.IndividualID 
            //result.InformationWithheld = row[Terms.informationWithheld];
            //result.InfraspecificEpithet = row[Terms.infraspecificEpithet];
            //result.InstitutionCode = row[Terms.institutionCode];
            //result.InstitutionID = row[Terms.institutionID];
            //result.Island = row[Terms.island];
            //result.IslandGroup = row[Terms.islandGroup];
            //result.Kingdom = row[Terms.kingdom];
            //result.Language = row[Terms.language];
            //result.LatestAgeOrHighestStage = row[Terms.latestAgeOrHighestStage];
            //result.LatestEonOrHighestEonothem = row[Terms.latestEonOrHighestEonothem];
            //result.LatestEpochOrHighestSeries = row[Terms.latestEpochOrHighestSeries];
            //result.LatestEraOrHighestErathem = row[Terms.latestEraOrHighestErathem];
            ////Terms.latestGeochronologicalEra
            //result.LatestPeriodOrHighestSystem = row[Terms.latestPeriodOrHighestSystem];
            //result.LifeStage = row[Terms.lifeStage];
            ////Terms.license
            //result.LithostratigraphicTerms = row[Terms.lithostratigraphicTerms];
            //result.Locality = row[Terms.locality];
            ////Terms.Location
            //result.LocationAccordingTo = row[Terms.locationAccordingTo];
            //result.LocationId = row[Terms.locationID];
            //result.LocationRemarks = row[Terms.locationRemarks];
            //result.LowestBiostratigraphicZone = row[Terms.lowestBiostratigraphicZone];
            ////Terms.MachineObservation
            ////Terms.MaterialSample
            ////Terms.materialSampleID
            //result.MaximumDepthInMeters = row[Terms.maximumDepthInMeters];
            //result.MaximumDistanceAboveSurfaceInMeters = row[Terms.maximumDistanceAboveSurfaceInMeters];
            //result.MaximumElevationInMeters = row[Terms.maximumElevationInMeters];
            ////Terms.measurementMethod
            //result.Member = row[Terms.member];
            //result.MinimumDepthInMeters = row[Terms.minimumDepthInMeters];
            //result.MinimumDistanceAboveSurfaceInMeters = row[Terms.minimumDistanceAboveSurfaceInMeters];
            //result.MinimumElevationInMeters = row[Terms.minimumElevationInMeters];
            //result.Modified = row[Terms.modified];
            //result.Month = row[Terms.month];
            //result.Municipality = row[Terms.municipality];
            //result.NameAccordingTo = row[Terms.nameAccordingTo];
            //result.NameAccordingToID = row[Terms.nameAccordingToID];
            //result.NamePublishedIn = row[Terms.namePublishedIn];
            //result.NamePublishedInID = row[Terms.namePublishedInID];
            //result.NamePublishedInYear = row[Terms.namePublishedInYear];
            //result.NomenclaturalCode = row[Terms.nomenclaturalCode];
            //result.NomenclaturalStatus = row[Terms.nomenclaturalStatus];
            ////Terms.Occurrence
            //result.OccurrenceID = row[Terms.occurrenceID];
            //result.OccurrenceRemarks = row[Terms.occurrenceRemarks];
            //result.OccurrenceStatus = row[Terms.occurrenceStatus];
            //result.Order = row[Terms.order];
            ////Terms.Organism x 7
            //result.OriginalNameUsage = row[Terms.originalNameUsage];
            //result.OriginalNameUsageID = row[Terms.originalNameUsageID];
            //result.OtherCatalogNumbers = row[Terms.otherCatalogNumbers];
            //result.OwnerInstitutionCode = row[Terms.ownerInstitutionCode];
            ////Terms.parentEventID
            //result.ParentNameUsage = row[Terms.parentNameUsage];
            //result.ParentNameUsageID = row[Terms.parentNameUsageID];
            //result.Phylum = row[Terms.phylum];
            //result.PointRadiusSpatialFit = row[Terms.pointRadiusSpatialFit];
            //result.Preparations = row[Terms.preparations];
            //result.PreviousIdentifications = row[Terms.previousIdentifications];
            //result.RecordedBy = row[Terms.recordedBy];
            //result.RecordNumber = row[Terms.recordNumber];
            //result.References = row[Terms.references];
            ////Terms.relatedResourceID
            ////Terms.relationshipAccordingTo x 4            
            //result.ReproductiveCondition = row[Terms.reproductiveCondition];
            //result.RightsHolder = row[Terms.rightsHolder];
            ////Terms.sampleSizeUnit x 2
            //result.SamplingEffort = row[Terms.samplingEffort];
            //result.SamplingProtocol = row[Terms.samplingProtocol];
            //result.ScientificName = row[Terms.scientificName];
            //result.ScientificNameAuthorship = row[Terms.scientificNameAuthorship];
            //result.ScientificNameID = row[Terms.scientificNameID];
            //result.Sex = row[Terms.sex];
            //result.SpecificEpithet = row[Terms.specificEpithet];
            //result.StartDayOfYear = row[Terms.startDayOfYear];
            //result.StateProvince = row[Terms.stateProvince];
            //result.Subgenus = row[Terms.subgenus];
            //result.TaxonConceptID = row[Terms.taxonConceptID];
            ////Terms.Taxon
            //result.TaxonID = row[Terms.taxonID];
            //result.TaxonomicStatus = row[Terms.taxonomicStatus];
            //result.TaxonRank = row[Terms.taxonRank];
            //result.TaxonRemarks = row[Terms.taxonRemarks];
            //result.Type = row[Terms.type];
            //result.TypeStatus = row[Terms.typeStatus];
            ////Terms.UseWithIRI
            //result.VerbatimCoordinates = row[Terms.verbatimCoordinates];
            //result.VerbatimCoordinateSystem = row[Terms.verbatimCoordinateSystem];
            //result.VerbatimDepth = row[Terms.verbatimDepth];
            //result.VerbatimElevation = row[Terms.verbatimElevation];
            //result.VerbatimEventDate = row[Terms.verbatimEventDate];
            //result.VerbatimLatitude = row[Terms.verbatimLatitude];
            //result.VerbatimLocality = row[Terms.verbatimLocality];
            //result.VerbatimLongitude = row[Terms.verbatimLongitude];
            //result.VerbatimSRS = row[Terms.verbatimSRS];
            //result.VerbatimTaxonRank = row[Terms.verbatimTaxonRank];
            //result.VernacularName = row[Terms.vernacularName];
            //result.WaterBody = row[Terms.waterBody];
            //result.Year = row[Terms.year];

            //return result;
        }

    }

    public class HarvestDarwinCore
    {
        #region RecordLevel
        /// <summary>
        /// Darwin Core term name: dcterms:accessRights.
        /// Information about who can access the resource or
        /// an indication of its security status.
        /// Access Rights may include information regarding
        /// access or restrictions based on privacy, security,
        /// or other policies.
        /// </summary>
        public string AccessRights { get; set; }

        /// <summary>
        /// Darwin Core term name: basisOfRecord.
        /// The specific nature of the data record -
        /// a subtype of the dcterms:type.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Darwin Core Type Vocabulary
        /// (http://rs.tdwg.org/dwc/terms/type-vocabulary/index.htm).
        /// In Species Gateway this property has the value
        /// HumanObservation.
        /// </summary>
        public string BasisOfRecord { get; set; }

        /// <summary>
        /// Darwin Core term name: dcterms:bibliographicCitation.
        /// A bibliographic reference for the resource as a statement
        /// indicating how this record should be cited (attributed)
        /// when used.
        /// Recommended practice is to include sufficient
        /// bibliographic detail to identify the resource as
        /// unambiguously as possible.
        /// This property is currently not used.
        /// </summary>
        public string BibliographicCitation { get; set; }

        /// <summary>
        /// Darwin Core term name: collectionCode.
        /// The name, acronym, coden, or initialism identifying the 
        /// collection or data set from which the record was derived.
        /// </summary>
        public string CollectionCode { get; set; }

        /// <summary>
        /// Darwin Core term name: collectionID.
        /// An identifier for the collection or dataset from which
        /// the record was derived.
        /// For physical specimens, the recommended best practice is
        /// to use the identifier in a collections registry such as
        /// the Biodiversity Collections Index
        /// (http://www.biodiversitycollectionsindex.org/).
        /// </summary>
        public string CollectionID { get; set; }

        /// <summary>
        /// Darwin Core term name: dataGeneralizations.
        /// Actions taken to make the shared data less specific or
        /// complete than in its original form.
        /// Suggests that alternative data of higher quality
        /// may be available on request.
        /// This property is currently not used.
        /// </summary>
        public string DataGeneralizations { get; set; }

        /// <summary>
        /// Darwin Core term name: datasetID.
        /// An identifier for the set of data.
        /// May be a global unique identifier or an identifier
        /// specific to a collection or institution.
        /// </summary>
        public string DatasetID { get; set; }

        /// <summary>
        /// Darwin Core term name: datasetName.
        /// The name identifying the data set
        /// from which the record was derived.
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        /// Darwin Core term name: dynamicProperties.
        /// A list (concatenated and separated) of additional
        /// measurements, facts, characteristics, or assertions
        /// about the record. Meant to provide a mechanism for
        /// structured content such as key-value pairs.
        /// This property is currently not used.
        /// </summary>
        public string DynamicProperties { get; set; }

        /// <summary>
        /// Darwin Core term name: informationWithheld.
        /// Additional information that exists, but that has
        /// not been shared in the given record.
        /// This property is currently not used.
        /// </summary>
        public string InformationWithheld { get; set; }

        /// <summary>
        /// Darwin Core term name: institutionCode.
        /// The name (or acronym) in use by the institution
        /// having custody of the object(s) or information
        /// referred to in the record.
        /// Currently this property has the value ArtDatabanken.
        /// </summary>
        public string InstitutionCode { get; set; }

        /// <summary>
        /// Darwin Core term name: institutionID.
        /// An identifier for the institution having custody of 
        /// the object(s) or information referred to in the record.
        /// This property is currently not used.
        /// </summary>
        public string InstitutionID { get; set; }

        /// <summary>
        /// Darwin Core term name: dcterms:language.
        /// A language of the resource.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as RFC 4646 [RFC4646].
        /// This property is currently not used.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Darwin Core term name: dcterms:modified.
        /// The most recent date-time on which the resource was changed.
        /// For Darwin Core, recommended best practice is to use an
        /// encoding scheme, such as ISO 8601:2004(E).
        /// </summary>
        public string Modified { get; set; }

        /// <summary>
        /// Darwin Core term name: ownerInstitutionCode.
        /// The name (or acronym) in use by the institution having
        /// ownership of the object(s) or information referred
        /// to in the record.
        /// This property is currently not used.
        /// </summary>
        public string OwnerInstitutionCode { get; set; }

        /// <summary>
        /// Darwin Core term name: dcterms:references.
        /// A related resource that is referenced, cited,
        /// or otherwise pointed to by the described resource.
        /// This property is currently not used.
        /// </summary>
        public string References { get; set; }

        /// <summary>
        /// Darwin Core term name: dcterms:rightsHolder.
        /// A person or organization owning or
        /// managing rights over the resource.
        /// This property is currently not used.
        /// </summary>
        public string RightsHolder { get; set; }

        /// <summary>
        /// Darwin Core term name: dcterms:type.
        /// The nature or genre of the resource.
        /// For Darwin Core, recommended best practice is
        /// to use the name of the class that defines the
        /// root of the record.
        /// This property is currently not used.
        /// </summary>
        public string Type { get; set; }
        #endregion

        #region Conservation
        /// <summary>
        /// Not defined in Darwin Core.
        /// This property indicates whether the species is the subject
        /// of an action plan ('åtgärdsprogram' in swedish).
        /// </summary>
        //public bool ActionPlan { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// This property indicates whether a species has been
        /// classified as nature conservation relevant
        /// ('naturvårdsintressant' in swedish).
        /// The concept 'nature conservation relevant' must be defined
        /// before this property can be used.
        /// </summary>
        //public bool ConservationRelevant { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// This property indicates whether
        /// the species is included in Natura 2000.
        /// </summary>
        //public bool Natura2000 { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// This property indicates whether the species 
        /// is protected by the law in Sweden.
        /// </summary>
        //public bool ProtectedByLaw { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Information about how protected information
        /// about a species is in Sweden.
        /// Currently this is a value between 1 to 5.
        /// 1 indicates public access and 5 is the highest used security level.
        /// </summary>
        //public int ProtectionLevel { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Redlist category for redlisted species. The property also
        /// contains information about which redlist that is referenced.
        /// Example value: CR (Sweden, 2010). Possible redlist values
        /// are DD (Data Deficient), EX (Extinct),
        /// RE (Regionally Extinct), CR (Critically Endangered),
        /// EN (Endangered), VU (Vulnerable), NT (Near Threatened).
        /// Not redlisted species has no value in this property.
        /// </summary>
        //public string RedlistCategory { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// This property contains information about the species
        /// immigration history.
        /// </summary>
        //public string SwedishImmigrationHistory { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Information about the species occurrence in Sweden.
        /// For example information about if the species reproduce
        /// in sweden.
        /// </summary>
        //public string SwedishOccurrence { get; set; }
        #endregion

        #region Event
        /// <summary>
        /// Darwin Core term name: day.
        /// The integer day of the month on which the Event occurred
        /// (start date of observation).
        /// This property is currently not used.
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Information about date and time when the
        /// species observation ended.
        /// </summary>
        //public DateTime End { get; set; }

        /// <summary>
        /// Darwin Core term name: endDayOfYear.
        /// The latest ordinal day of the year on which the Event
        /// occurred (1 for January 1, 365 for December 31,
        /// except in a leap year, in which case it is 366).
        /// This property is currently not used.
        /// </summary>
        public string EndDayOfYear { get; set; }

        /// <summary>
        /// Darwin Core term name: eventDate.
        /// The date-time or interval during which an Event occurred.
        /// For occurrences, this is the date-time when the event
        /// was recorded. Not suitable for a time in a geological
        /// context. Recommended best practice is to use an encoding
        /// scheme, such as ISO 8601:2004(E).
        /// For example: ”2007-03-01 13:00:00 - 2008-05-11 15:30:00”
        /// This property is currently not used.
        /// </summary>
        //public string EventDate { get; set; }

        /// <summary>
        /// Darwin Core term name: eventID.
        /// A list (concatenated and separated) of identifiers
        /// (publication, global unique identifier, URI) of
        /// media associated with the Occurrence.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string EventID { get; set; }

        /// <summary>
        /// Darwin Core term name: eventRemarks.
        /// Comments or notes about the Event.
        /// This property is currently not used.
        /// </summary>
        public string EventRemarks { get; set; }

        /// <summary>
        /// Darwin Core term name: eventTime.
        /// The time or interval during which an Event occurred.
        /// Recommended best practice is to use an encoding scheme,
        /// such as ISO 8601:2004(E).
        /// For example: ”13:00:00 - 15:30:00”
        /// This property is currently not used.
        /// </summary>
        public string EventTime { get; set; }

        /// <summary>
        /// Darwin Core term name: fieldNotes.
        /// One of a) an indicator of the existence of, b) a
        /// reference to (publication, URI), or c) the text of
        /// notes taken in the field about the Event.
        /// This property is currently not used.
        /// </summary>
        public string FieldNotes { get; set; }

        /// <summary>
        /// Darwin Core term name: fieldNumber.
        /// An identifier given to the event in the field. Often 
        /// serves as a link between field notes and the Event.
        /// This property is currently not used.
        /// </summary>
        public string FieldNumber { get; set; }

        /// <summary>
        /// Darwin Core term name: habitat.
        /// A category or description of the habitat
        /// in which the Event occurred.
        /// This property is currently not used.
        /// </summary>
        public string Habitat { get; set; }

        /// <summary>
        /// Darwin Core term name: month.
        /// The ordinal month in which the Event occurred.
        /// This property is currently not used.
        /// </summary>
        public string Month { get; set; }

        /// <summary>
        /// Darwin Core term name: samplingEffort.
        /// The amount of effort expended during an Event.
        /// This property is currently not used.
        /// </summary>
        public string SamplingEffort { get; set; }

        /// <summary>
        /// Darwin Core term name: samplingProtocol.
        /// The name of, reference to, or description of the
        /// method or protocol used during an Event.
        /// This property is currently not used.
        /// </summary>
        public string SamplingProtocol { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Information about date and time when the
        /// species observation started.
        /// </summary>
        //public DateTime Start { get; set; }

        /// <summary>
        /// Darwin Core term name: startDayOfYear.
        /// The earliest ordinal day of the year on which the
        /// Event occurred (1 for January 1, 365 for December 31,
        /// except in a leap year, in which case it is 366).
        /// This property is currently not used.
        /// </summary>
        public string StartDayOfYear { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimEventDate.
        /// The verbatim original representation of the date
        /// and time information for an Event.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimEventDate { get; set; }

        /// <summary>
        /// Darwin Core term name: year.
        /// The four-digit year in which the Event occurred,
        /// according to the Common Era Calendar.
        /// This property is currently not used.
        /// </summary>
        public string Year { get; set; }
        #endregion

        #region GeologicalContext
        /// <summary>
        /// Darwin Core term name: bed.
        /// The full name of the lithostratigraphic bed from which
        /// the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string Bed { get; set; }

        /// <summary>
        /// Darwin Core term name: earliestAgeOrLowestStage.
        /// The full name of the earliest possible geochronologic
        /// age or lowest chronostratigraphic stage attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string EarliestAgeOrLowestStage { get; set; }

        /// <summary>
        /// Darwin Core term name: earliestEonOrLowestEonothem.
        /// The full name of the earliest possible geochronologic eon
        /// or lowest chrono-stratigraphic eonothem or the informal
        /// name ("Precambrian") attributable to the stratigraphic
        /// horizon from which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string EarliestEonOrLowestEonothem { get; set; }

        /// <summary>
        /// Darwin Core term name: earliestEpochOrLowestSeries.
        /// The full name of the earliest possible geochronologic
        /// epoch or lowest chronostratigraphic series attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string EarliestEpochOrLowestSeries { get; set; }

        /// <summary>
        /// Darwin Core term name: earliestEraOrLowestErathem.
        /// The full name of the earliest possible geochronologic
        /// era or lowest chronostratigraphic erathem attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string EarliestEraOrLowestErathem { get; set; }

        /// <summary>
        /// Darwin Core term name: earliestPeriodOrLowestSystem.
        /// The full name of the earliest possible geochronologic
        /// period or lowest chronostratigraphic system attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string EarliestPeriodOrLowestSystem { get; set; }

        /// <summary>
        /// Darwin Core term name: formation.
        /// The full name of the lithostratigraphic formation from
        /// which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string Formation { get; set; }

        /// <summary>
        /// Darwin Core term name: geologicalContextID.
        /// An identifier for the set of information associated
        /// with a GeologicalContext (the location within a geological
        /// context, such as stratigraphy). May be a global unique
        /// identifier or an identifier specific to the data set.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string GeologicalContextID { get; set; }

        /// <summary>
        /// Darwin Core term name: group.
        /// The full name of the lithostratigraphic group from
        /// which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Darwin Core term name: highestBiostratigraphicZone.
        /// The full name of the highest possible geological
        /// biostratigraphic zone of the stratigraphic horizon
        /// from which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string HighestBiostratigraphicZone { get; set; }

        /// <summary>
        /// Darwin Core term name: latestAgeOrHighestStage.
        /// The full name of the latest possible geochronologic
        /// age or highest chronostratigraphic stage attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LatestAgeOrHighestStage { get; set; }

        /// <summary>
        /// Darwin Core term name: latestEonOrHighestEonothem.
        /// The full name of the latest possible geochronologic eon
        /// or highest chrono-stratigraphic eonothem or the informal
        /// name ("Precambrian") attributable to the stratigraphic
        /// horizon from which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LatestEonOrHighestEonothem { get; set; }

        /// <summary>
        /// Darwin Core term name: latestEpochOrHighestSeries.
        /// The full name of the latest possible geochronologic
        /// epoch or highest chronostratigraphic series attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LatestEpochOrHighestSeries { get; set; }

        /// <summary>
        /// Darwin Core term name: latestEraOrHighestErathem.
        /// The full name of the latest possible geochronologic
        /// era or highest chronostratigraphic erathem attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LatestEraOrHighestErathem { get; set; }

        /// <summary>
        /// Darwin Core term name: latestPeriodOrHighestSystem.
        /// The full name of the latest possible geochronologic
        /// period or highest chronostratigraphic system attributable
        /// to the stratigraphic horizon from which the cataloged
        /// item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LatestPeriodOrHighestSystem { get; set; }

        /// <summary>
        /// Darwin Core term name: lithostratigraphicTerms.
        /// The combination of all litho-stratigraphic names for
        /// the rock from which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LithostratigraphicTerms { get; set; }

        /// <summary>
        /// Darwin Core term name: lowestBiostratigraphicZone.
        /// The full name of the lowest possible geological
        /// biostratigraphic zone of the stratigraphic horizon
        /// from which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string LowestBiostratigraphicZone { get; set; }

        /// <summary>
        /// Darwin Core term name: member.
        /// The full name of the lithostratigraphic member from
        /// which the cataloged item was collected.
        /// This property is currently not used.
        /// </summary>
        public string Member { get; set; }
        #endregion

        #region Identification
        /// <summary>
        /// Darwin Core term name: dateIdentified.
        /// The date on which the subject was identified as
        /// representing the Taxon. Recommended best practice is
        /// to use an encoding scheme, such as ISO 8601:2004(E).
        /// This property is currently not used.
        /// </summary>
        public string DateIdentified { get; set; }

        /// <summary>
        /// Darwin Core term name: identificationID.
        /// An identifier for the Identification (the body of
        /// information associated with the assignment of a scientific
        /// name). May be a global unique identifier or an identifier
        /// specific to the data set.
        /// This property is currently not used.
        /// </summary>
        public string IdentificationID { get; set; }

        /// <summary>
        /// Darwin Core term name: identificationQualifier.
        /// A brief phrase or a standard term ("cf.", "aff.") to
        /// express the determiner's doubts about the Identification.
        /// </summary>
        public string IdentificationQualifier { get; set; }

        /// <summary>
        /// Darwin Core term name: identificationReferences.
        /// A list (concatenated and separated) of references
        /// (publication, global unique identifier, URI) used in
        /// the Identification.
        /// This property is currently not used.
        /// </summary>
        public string IdentificationReferences { get; set; }

        /// <summary>
        /// Darwin Core term name: identificationRemarks.
        /// Comments or notes about the Identification.
        /// Contains for example information about that
        /// the observer is uncertain about which species
        /// that has been observed.
        /// </summary>
        public string IdentificationRemarks { get; set; }

        /// <summary>
        /// Darwin Core term name: identificationVerificationStatus.
        /// A categorical indicator of the extent to which the taxonomic
        /// identification has been verified to be correct.
        /// Recommended best practice is to use a controlled vocabulary
        /// such as that used in HISPID/ABCD.
        /// This property is currently not used.
        /// </summary>
        public string IdentificationVerificationStatus { get; set; }

        /// <summary>
        /// Darwin Core term name: identifiedBy.
        /// A list (concatenated and separated) of names of people,
        /// groups, or organizations who assigned the Taxon to the
        /// subject.
        /// </summary>
        public string IdentifiedBy { get; set; }

        /// <summary>
        /// Darwin Core term name: typeStatus.
        /// A list (concatenated and separated) of nomenclatural
        /// types (type status, typified scientific name, publication)
        /// applied to the subject.
        /// This property is currently not used.
        /// </summary>
        public string TypeStatus { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Indicates if the species observer himself is
        /// uncertain about the taxon determination.
        /// </summary>
        //public bool UncertainDetermination { get; set; }
        #endregion

        #region Location
        /// <summary>
        /// Darwin Core term name: continent.
        /// The name of the continent in which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographi
        /// Names or the ISO 3166 Continent code.
        /// This property is currently not used.
        /// </summary>
        public string Continent { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// M value that is part of a linear reference system.
        /// The properties CoordinateX, CoordinateY, CoordinateZ,
        /// CoordinateM and CoordinateSystemWkt defines where the
        /// species observation was made.
        /// </summary>
        //public string CoordinateM { get; set; }

        /// <summary>
        /// Darwin Core term name: CoordinatePrecision.
        /// A decimal representation of the precision of the coordinates
        /// given in the DecimalLatitude and DecimalLongitude.
        /// This property is currently not used.
        /// </summary>
        public string CoordinatePrecision { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Coordinate system wkt (Well-known text)
        /// as defined by OGC (Open Geospatial Consortium).
        /// The properties CoordinateX, CoordinateY, CoordinateZ,
        /// CoordinateM and CoordinateSystemWkt defines where the
        /// species observation was made.
        /// </summary>
        //public string CoordinateSystemWkt { get; set; }

        /// <summary>
        /// Darwin Core term name: coordinateUncertaintyInMeters.
        /// The horizontal distance (in meters) from the given
        /// CoordinateX and CoordinateY describing the
        /// smallest circle containing the whole of the Location.
        /// Leave the value empty if the uncertainty is unknown, cannot
        /// be estimated, or is not applicable (because there are
        /// no coordinates). Zero is not a valid value for this term.
        /// </summary>
        public string CoordinateUncertaintyInMeters { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// East-west value of the coordinate.
        /// The properties CoordinateX, CoordinateY, CoordinateZ,
        /// CoordinateM and CoordinateSystemWkt defines where the
        /// species observation was made.
        /// Which values that are valid depends on which
        /// coordinate system that is used.
        /// </summary>
        //public Double CoordinateX { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// North-south value of the coordinate.
        /// The properties CoordinateX, CoordinateY, CoordinateZ,
        /// CoordinateM and CoordinateSystemWkt defines where the
        /// species observation was made.
        /// Which values that are valid depends on which
        /// coordinate system that is used.
        /// </summary>
        //public Double CoordinateY { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Altitude value of the coordinate.
        /// The properties CoordinateX, CoordinateY, CoordinateZ,
        /// CoordinateM and CoordinateSystemWkt defines where the
        /// species observation was made.
        /// </summary>
        //public string CoordinateZ { get; set; }

        /// <summary>
        /// Darwin Core term name: country.
        /// The name of the country or major administrative unit
        /// in which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographic Names.
        /// This property is currently not used.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Darwin Core term name: countryCode.
        /// The standard code for the country in which the
        /// Location occurs.
        /// Recommended best practice is to use ISO 3166-1-alpha-2
        /// country codes.
        /// This property is currently not used.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Darwin Core term name: county.
        /// The full, unabbreviated name of the next smaller
        /// administrative region than stateProvince(county, shire,
        /// department, etc.) in which the Location occurs
        /// ('län' in swedish).
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// Darwin Core term name: decimalLatitude.
        /// Definition in Darwin Core:
        /// The geographic latitude (in decimal degrees, using
        /// the spatial reference system given in geodeticDatum)
        /// of the geographic center of a Location. Positive values
        /// are north of the Equator, negative values are south of it.
        /// Legal values lie between -90 and 90, inclusive.
        /// </summary>
        public string DecimalLatitude { get; set; }

        /// <summary>
        /// Darwin Core term name: decimalLongitude.
        /// Definition in Darwin Core:
        /// The geographic longitude (in decimal degrees, using
        /// the spatial reference system given in geodeticDatum)
        /// of the geographic center of a Location. Positive
        /// values are east of the Greenwich Meridian, negative
        /// values are west of it. Legal values lie between -180
        /// and 180, inclusive.
        /// </summary>
        public string DecimalLongitude { get; set; }

        /// <summary>
        /// Darwin Core term name: footprintSpatialFit.
        /// The ratio of the area of the footprint (footprintWKT)
        /// to the area of the true (original, or most specific)
        /// spatial representation of the Location. Legal values are
        /// 0, greater than or equal to 1, or undefined. A value of
        /// 1 is an exact match or 100% overlap. A value of 0 should
        /// be used if the given footprint does not completely contain
        /// the original representation. The footprintSpatialFit is
        /// undefined (and should be left blank) if the original
        /// representation is a point and the given georeference is
        /// not that same point. If both the original and the given
        /// georeference are the same point, the footprintSpatialFit
        /// is 1.
        /// This property is currently not used.
        /// </summary>
        public string FootprintSpatialFit { get; set; }

        /// <summary>
        /// Darwin Core term name: footprintSRS.
        /// A Well-Known Text (WKT) representation of the Spatial
        /// Reference System (SRS) for the footprintWKT of the
        /// Location. Do not use this term to describe the SRS of
        /// the decimalLatitude and decimalLongitude, even if it is
        /// the same as for the footprintWKT - use the geodeticDatum
        /// instead.
        /// This property is currently not used.
        /// </summary>
        public string FootprintSRS { get; set; }

        /// <summary>
        /// Darwin Core term name: footprintWKT.
        /// A Well-Known Text (WKT) representation of the shape
        /// (footprint, geometry) that defines the Location.
        /// A Location may have both a point-radius representation
        /// (see decimalLatitude) and a footprint representation,
        /// and they may differ from each other.
        /// This property is currently not used.
        /// </summary>
        public string FootprintWKT { get; set; }

        /// <summary>
        /// Darwin Core term name: geodeticDatum.
        /// The ellipsoid, geodetic datum, or spatial reference
        /// system (SRS) upon which the geographic coordinates
        /// given in decimalLatitude and decimalLongitude as based.
        /// Recommended best practice is use the EPSG code as a
        /// controlled vocabulary to provide an SRS, if known.
        /// Otherwise use a controlled vocabulary for the name or
        /// code of the geodetic datum, if known. Otherwise use a
        /// controlled vocabulary for the name or code of the
        /// ellipsoid, if known. If none of these is known, use the
        /// value "unknown".
        /// This property is currently not used.
        /// </summary>
        public string GeodeticDatum { get; set; }

        /// <summary>
        /// Darwin Core term name: georeferencedBy.
        /// A list (concatenated and separated) of names of people,
        /// groups, or organizations who determined the georeference
        /// (spatial representation) the Location.
        /// This property is currently not used.
        /// </summary>
        public string GeoreferencedBy { get; set; }

        /// <summary>
        /// Darwin Core term name: georeferencedDate.
        /// The date on which the Location was georeferenced.
        /// Recommended best practice is to use an encoding scheme,
        /// such as ISO 8601:2004(E).
        /// This property is currently not used.
        /// </summary>
        public string GeoreferencedDate { get; set; }

        /// <summary>
        /// Darwin Core term name: georeferenceProtocol.
        /// A description or reference to the methods used to
        /// determine the spatial footprint, coordinates, and
        /// uncertainties.
        /// This property is currently not used.
        /// </summary>
        public string GeoreferenceProtocol { get; set; }

        /// <summary>
        /// Darwin Core term name: georeferenceRemarks.
        /// Notes or comments about the spatial description
        /// determination, explaining assumptions made in addition
        /// or opposition to the those formalized in the method
        /// referred to in georeferenceProtocol.
        /// This property is currently not used.
        /// </summary>
        public string GeoreferenceRemarks { get; set; }

        /// <summary>
        /// Darwin Core term name: georeferenceSources.
        /// A list (concatenated and separated) of maps, gazetteers,
        /// or other resources used to georeference the Location,
        /// described specifically enough to allow anyone in the
        /// future to use the same resources.
        /// This property is currently not used.
        /// </summary>
        public string GeoreferenceSources { get; set; }

        /// <summary>
        /// Darwin Core term name: georeferenceVerificationStatus.
        /// A categorical description of the extent to which the
        /// georeference has been verified to represent the best
        /// possible spatial description. Recommended best practice
        /// is to use a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string GeoreferenceVerificationStatus { get; set; }

        /// <summary>
        /// Darwin Core term name: higherGeography.
        /// A list (concatenated and separated) of geographic
        /// names less specific than the information captured
        /// in the locality term.
        /// This property is currently not used.
        /// </summary>
        public string HigherGeography { get; set; }

        /// <summary>
        /// Darwin Core term name: higherGeographyID.
        /// An identifier for the geographic region within which
        /// the Location occurred.
        /// Recommended best practice is to use an
        /// persistent identifier from a controlled vocabulary
        /// such as the Getty Thesaurus of Geographic Names.
        /// This property is currently not used.
        /// </summary>
        public string HigherGeographyID { get; set; }

        /// <summary>
        /// Darwin Core term name: island.
        /// The name of the island on or near which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographic Names.
        /// This property is currently not used.
        /// </summary>
        public string Island { get; set; }

        /// <summary>
        /// Darwin Core term name: islandGroup.
        /// The name of the island group in which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographic Names.
        /// This property is currently not used.
        /// </summary>
        public string IslandGroup { get; set; }

        /// <summary>
        /// Darwin Core term name: locality.
        /// The specific description of the place. Less specific
        /// geographic information can be provided in other
        /// geographic terms (higherGeography, continent, country,
        /// stateProvince, county, municipality, waterBody, island,
        /// islandGroup). This term may contain information modified
        /// from the original to correct perceived errors or
        /// standardize the description.
        /// </summary>
        public string Locality { get; set; }

        /// <summary>
        /// Darwin Core term name: locationAccordingTo.
        /// Information about the source of this Location information.
        /// Could be a publication (gazetteer), institution,
        /// or team of individuals.
        /// This property is currently not used.
        /// </summary>
        public string LocationAccordingTo { get; set; }

        /// <summary>
        /// Darwin Core term name: locationID.
        /// An identifier for the set of location information
        /// (data associated with dcterms:Location).
        /// May be a global unique identifier or an identifier
        /// specific to the data set.
        /// This property is currently not used.
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// Darwin Core term name: locationRemarks.
        /// Comments or notes about the Location.
        /// This property is currently not used.
        /// </summary>
        public string LocationRemarks { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Web address that leads to more information about the
        /// location. The information should be accessible
        /// from the most commonly used web browsers.
        /// </summary>
        //public string LocationURL { get; set; }

        /// <summary>
        /// Darwin Core term name: maximumDepthInMeters.
        /// The greater depth of a range of depth below
        /// the local surface, in meters.
        /// This property is currently not used.
        /// </summary>
        public string MaximumDepthInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: maximumDistanceAboveSurfaceInMeters.
        /// The greater distance in a range of distance from a
        /// reference surface in the vertical direction, in meters.
        /// Use positive values for locations above the surface,
        /// negative values for locations below. If depth measures
        /// are given, the reference surface is the location given
        /// by the depth, otherwise the reference surface is the
        /// location given by the elevation.
        /// This property is currently not used.
        /// </summary>
        public string MaximumDistanceAboveSurfaceInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: maximumElevationInMeters.
        /// The upper limit of the range of elevation (altitude,
        /// usually above sea level), in meters.
        /// This property is currently not used.
        /// </summary>
        public string MaximumElevationInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: minimumDepthInMeters.
        /// The lesser depth of a range of depth below the
        /// local surface, in meters.
        /// This property is currently not used.
        /// </summary>
        public string MinimumDepthInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: minimumDistanceAboveSurfaceInMeters.
        /// The lesser distance in a range of distance from a
        /// reference surface in the vertical direction, in meters.
        /// Use positive values for locations above the surface,
        /// negative values for locations below.
        /// If depth measures are given, the reference surface is
        /// the location given by the depth, otherwise the reference
        /// surface is the location given by the elevation.
        /// This property is currently not used.
        /// </summary>
        public string MinimumDistanceAboveSurfaceInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: minimumElevationInMeters.
        /// The lower limit of the range of elevation (altitude,
        /// usually above sea level), in meters.
        /// This property is currently not used.
        /// </summary>
        public string MinimumElevationInMeters { get; set; }

        /// <summary>
        /// Darwin Core term name: municipality.
        /// The full, unabbreviated name of the next smaller
        /// administrative region than county (city, municipality, etc.)
        /// in which the Location occurs.
        /// Do not use this term for a nearby named place
        /// that does not contain the actual location.
        /// </summary>
        public string Municipality { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Parish where the species observation where made.
        /// 'Socken/församling' in swedish.
        /// </summary>
        //public string Parish { get; set; }

        /// <summary>
        /// Darwin Core term name: pointRadiusSpatialFit.
        /// The ratio of the area of the point-radius
        /// (decimalLatitude, decimalLongitude,
        /// coordinateUncertaintyInMeters) to the area of the true
        /// (original, or most specific) spatial representation of
        /// the Location. Legal values are 0, greater than or equal
        /// to 1, or undefined. A value of 1 is an exact match or
        /// 100% overlap. A value of 0 should be used if the given
        /// point-radius does not completely contain the original
        /// representation. The pointRadiusSpatialFit is undefined
        /// (and should be left blank) if the original representation
        /// is a point without uncertainty and the given georeference
        /// is not that same point (without uncertainty). If both the
        /// original and the given georeference are the same point,
        /// the pointRadiusSpatialFit is 1.
        /// This property is currently not used.
        /// </summary>
        public string PointRadiusSpatialFit { get; set; }

        /// <summary>
        /// Darwin Core term name: stateProvince.
        /// The name of the next smaller administrative region than
        /// country (state, province, canton, department, region, etc.)
        /// in which the Location occurs.
        /// ('landskap' in swedish).
        /// </summary>
        public string StateProvince { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimCoordinates.
        /// The verbatim original spatial coordinates of the Location.
        /// The coordinate ellipsoid, geodeticDatum, or full
        /// Spatial Reference System (SRS) for these coordinates
        /// should be stored in verbatimSRS and the coordinate
        /// system should be stored in verbatimCoordinateSystem.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimCoordinates { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimCoordinateSystem.
        /// The spatial coordinate system for the verbatimLatitude
        /// and verbatimLongitude or the verbatimCoordinates of the
        /// Location.
        /// Recommended best practice is to use a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimCoordinateSystem { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimDepth.
        /// The original description of the
        /// depth below the local surface.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimDepth { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimElevation.
        /// The original description of the elevation (altitude,
        /// usually above sea level) of the Location.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimElevation { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimLatitude.
        /// The verbatim original latitude of the Location.
        /// The coordinate ellipsoid, geodeticDatum, or full
        /// Spatial Reference System (SRS) for these coordinates
        /// should be stored in verbatimSRS and the coordinate
        /// system should be stored in verbatimCoordinateSystem.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimLatitude { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimLocality.
        /// The original textual description of the place.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimLocality { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimLongitude.
        /// The verbatim original longitude of the Location.
        /// The coordinate ellipsoid, geodeticDatum, or full
        /// Spatial Reference System (SRS) for these coordinates
        /// should be stored in verbatimSRS and the coordinate
        /// system should be stored in verbatimCoordinateSystem.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimLongitude { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimSRS.
        /// The ellipsoid, geodetic datum, or spatial reference
        /// system (SRS) upon which coordinates given in
        /// verbatimLatitude and verbatimLongitude, or
        /// verbatimCoordinates are based.
        /// Recommended best practice is use the EPSG code as
        /// a controlled vocabulary to provide an SRS, if known.
        /// Otherwise use a controlled vocabulary for the name or
        /// code of the geodetic datum, if known.
        /// Otherwise use a controlled vocabulary for the name or
        /// code of the ellipsoid, if known. If none of these is
        /// known, use the value "unknown".
        /// This property is currently not used.
        /// </summary>
        public string VerbatimSRS { get; set; }

        /// <summary>
        /// Darwin Core term name: waterBody.
        /// The name of the water body in which the Location occurs.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Getty Thesaurus of Geographic Names.
        /// This property is currently not used.
        /// </summary>
        public string WaterBody { get; set; }
        #endregion

        #region Occurrence
        /// <summary>
        /// Darwin Core term name: associatedMedia.
        /// A list (concatenated and separated) of identifiers
        /// (publication, global unique identifier, URI) of
        /// media associated with the Occurrence.
        /// This property is currently not used.
        /// </summary>
        public string AssociatedMedia { get; set; }

        /// <summary>
        /// Darwin Core term name: associatedOccurrences.
        /// A list (concatenated and separated) of identifiers of
        /// other Occurrence records and their associations to
        /// this Occurrence.
        /// This property is currently not used.
        /// </summary>
        public string AssociatedOccurrences { get; set; }

        /// <summary>
        /// Darwin Core term name: associatedReferences.
        /// A list (concatenated and separated) of identifiers
        /// (publication, bibliographic reference, global unique
        /// identifier, URI) of literature associated with
        /// the Occurrence.
        /// This property is currently not used.
        /// </summary>
        public string AssociatedReferences { get; set; }

        /// <summary>
        /// Darwin Core term name: associatedSequences.
        /// A list (concatenated and separated) of identifiers of
        /// other Occurrence records and their associations to
        /// this Occurrence.
        /// This property is currently not used.
        /// </summary>
        public string AssociatedSequences { get; set; }

        /// <summary>
        /// Darwin Core term name: associatedTaxa.
        /// A list (concatenated and separated) of identifiers or
        /// names of taxa and their associations with the Occurrence.
        /// This property is currently not used.
        /// </summary>
        public string AssociatedTaxa { get; set; }

        /// <summary>
        /// Darwin Core term name: behavior.
        /// A description of the behavior shown by the subject at
        /// the time the Occurrence was recorded.
        /// Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string Behavior { get; set; }

        /// <summary>
        /// Darwin Core term name: catalogNumber.
        /// An identifier (preferably unique) for the record
        /// within the data set or collection.
        /// </summary>
        public string CatalogNumber { get; set; }

        /// <summary>
        /// Darwin Core term name: disposition.
        /// The current state of a specimen with respect to the
        /// collection identified in collectionCode or collectionID.
        /// Recommended best practice is to use a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string Disposition { get; set; }

        /// <summary>
        /// Darwin Core term name: establishmentMeans.
        /// The process by which the biological individual(s)
        /// represented in the Occurrence became established at the
        /// location.
        /// Recommended best practice is to use a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string EstablishmentMeans { get; set; }

        /// <summary>
        /// Darwin Core term name: individualCount.
        /// The number of individuals represented present
        /// at the time of the Occurrence.
        /// </summary>
        public string IndividualCount { get; set; }

        /// <summary>
        /// Darwin Core term name: individualID.
        /// An identifier for an individual or named group of
        /// individual organisms represented in the Occurrence.
        /// Meant to accommodate resampling of the same individual
        /// or group for monitoring purposes. May be a global unique
        /// identifier or an identifier specific to a data set.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string IndividualID { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Indicates if this species occurrence is natural or
        /// if it is a result of human activity.
        /// </summary>
        //public bool IsNaturalOccurrence { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Indicates if this observation is a never found observation.
        /// "Never found observation" is an observation that says
        /// that the specified species was not found in a location
        /// deemed appropriate for the species.
        /// </summary>
        //public bool IsNeverFoundObservation { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Indicates if this observation is a 
        /// not rediscovered observation.
        /// "Not rediscovered observation" is an observation that says
        /// that the specified species was not found in a location
        /// where it has previously been observed.
        /// </summary>
        //public bool IsNotRediscoveredObservation { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Indicates if this observation is a positive observation.
        /// "Positive observation" is a normal observation indicating
        /// that a species has been seen at a specified location.
        /// </summary>
        //public bool IsPositiveObservation { get; set; }

        /// <summary>
        /// Darwin Core term name: lifeStage.
        /// The age class or life stage of the biological individual(s)
        /// at the time the Occurrence was recorded.
        /// Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string LifeStage { get; set; }

        /// <summary>
        /// Darwin Core term name: occurrenceID.
        /// An identifier for the Occurrence (as opposed to a
        /// particular digital record of the occurrence).
        /// In the absence of a persistent global unique identifier,
        /// construct one from a combination of identifiers in
        /// the record that will most closely make the occurrenceID
        /// globally unique.
        /// The format LSID (Life Science Identifiers) is used as GUID
        /// (Globally unique identifier) for species observations.
        /// Currently known GUIDs:
        /// Species Gateway (Artportalen) 1,
        /// urn:lsid:artportalen.se:Sighting:{reporting system}.{id}
        /// where {reporting system} is one of Bird, Bugs, Fish, 
        /// MarineInvertebrates, PlantAndMushroom or Vertebrate.
        /// Species Gateway (Artportalen) 2,
        /// urn:lsid:artportalen.se:Sighting:{id}
        /// Red list database: urn:lsid:artdata.slu.se:SpeciesObservation:{id}
        /// </summary>
        public string OccurrenceID { get; set; }

        /// <summary>
        /// Darwin Core term name: occurrenceRemarks.
        /// Comments or notes about the Occurrence.
        /// </summary>
        public string OccurrenceRemarks { get; set; }

        /// <summary>
        /// Darwin Core term name: occurrenceStatus.
        /// A statement about the presence or absence of a Taxon at a
        /// Location.
        /// Recommended best practice is to use a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string OccurrenceStatus { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Web address that leads to more information about the
        /// occurrence. The information should be accessible
        /// from the most commonly used web browsers.
        /// </summary>
        //public string OccurrenceURL { get; set; }

        /// <summary>
        /// Darwin Core term name: otherCatalogNumbers.
        /// A list (concatenated and separated) of previous or
        /// alternate fully qualified catalog numbers or other
        /// human-used identifiers for the same Occurrence,
        /// whether in the current or any other data set or collection.
        /// This property is currently not used.
        /// </summary>
        public string OtherCatalogNumbers { get; set; }

        /// <summary>
        /// Darwin Core term name: preparations.
        /// A list (concatenated and separated) of preparations
        /// and preservation methods for a specimen.
        /// This property is currently not used.
        /// </summary>
        public string Preparations { get; set; }

        /// <summary>
        /// Darwin Core term name: previousIdentifications.
        /// A list (concatenated and separated) of previous
        /// assignments of names to the Occurrence.
        /// This property is currently not used.
        /// </summary>
        public string PreviousIdentifications { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Quantity of observed species, for example distribution area.
        /// Unit is specified in property QuantityUnit.
        /// </summary>
        //public string Quantity { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Unit for quantity value of observed species.
        /// </summary>
        //public string QuantityUnit { get; set; }

        /// <summary>
        /// Darwin Core term name: recordedBy.
        /// A list (concatenated and separated) of names of people,
        /// groups, or organizations responsible for recording the
        /// original Occurrence. The primary collector or observer,
        /// especially one who applies a personal identifier
        /// (recordNumber), should be listed first.
        /// </summary>
        public string RecordedBy { get; set; }

        /// <summary>
        /// Darwin Core term name: recordNumber.
        /// An identifier given to the Occurrence at the time it was
        /// recorded. Often serves as a link between field notes and
        /// an Occurrence record, such as a specimen collector's number.
        /// This property is currently not used.
        /// </summary>
        public string RecordNumber { get; set; }

        /// <summary>
        /// Darwin Core term name: reproductiveCondition.
        /// The reproductive condition of the biological individual(s)
        /// represented in the Occurrence.
        /// Recommended best practice is to use a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string ReproductiveCondition { get; set; }

        /// <summary>
        /// Darwin Core term name: sex.
        /// The sex of the biological individual(s) represented in
        /// the Occurrence.
        /// Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string Sex { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Substrate on which the species was observed.
        /// </summary>
        //public string Substrate { get; set; }
        #endregion

        #region Project
        /// <summary>
        /// Not defined in Darwin Core.
        /// Indicates if species observations that are reported in
        /// a project are publicly available or not.
        /// </summary>
        //public bool IsPublic { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Information about the type of project,
        /// for example 'Environmental monitoring'.
        /// </summary>
        //public string ProjectCategory { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Description of a project.
        /// </summary>
        //public string ProjectDescription { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Date when the project ends.
        /// </summary>
        //public string ProjectEndDate { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// An identifier for the project.
        /// In the absence of a persistent global unique identifier,
        /// construct one from a combination of identifiers in
        /// the project that will most closely make the ProjectID
        /// globally unique.
        /// The format LSID (Life Science Identifiers) is used as GUID
        /// (Globally unique identifier).
        /// </summary>
        //public string ProjectID { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Name of the project.
        /// </summary>
        //public string ProjectName { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Name of person or organization that owns the project.
        /// </summary>
        //public string ProjectOwner { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Date when the project starts.
        /// </summary>
        //public string ProjectStartDate { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Web address that leads to more information about the
        /// project. The information should be accessible
        /// from the most commonly used web browsers.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        //public string ProjectURL { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Survey method used in a project to
        /// retrieve species observations.
        /// </summary>
        //public string SurveyMethod { get; set; }
        #endregion

        #region Taxon
        /// <summary>
        /// Darwin Core term name: acceptedNameUsage.
        /// The full name, with authorship and date information
        /// if known, of the currently valid (zoological) or
        /// accepted (botanical) taxon.
        /// This property is currently not used.
        /// </summary>
        public string AcceptedNameUsage { get; set; }

        /// <summary>
        /// Darwin Core term name: acceptedNameUsageID.
        /// An identifier for the name usage (documented meaning of
        /// the name according to a source) of the currently valid
        /// (zoological) or accepted (botanical) taxon.
        /// This property is currently not used.
        /// </summary>
        public string AcceptedNameUsageID { get; set; }

        /// <summary>
        /// Darwin Core term name: class.
        /// The full scientific name of the class in which
        /// the taxon is classified.
        /// This property is currently not used.
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// Darwin Core term name: family.
        /// The full scientific name of the family in which
        /// the taxon is classified.
        /// This property is currently not used.
        /// </summary>
        public string Family { get; set; }

        /// <summary>
        /// Darwin Core term name: genus.
        /// The full scientific name of the genus in which
        /// the taxon is classified.
        /// This property is currently not used.
        /// </summary>
        public string Genus { get; set; }

        /// <summary>
        /// Darwin Core term name: higherClassification.
        /// A list (concatenated and separated) of taxa names
        /// terminating at the rank immediately superior to the
        /// taxon referenced in the taxon record.
        /// Recommended best practice is to order the list
        /// starting with the highest rank and separating the names
        /// for each rank with a semi-colon (";").
        /// This property is currently not used.
        /// </summary>
        public string HigherClassification { get; set; }

        /// <summary>
        /// Darwin Core term name: infraspecificEpithet.
        /// The name of the lowest or terminal infraspecific epithet
        /// of the scientificName, excluding any rank designation.
        /// This property is currently not used.
        /// </summary>
        public string InfraspecificEpithet { get; set; }

        /// <summary>
        /// Darwin Core term name: kingdom.
        /// The full scientific name of the kingdom in which the
        /// taxon is classified.
        /// This property is currently not used.
        /// </summary>
        public string Kingdom { get; set; }

        /// <summary>
        /// Darwin Core term name: nameAccordingTo.
        /// The reference to the source in which the specific
        /// taxon concept circumscription is defined or implied -
        /// traditionally signified by the Latin "sensu" or "sec."
        /// (from secundum, meaning "according to").
        /// For taxa that result from identifications, a reference
        /// to the keys, monographs, experts and other sources should
        /// be given.
        /// This property is currently not used.
        /// </summary>
        public string NameAccordingTo { get; set; }

        /// <summary>
        /// Darwin Core term name: nameAccordingToID.
        /// An identifier for the source in which the specific
        /// taxon concept circumscription is defined or implied.
        /// See nameAccordingTo.
        /// This property is currently not used.
        /// </summary>
        public string NameAccordingToID { get; set; }

        /// <summary>
        /// Darwin Core term name: namePublishedIn.
        /// A reference for the publication in which the
        /// scientificName was originally established under the rules
        /// of the associated nomenclaturalCode.
        /// This property is currently not used.
        /// </summary>
        public string NamePublishedIn { get; set; }

        /// <summary>
        /// Darwin Core term name: namePublishedInID.
        /// An identifier for the publication in which the
        /// scientificName was originally established under the
        /// rules of the associated nomenclaturalCode.
        /// This property is currently not used.
        /// </summary>
        public string NamePublishedInID { get; set; }

        /// <summary>
        /// Darwin Core term name: namePublishedInYear.
        /// The four-digit year in which the scientificName
        /// was published.
        /// This property is currently not used.
        /// </summary>
        public string NamePublishedInYear { get; set; }

        /// <summary>
        /// Darwin Core term name: nomenclaturalCode.
        /// The nomenclatural code (or codes in the case of an
        /// ambiregnal name) under which the scientificName is
        /// constructed.
        /// Recommended best practice is to use a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string NomenclaturalCode { get; set; }

        /// <summary>
        /// Darwin Core term name: nomenclaturalStatus.
        /// The status related to the original publication of the
        /// name and its conformance to the relevant rules of
        /// nomenclature. It is based essentially on an algorithm
        /// according to the business rules of the code.
        /// It requires no taxonomic opinion.
        /// This property is currently not used.
        /// </summary>
        public string NomenclaturalStatus { get; set; }

        /// <summary>
        /// Darwin Core term name: order.
        /// The full scientific name of the order in which
        /// the taxon is classified.
        /// This property is currently not used.
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Common name of the organism group that observed species
        /// belongs to. Classification of species groups is the same as
        /// used in latest 'Red List of Swedish Species'.
        /// </summary>
        //public string OrganismGroup { get; set; }

        /// <summary>
        /// Darwin Core term name: originalNameUsage.
        /// The taxon name, with authorship and date information
        /// if known, as it originally appeared when first established
        /// under the rules of the associated nomenclaturalCode.
        /// The basionym (botany) or basonym (bacteriology) of the
        /// scientificName or the senior/earlier homonym for replaced
        /// names.
        /// This property is currently not used.
        /// </summary>
        public string OriginalNameUsage { get; set; }

        /// <summary>
        /// Darwin Core term name: originalNameUsageID.
        /// An identifier for the name usage (documented meaning of
        /// the name according to a source) in which the terminal
        /// element of the scientificName was originally established
        /// under the rules of the associated nomenclaturalCode.
        /// This property is currently not used.
        /// </summary>
        public string OriginalNameUsageID { get; set; }

        /// <summary>
        /// Darwin Core term name: parentNameUsage.
        /// The full name, with authorship and date information
        /// if known, of the direct, most proximate higher-rank
        /// parent taxon (in a classification) of the most specific
        /// element of the scientificName.
        /// This property is currently not used.
        /// </summary>
        public string ParentNameUsage { get; set; }

        /// <summary>
        /// Darwin Core term name: parentNameUsageID.
        /// An identifier for the name usage (documented meaning
        /// of the name according to a source) of the direct,
        /// most proximate higher-rank parent taxon
        /// (in a classification) of the most specific
        /// element of the scientificName.
        /// This property is currently not used.
        /// </summary>
        public string ParentNameUsageID { get; set; }

        /// <summary>
        /// Darwin Core term name: phylum.
        /// The full scientific name of the phylum or division
        /// in which the taxon is classified.
        /// This property is currently not used.
        /// </summary>
        public string Phylum { get; set; }

        /// <summary>
        /// Darwin Core term name: scientificName.
        /// The full scientific name, with authorship and date
        /// information if known. When forming part of an
        /// Identification, this should be the name in lowest level
        /// taxonomic rank that can be determined.
        /// This term should not contain identification qualifications,
        /// which should instead be supplied in the
        /// IdentificationQualifier term.
        /// Currently scientific name without author is provided.
        /// </summary>
        public string ScientificName { get; set; }

        /// <summary>
        /// Darwin Core term name: scientificNameAuthorship.
        /// The authorship information for the scientificName
        /// formatted according to the conventions of the applicable
        /// nomenclaturalCode.
        /// This property is currently not used.
        /// </summary>
        public string ScientificNameAuthorship { get; set; }

        /// <summary>
        /// Darwin Core term name: scientificNameID.
        /// An identifier for the nomenclatural (not taxonomic)
        /// details of a scientific name.
        /// This property is currently not used.
        /// </summary>
        public string ScientificNameID { get; set; }

        /// <summary>
        /// Darwin Core term name: specificEpithet.
        /// The name of the first or species epithet of
        /// the scientificName.
        /// This property is currently not used.
        /// </summary>
        public string SpecificEpithet { get; set; }

        /// <summary>
        /// Darwin Core term name: subgenus.
        /// The full scientific name of the subgenus in which
        /// the taxon is classified. Values should include the
        /// genus to avoid homonym confusion.
        /// This property is currently not used.
        /// </summary>
        public string Subgenus { get; set; }

        /// <summary>
        /// Darwin Core term name: taxonConceptID.
        /// An identifier for the taxonomic concept to which the record
        /// refers - not for the nomenclatural details of a taxon.
        /// In SwedishSpeciesObservationSOAPService this property
        /// has the same value as property TaxonID. 
        /// GUID in Dyntaxa is used as value for this property.
        /// This property is currently not used.
        /// </summary>
        public string TaxonConceptID { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Status of the taxon concept.
        /// Examples of possible values are InvalidDueToSplit,
        /// InvalidDueToLump, InvalidDueToDelete, Unchanged,
        /// ValidAfterLump or ValidAfterSplit.
        /// </summary>
        //public string TaxonConceptStatus { get; set; }

        /// <summary>
        /// Darwin Core term name: taxonID.
        /// An identifier for the set of taxon information
        /// (data associated with the Taxon class). May be a global
        /// unique identifier or an identifier specific to the data set.
        /// In SwedishSpeciesObservationSOAPService this property
        /// has the same value as property TaxonConceptID. 
        /// GUID in Dyntaxa is used as value for this property.
        /// </summary>
        public string TaxonID { get; set; }

        /// <summary>
        /// Darwin Core term name: taxonomicStatus.
        /// The status of the use of the scientificName as a label
        /// for a taxon. Requires taxonomic opinion to define the
        /// scope of a taxon. Rules of priority then are used to
        /// define the taxonomic status of the nomenclature contained
        /// in that scope, combined with the experts opinion.
        /// It must be linked to a specific taxonomic reference that
        /// defines the concept.
        /// Recommended best practice is to use a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string TaxonomicStatus { get; set; }

        /// <summary>
        /// Darwin Core term name: taxonRank.
        /// The taxonomic rank of the most specific name in the
        /// scientificName. Recommended best practice is to use
        /// a controlled vocabulary.
        /// This property is currently not used.
        /// </summary>
        public string TaxonRank { get; set; }

        /// <summary>
        /// Darwin Core term name: taxonRemarks.
        /// Comments or notes about the taxon or name.
        /// This property is currently not used.
        /// </summary>
        public string TaxonRemarks { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Sort order of taxon according to Dyntaxa.
        /// This property is currently not used.
        /// </summary>
        //public int TaxonSortOrder { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Web address that leads to more information about the
        /// taxon. The information should be accessible
        /// from the most commonly used web browsers.
        /// </summary>
        //public string TaxonURL { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// ScientificName as it appears in the original record.
        /// This property is currently not used.
        /// </summary>
        //public string VerbatimScientificName { get; set; }

        /// <summary>
        /// Darwin Core term name: verbatimTaxonRank.
        /// The taxonomic rank of the most specific name in the
        /// scientificName as it appears in the original record.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimTaxonRank { get; set; }

        /// <summary>
        /// Darwin Core term name: vernacularName.
        /// A common or vernacular name.
        /// </summary>
        public string VernacularName { get; set; }
        #endregion
    }

}