using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using OfficeOpenXml;
using SOS.Import.DarwinCore;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Repositories.Destination.DarwinCoreArchive;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Models.Verbatim.DarwinCore;
using Xunit;
using Xunit.Abstractions;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateDwcDataStatisticsTool : TestBase
    {
        public CreateDwcDataStatisticsTool(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private readonly ITestOutputHelper _testOutputHelper;

        private DarwinCoreArchiveVerbatimRepository CreateArchiveVerbatimRepository()
        {
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var importClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);
            var repository =
                new DarwinCoreArchiveVerbatimRepository(importClient,
                    new NullLogger<DarwinCoreArchiveVerbatimRepository>());

            return repository;
        }

        private DwcObservationHarvester CreateDwcObservationHarvester()
        {
            var verbatimDbConfiguration = GetVerbatimDbConfiguration();
            var importClient = new VerbatimClient(
                verbatimDbConfiguration.GetMongoDbSettings(),
                verbatimDbConfiguration.DatabaseName,
                verbatimDbConfiguration.ReadBatchSize,
                verbatimDbConfiguration.WriteBatchSize);
            var dwcObservationHarvester = new DwcObservationHarvester(
                new DarwinCoreArchiveVerbatimRepository(importClient,
                    new NullLogger<DarwinCoreArchiveVerbatimRepository>()),
                new DarwinCoreArchiveEventRepository(importClient, new NullLogger<DarwinCoreArchiveEventRepository>()),
                new DwcArchiveReader(new NullLogger<DwcArchiveReader>()),
                new NullLogger<DwcObservationHarvester>());
            return dwcObservationHarvester;
        }


        private Dictionary<string, Expression<Func<DwcObservationVerbatim, DistinctValueObject<string>>>>
            DwcPropertyExpressionDictionary
        {
            get
            {
                return new Dictionary<string, Expression<Func<DwcObservationVerbatim, DistinctValueObject<string>>>>
                {
                    {
                        "DataProviderIdentifier",
                        observation => new DistinctValueObject<string>(observation.DataProviderIdentifier)
                    },
                    {"AccessRights", observation => new DistinctValueObject<string>(observation.AccessRights)},
                    {"BasisOfRecord", observation => new DistinctValueObject<string>(observation.BasisOfRecord)},
                    {
                        "BibliographicCitation",
                        observation => new DistinctValueObject<string>(observation.BibliographicCitation)
                    },
                    {"CollectionCode", observation => new DistinctValueObject<string>(observation.CollectionCode)},
                    {"CollectionID", observation => new DistinctValueObject<string>(observation.CollectionID)},
                    {
                        "DataGeneralizations",
                        observation => new DistinctValueObject<string>(observation.DataGeneralizations)
                    },
                    {"DatasetID", observation => new DistinctValueObject<string>(observation.DatasetID)},
                    {"DatasetName", observation => new DistinctValueObject<string>(observation.DatasetName)},
                    {
                        "DynamicProperties",
                        observation => new DistinctValueObject<string>(observation.DynamicProperties)
                    },
                    {
                        "InformationWithheld",
                        observation => new DistinctValueObject<string>(observation.InformationWithheld)
                    },
                    {"InstitutionCode", observation => new DistinctValueObject<string>(observation.InstitutionCode)},
                    {"InstitutionID", observation => new DistinctValueObject<string>(observation.InstitutionID)},
                    {"Language", observation => new DistinctValueObject<string>(observation.Language)},
                    {"License", observation => new DistinctValueObject<string>(observation.License)},
                    {"Modified", observation => new DistinctValueObject<string>(observation.Modified)},
                    {
                        "OwnerInstitutionCode",
                        observation => new DistinctValueObject<string>(observation.OwnerInstitutionCode)
                    },
                    {"References", observation => new DistinctValueObject<string>(observation.References)},
                    {"RightsHolder", observation => new DistinctValueObject<string>(observation.RightsHolder)},
                    {"Type", observation => new DistinctValueObject<string>(observation.Type)},
                    {"Day", observation => new DistinctValueObject<string>(observation.Day)},
                    {"EndDayOfYear", observation => new DistinctValueObject<string>(observation.EndDayOfYear)},
                    {"EventDate", observation => new DistinctValueObject<string>(observation.EventDate)},
                    {"EventID", observation => new DistinctValueObject<string>(observation.EventID)},
                    {"ParentEventID", observation => new DistinctValueObject<string>(observation.ParentEventID)},
                    {"EventRemarks", observation => new DistinctValueObject<string>(observation.EventRemarks)},
                    {"EventTime", observation => new DistinctValueObject<string>(observation.EventTime)},
                    {"FieldNotes", observation => new DistinctValueObject<string>(observation.FieldNotes)},
                    {"FieldNumber", observation => new DistinctValueObject<string>(observation.FieldNumber)},
                    {"Habitat", observation => new DistinctValueObject<string>(observation.Habitat)},
                    {"Month", observation => new DistinctValueObject<string>(observation.Month)},
                    {"SampleSizeUnit", observation => new DistinctValueObject<string>(observation.SampleSizeUnit)},
                    {"SampleSizeValue", observation => new DistinctValueObject<string>(observation.SampleSizeValue)},
                    {"SamplingEffort", observation => new DistinctValueObject<string>(observation.SamplingEffort)},
                    {"SamplingProtocol", observation => new DistinctValueObject<string>(observation.SamplingProtocol)},
                    {"StartDayOfYear", observation => new DistinctValueObject<string>(observation.StartDayOfYear)},
                    {
                        "VerbatimEventDate",
                        observation => new DistinctValueObject<string>(observation.VerbatimEventDate)
                    },
                    {"Year", observation => new DistinctValueObject<string>(observation.Year)},
                    {"Bed", observation => new DistinctValueObject<string>(observation.Bed)},
                    {
                        "EarliestAgeOrLowestStage",
                        observation => new DistinctValueObject<string>(observation.EarliestAgeOrLowestStage)
                    },
                    {
                        "EarliestEonOrLowestEonothem",
                        observation => new DistinctValueObject<string>(observation.EarliestEonOrLowestEonothem)
                    },
                    {
                        "EarliestEpochOrLowestSeries",
                        observation => new DistinctValueObject<string>(observation.EarliestEpochOrLowestSeries)
                    },
                    {
                        "EarliestEraOrLowestErathem",
                        observation => new DistinctValueObject<string>(observation.EarliestEraOrLowestErathem)
                    },
                    {
                        "EarliestGeochronologicalEra",
                        observation => new DistinctValueObject<string>(observation.EarliestGeochronologicalEra)
                    },
                    {
                        "EarliestPeriodOrLowestSystem",
                        observation => new DistinctValueObject<string>(observation.EarliestPeriodOrLowestSystem)
                    },
                    {"Formation", observation => new DistinctValueObject<string>(observation.Formation)},
                    {
                        "GeologicalContextID",
                        observation => new DistinctValueObject<string>(observation.GeologicalContextID)
                    },
                    {"Group", observation => new DistinctValueObject<string>(observation.Group)},
                    {
                        "HighestBiostratigraphicZone",
                        observation => new DistinctValueObject<string>(observation.HighestBiostratigraphicZone)
                    },
                    {
                        "LatestAgeOrHighestStage",
                        observation => new DistinctValueObject<string>(observation.LatestAgeOrHighestStage)
                    },
                    {
                        "LatestEonOrHighestEonothem",
                        observation => new DistinctValueObject<string>(observation.LatestEonOrHighestEonothem)
                    },
                    {
                        "LatestEpochOrHighestSeries",
                        observation => new DistinctValueObject<string>(observation.LatestEpochOrHighestSeries)
                    },
                    {
                        "LatestEraOrHighestErathem",
                        observation => new DistinctValueObject<string>(observation.LatestEraOrHighestErathem)
                    },
                    {
                        "LatestGeochronologicalEra",
                        observation => new DistinctValueObject<string>(observation.LatestGeochronologicalEra)
                    },
                    {
                        "LatestPeriodOrHighestSystem",
                        observation => new DistinctValueObject<string>(observation.LatestPeriodOrHighestSystem)
                    },
                    {
                        "LithostratigraphicTerms",
                        observation => new DistinctValueObject<string>(observation.LithostratigraphicTerms)
                    },
                    {
                        "LowestBiostratigraphicZone",
                        observation => new DistinctValueObject<string>(observation.LowestBiostratigraphicZone)
                    },
                    {"Member", observation => new DistinctValueObject<string>(observation.Member)},
                    {"DateIdentified", observation => new DistinctValueObject<string>(observation.DateIdentified)},
                    {"IdentificationID", observation => new DistinctValueObject<string>(observation.IdentificationID)},
                    {
                        "IdentificationQualifier",
                        observation => new DistinctValueObject<string>(observation.IdentificationQualifier)
                    },
                    {
                        "IdentificationReferences",
                        observation => new DistinctValueObject<string>(observation.IdentificationReferences)
                    },
                    {
                        "IdentificationRemarks",
                        observation => new DistinctValueObject<string>(observation.IdentificationRemarks)
                    },
                    {
                        "IdentificationVerificationStatus",
                        observation => new DistinctValueObject<string>(observation.IdentificationVerificationStatus)
                    },
                    {"IdentifiedBy", observation => new DistinctValueObject<string>(observation.IdentifiedBy)},
                    {"TypeStatus", observation => new DistinctValueObject<string>(observation.TypeStatus)},
                    {"Continent", observation => new DistinctValueObject<string>(observation.Continent)},
                    {
                        "CoordinatePrecision",
                        observation => new DistinctValueObject<string>(observation.CoordinatePrecision)
                    },
                    {
                        "CoordinateUncertaintyInMeters",
                        observation => new DistinctValueObject<string>(observation.CoordinateUncertaintyInMeters)
                    },
                    {"Country", observation => new DistinctValueObject<string>(observation.Country)},
                    {"CountryCode", observation => new DistinctValueObject<string>(observation.CountryCode)},
                    {"County", observation => new DistinctValueObject<string>(observation.County)},
                    //{"DecimalLatitude",observation => new ValueObject<string>(observation.DecimalLatitude)},
                    //{"DecimalLongitude",observation => new ValueObject<string>(observation.DecimalLongitude)},
                    {
                        "FootprintSpatialFit",
                        observation => new DistinctValueObject<string>(observation.FootprintSpatialFit)
                    },
                    {"FootprintSRS", observation => new DistinctValueObject<string>(observation.FootprintSRS)},
                    {"FootprintWKT", observation => new DistinctValueObject<string>(observation.FootprintWKT)},
                    {"GeodeticDatum", observation => new DistinctValueObject<string>(observation.GeodeticDatum)},
                    {"GeoreferencedBy", observation => new DistinctValueObject<string>(observation.GeoreferencedBy)},
                    {
                        "GeoreferencedDate",
                        observation => new DistinctValueObject<string>(observation.GeoreferencedDate)
                    },
                    {
                        "GeoreferenceProtocol",
                        observation => new DistinctValueObject<string>(observation.GeoreferenceProtocol)
                    },
                    {
                        "GeoreferenceRemarks",
                        observation => new DistinctValueObject<string>(observation.GeoreferenceRemarks)
                    },
                    {
                        "GeoreferenceSources",
                        observation => new DistinctValueObject<string>(observation.GeoreferenceSources)
                    },
                    {
                        "GeoreferenceVerificationStatus",
                        observation => new DistinctValueObject<string>(observation.GeoreferenceVerificationStatus)
                    },
                    {"HigherGeography", observation => new DistinctValueObject<string>(observation.HigherGeography)},
                    {
                        "HigherGeographyID",
                        observation => new DistinctValueObject<string>(observation.HigherGeographyID)
                    },
                    {"Island", observation => new DistinctValueObject<string>(observation.Island)},
                    {"IslandGroup", observation => new DistinctValueObject<string>(observation.IslandGroup)},
                    //{"Locality",observation => new ValueObject<string>(observation.Locality)},
                    {
                        "LocationAccordingTo",
                        observation => new DistinctValueObject<string>(observation.LocationAccordingTo)
                    },
                    {"LocationID", observation => new DistinctValueObject<string>(observation.LocationID)},
                    {"LocationRemarks", observation => new DistinctValueObject<string>(observation.LocationRemarks)},
                    {
                        "MaximumDepthInMeters",
                        observation => new DistinctValueObject<string>(observation.MaximumDepthInMeters)
                    },
                    {
                        "MaximumDistanceAboveSurfaceInMeters",
                        observation => new DistinctValueObject<string>(observation.MaximumDistanceAboveSurfaceInMeters)
                    },
                    {
                        "MaximumElevationInMeters",
                        observation => new DistinctValueObject<string>(observation.MaximumElevationInMeters)
                    },
                    {
                        "MinimumDepthInMeters",
                        observation => new DistinctValueObject<string>(observation.MinimumDepthInMeters)
                    },
                    {
                        "MinimumDistanceAboveSurfaceInMeters",
                        observation => new DistinctValueObject<string>(observation.MinimumDistanceAboveSurfaceInMeters)
                    },
                    {
                        "MinimumElevationInMeters",
                        observation => new DistinctValueObject<string>(observation.MinimumElevationInMeters)
                    },
                    {"Municipality", observation => new DistinctValueObject<string>(observation.Municipality)},
                    {
                        "PointRadiusSpatialFit",
                        observation => new DistinctValueObject<string>(observation.PointRadiusSpatialFit)
                    },
                    {"StateProvince", observation => new DistinctValueObject<string>(observation.StateProvince)},
                    {
                        "VerbatimCoordinates",
                        observation => new DistinctValueObject<string>(observation.VerbatimCoordinates)
                    },
                    {
                        "VerbatimCoordinateSystem",
                        observation => new DistinctValueObject<string>(observation.VerbatimCoordinateSystem)
                    },
                    {"VerbatimDepth", observation => new DistinctValueObject<string>(observation.VerbatimDepth)},
                    {
                        "VerbatimElevation",
                        observation => new DistinctValueObject<string>(observation.VerbatimElevation)
                    },
                    {"VerbatimLatitude", observation => new DistinctValueObject<string>(observation.VerbatimLatitude)},
                    {"VerbatimLocality", observation => new DistinctValueObject<string>(observation.VerbatimLocality)},
                    {
                        "VerbatimLongitude",
                        observation => new DistinctValueObject<string>(observation.VerbatimLongitude)
                    },
                    {"VerbatimSRS", observation => new DistinctValueObject<string>(observation.VerbatimSRS)},
                    {"WaterBody", observation => new DistinctValueObject<string>(observation.WaterBody)},
                    {"MaterialSampleID", observation => new DistinctValueObject<string>(observation.MaterialSampleID)},
                    {"AssociatedMedia", observation => new DistinctValueObject<string>(observation.AssociatedMedia)},
                    {
                        "AssociatedReferences",
                        observation => new DistinctValueObject<string>(observation.AssociatedReferences)
                    },
                    {
                        "AssociatedSequences",
                        observation => new DistinctValueObject<string>(observation.AssociatedSequences)
                    },
                    {"AssociatedTaxa", observation => new DistinctValueObject<string>(observation.AssociatedTaxa)},
                    {"Behavior", observation => new DistinctValueObject<string>(observation.Behavior)},
                    {"CatalogNumber", observation => new DistinctValueObject<string>(observation.CatalogNumber)},
                    {"Disposition", observation => new DistinctValueObject<string>(observation.Disposition)},
                    {
                        "EstablishmentMeans",
                        observation => new DistinctValueObject<string>(observation.EstablishmentMeans)
                    },
                    {"IndividualCount", observation => new DistinctValueObject<string>(observation.IndividualCount)},
                    {"LifeStage", observation => new DistinctValueObject<string>(observation.LifeStage)},
                    {"OccurrenceID", observation => new DistinctValueObject<string>(observation.OccurrenceID)},
                    {
                        "OccurrenceRemarks",
                        observation => new DistinctValueObject<string>(observation.OccurrenceRemarks)
                    },
                    {"OccurrenceStatus", observation => new DistinctValueObject<string>(observation.OccurrenceStatus)},
                    {"OrganismQuantity", observation => new DistinctValueObject<string>(observation.OrganismQuantity)},
                    {
                        "OrganismQuantityType",
                        observation => new DistinctValueObject<string>(observation.OrganismQuantityType)
                    },
                    {
                        "OtherCatalogNumbers",
                        observation => new DistinctValueObject<string>(observation.OtherCatalogNumbers)
                    },
                    {"Preparations", observation => new DistinctValueObject<string>(observation.Preparations)},
                    {"RecordedBy", observation => new DistinctValueObject<string>(observation.RecordedBy)},
                    {"RecordNumber", observation => new DistinctValueObject<string>(observation.RecordNumber)},
                    {
                        "ReproductiveCondition",
                        observation => new DistinctValueObject<string>(observation.ReproductiveCondition)
                    },
                    {"Sex", observation => new DistinctValueObject<string>(observation.Sex)},
                    {"OrganismID", observation => new DistinctValueObject<string>(observation.OrganismID)},
                    {"OrganismName", observation => new DistinctValueObject<string>(observation.OrganismName)},
                    {"OrganismScope", observation => new DistinctValueObject<string>(observation.OrganismScope)},
                    {
                        "AssociatedOccurrences",
                        observation => new DistinctValueObject<string>(observation.AssociatedOccurrences)
                    },
                    {
                        "AssociatedOrganisms",
                        observation => new DistinctValueObject<string>(observation.AssociatedOrganisms)
                    },
                    {
                        "PreviousIdentifications",
                        observation => new DistinctValueObject<string>(observation.PreviousIdentifications)
                    },
                    {"OrganismRemarks", observation => new DistinctValueObject<string>(observation.OrganismRemarks)},
                    {
                        "AcceptedNameUsage",
                        observation => new DistinctValueObject<string>(observation.AcceptedNameUsage)
                    },
                    {
                        "AcceptedNameUsageID",
                        observation => new DistinctValueObject<string>(observation.AcceptedNameUsageID)
                    },
                    {"Class", observation => new DistinctValueObject<string>(observation.Class)},
                    {"Family", observation => new DistinctValueObject<string>(observation.Family)},
                    {"Genus", observation => new DistinctValueObject<string>(observation.Genus)},
                    {
                        "HigherClassification",
                        observation => new DistinctValueObject<string>(observation.HigherClassification)
                    },
                    {
                        "InfraspecificEpithet",
                        observation => new DistinctValueObject<string>(observation.InfraspecificEpithet)
                    },
                    {"Kingdom", observation => new DistinctValueObject<string>(observation.Kingdom)},
                    {"NameAccordingTo", observation => new DistinctValueObject<string>(observation.NameAccordingTo)},
                    {
                        "NameAccordingToID",
                        observation => new DistinctValueObject<string>(observation.NameAccordingToID)
                    },
                    {"NamePublishedIn", observation => new DistinctValueObject<string>(observation.NamePublishedIn)},
                    {
                        "NamePublishedInID",
                        observation => new DistinctValueObject<string>(observation.NamePublishedInID)
                    },
                    {
                        "NamePublishedInYear",
                        observation => new DistinctValueObject<string>(observation.NamePublishedInYear)
                    },
                    {
                        "NomenclaturalCode",
                        observation => new DistinctValueObject<string>(observation.NomenclaturalCode)
                    },
                    {
                        "NomenclaturalStatus",
                        observation => new DistinctValueObject<string>(observation.NomenclaturalStatus)
                    },
                    {"Order", observation => new DistinctValueObject<string>(observation.Order)},
                    {
                        "OriginalNameUsage",
                        observation => new DistinctValueObject<string>(observation.OriginalNameUsage)
                    },
                    {
                        "OriginalNameUsageID",
                        observation => new DistinctValueObject<string>(observation.OriginalNameUsageID)
                    },
                    {"ParentNameUsage", observation => new DistinctValueObject<string>(observation.ParentNameUsage)},
                    {
                        "ParentNameUsageID",
                        observation => new DistinctValueObject<string>(observation.ParentNameUsageID)
                    },
                    {"Phylum", observation => new DistinctValueObject<string>(observation.Phylum)},
                    {"ScientificName", observation => new DistinctValueObject<string>(observation.ScientificName)},
                    {
                        "ScientificNameAuthorship",
                        observation => new DistinctValueObject<string>(observation.ScientificNameAuthorship)
                    },
                    {"ScientificNameID", observation => new DistinctValueObject<string>(observation.ScientificNameID)},
                    {"SpecificEpithet", observation => new DistinctValueObject<string>(observation.SpecificEpithet)},
                    {"Subgenus", observation => new DistinctValueObject<string>(observation.Subgenus)},
                    {"TaxonConceptID", observation => new DistinctValueObject<string>(observation.TaxonConceptID)},
                    {"TaxonID", observation => new DistinctValueObject<string>(observation.TaxonID)},
                    {"TaxonomicStatus", observation => new DistinctValueObject<string>(observation.TaxonomicStatus)},
                    {"TaxonRank", observation => new DistinctValueObject<string>(observation.TaxonRank)},
                    {"TaxonRemarks", observation => new DistinctValueObject<string>(observation.TaxonRemarks)},
                    {
                        "VerbatimTaxonRank",
                        observation => new DistinctValueObject<string>(observation.VerbatimTaxonRank)
                    },
                    {"VernacularName", observation => new DistinctValueObject<string>(observation.VernacularName)},
                    {
                        "FromLithostratigraphicUnit",
                        observation => new DistinctValueObject<string>(observation.FromLithostratigraphicUnit)
                    },
                    {"InDataset", observation => new DistinctValueObject<string>(observation.InDataset)},
                    {"InDescribedPlace", observation => new DistinctValueObject<string>(observation.InDescribedPlace)},
                    {"ToTaxon", observation => new DistinctValueObject<string>(observation.ToTaxon)},
                    {"InCollection", observation => new DistinctValueObject<string>(observation.InCollection)}
                };
            }
        }


        [Fact]
        public void CreateExcelFileWithDwcDataStatistics()
        {
            var filePath = @"c:\temp\DwcDataStatistics.xlsx";
            const int maxNrOfValues = 10000;
            var collectionName = nameof(DwcObservationVerbatim);
            var dwcRepository = CreateArchiveVerbatimRepository();

            // Delete the file if it exists.
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using var fileStream = File.Create(filePath);
            using var package = new ExcelPackage(fileStream);
            var summaryWorksheet = package.Workbook.Worksheets.Add("Summary");
            var fieldsWithoutValues = new List<string>();
            int row;
            //foreach (var pair in DwcPropertyExpressionDictionary.Skip(0).Take(50))
            foreach (var pair in DwcPropertyExpressionDictionary)
            {
                try
                {
                    var valueCountList =
                        dwcRepository.GetDistinctValuesCount(collectionName, pair.Value, maxNrOfValues);
                    var containValues = valueCountList.Any(m => !string.IsNullOrWhiteSpace(m.Value));
                    if (!containValues)
                    {
                        fieldsWithoutValues.Add(pair.Key);
                        continue;
                    }

                    // Add headers
                    var fieldWorksheet = package.Workbook.Worksheets.Add(pair.Key);
                    fieldWorksheet.Cells[1, 1].Value = "Value";
                    fieldWorksheet.Cells[1, 2].Value = "Count";
                    fieldWorksheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;

                    // Add values
                    row = 2;
                    foreach (var valueCount in valueCountList)
                    {
                        fieldWorksheet.Cells[row, 1].Value = valueCount.Value ?? "null";
                        fieldWorksheet.Cells[row, 2].Value = valueCount.Count;
                        fieldWorksheet.Cells[row, 2].Style.Numberformat.Format = "0";
                        row++;
                    }

                    fieldWorksheet.Cells.AutoFitColumns();
                }
                catch (Exception e)
                {
                    _testOutputHelper.WriteLine(e.ToString());
                    //throw;
                }
            }

            // Summarize all fields without values
            summaryWorksheet.Cells[1, 1].Value = "Fields without any values";
            summaryWorksheet.Cells[1, 1].Style.Font.Bold = true;
            row = 2;
            foreach (var fieldsWithoutValue in fieldsWithoutValues)
            {
                summaryWorksheet.Cells[row, 1].Value = fieldsWithoutValue;
                row++;
            }

            summaryWorksheet.Cells.AutoFitColumns();
            package.Save();
        }

        [Fact]
        public async Task Harvest_all_dwca_files_in_specified_folder()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string folderPath = @"C:\DwC-A\NRM IPT\";
            //const string folderPath = @"C:\DwC-A\NRM IPT\Fixed problematic files\";
            var filePaths = Directory.GetFiles(folderPath, "*.zip");
            var dwcObservationHarvester = CreateDwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var harvestInfo = await dwcObservationHarvester.HarvestMultipleDwcaFilesAsync(
                filePaths,
                true,
                JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }

        //[Fact]
        //public void CreateProperties()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (var property in typeof(DwcObservationVerbatim).GetProperties())
        //    {
        //        sb.AppendLine($"{{\"{property.Name}\",observation => new ValueObject<string>(observation.{property.Name})}},");
        //    }

        //    var result = sb.ToString();
        //}
    }
}