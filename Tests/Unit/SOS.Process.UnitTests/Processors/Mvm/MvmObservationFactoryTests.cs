using FluentAssertions;
using Moq;
using NetTopologySuite.Geometries;
using SOS.Harvest.Managers;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive;
using SOS.Harvest.Processors.Mvm;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SOS.Process.UnitTests.Processors.Mvm;

public class MvmObservationFactoryTests
{
    private readonly MvmObservationFactory _factory;

    public MvmObservationFactoryTests()
    {        
        _factory = CreateMvmObservationFactory();
    }

    public MvmObservationFactory CreateMvmObservationFactory(ValidationStatusId? verificationStatusId = null)
    {
        var dataProviderDummy = new DataProvider { DefaultVerificationStatus = verificationStatusId };
        var mammaliaTaxa =
            MessagePackHelper.CreateListFromMessagePackFile<Taxon>(
                @"Resources/MammaliaProcessedTaxa.msgpck");
        var mammaliaTaxonByTaxonId = mammaliaTaxa.ToDictionary(t => t.Id, t => t);
        var processedAreaRepositoryStub =
            ProcessedAreaRepositoryStubFactory.Create(AreaType.County, AreaType.Province);
        var vocabularyRepository = VocabularyRepositoryStubFactory.Create();
        var vocabularies = vocabularyRepository.Object.GetAllAsync().Result;
        var dwcaVocabularyById = VocabularyHelper.GetVocabulariesDictionary(
            ExternalSystemId.DarwinCore,
            vocabularies.ToArray(),
            true);

        var areaHelper = new AreaHelper(new AreaConfiguration(), processedAreaRepositoryStub.Object);
        var processConfiguration = new ProcessConfiguration();
        var factory = new MvmObservationFactory(
            dataProviderDummy,
            mammaliaTaxonByTaxonId,
            dwcaVocabularyById,
            areaHelper,
            new ProcessTimeManager(processConfiguration),
            vocabularyRepository.Object,
            processConfiguration);

        return factory;
    }

    /// <summary>
    /// Based on sample 241989: IsQualitative=True with Observerad=1.
    /// Since the observation is qualitative the quantity should be null.
    /// </summary>
    [Fact]
    public void CreateProcessedObservations_QualitativeObservation_WithObserverad_QuantityIsNull()
    {
        // Arrange
        var verbatim = CreateBaseSample(241989, "2006-10-04");
        verbatim.Observations =
        [
            new SampleObservation
            {
                ObservationId = 6273044,
                PropertyRepresentationType = "TaxonProperty",
                IsQualitative = "True",
                PropertyCode = "101268",
                ObservationValues =
                [
                    new SampleObservationValue(name: "Observerad", value: "1", valueType: "BooleanValue")
                ],
                ObservationMetadata = []
            }
        ];

        // Act
        var observations = _factory.CreateProcessedObservations(verbatim, false)?.ToList();

        // Assert
        observations.Should().HaveCount(1);
        var obs = observations!.First();
        obs.Occurrence.IsPositiveObservation.Should().BeTrue();
        obs.Occurrence.OrganismQuantityInt.Should().BeNull();
        obs.Occurrence.OrganismQuantity.Should().BeNull();
        obs.Occurrence.IndividualCount.Should().BeNull();
        obs.Occurrence.OrganismQuantityUnit.Should().BeNull();
    }

    /// <summary>
    /// Based on sample 953452: IsQualitative=False, Antal levande=0, Ej återfunnen=0.
    /// Should result in IsNeverFoundObservation=true.
    /// </summary>
    [Fact]
    public void CreateProcessedObservations_AntalLevande0_EjAterfunnen0_IsNeverFound()
    {
        // Arrange
        var verbatim = CreateBaseSample(953452, "2018-08-28");
        verbatim.Observations =
        [
            new SampleObservation
            {
                ObservationId = 17064715,
                PropertyRepresentationType = "TaxonProperty",
                IsQualitative = "False",
                PropertyCode = "101268",
                ObservationValues =
                [
                    new SampleObservationValue(name: "Antal levande", value: "0", unit: "Antal"),
                    new SampleObservationValue(name: "Ej återfunnen", value: "0", valueType: "BooleanValue")
                ],
                ObservationMetadata = []
            }
        ];

        // Act
        var observations = _factory.CreateProcessedObservations(verbatim, false)?.ToList();

        // Assert
        observations.Should().HaveCount(1);
        var obs = observations!.First();
        obs.Occurrence.IsPositiveObservation.Should().BeFalse();
        obs.Occurrence.IsNeverFoundObservation.Should().BeTrue();
    }

    /// <summary>
    /// Based on sample 960915: IsQualitative=False, Antal levande=0, with 6 individual
    /// Bredd/Höjd/Längd measurements. OrganismQuantityInt should equal the max count
    /// per measurement type (6).
    /// </summary>
    [Fact]
    public void CreateProcessedObservations_IndividualMeasurements_QuantityEqualsMaxMeasurementCount()
    {
        // Arrange
        var verbatim = CreateBaseSample(960915, "2011-05-26");
        verbatim.Observations =
        [
            new SampleObservation
            {
                ObservationId = 17072178,
                PropertyRepresentationType = "TaxonProperty",
                IsQualitative = "False",
                PropertyCode = "101268",
                ObservationValues =
                [
                    new SampleObservationValue(name: "Antal levande", value: "0", unit: "Antal"),
                    new SampleObservationValue(name: "Ej återfunnen", value: "0", valueType: "BooleanValue"),
                    // 6 individuals measured — one Bredd/Höjd/Längd triplet per individual
                    new SampleObservationValue(name: "Bredd (mm)", value: "33", unit: "mm"),
                    new SampleObservationValue(name: "Höjd (mm)",  value: "50", unit: "mm"),
                    new SampleObservationValue(name: "Längd (mm)", value: "103", unit: "mm"),
                    new SampleObservationValue(name: "Bredd (mm)", value: "28", unit: "mm"),
                    new SampleObservationValue(name: "Höjd (mm)",  value: "49", unit: "mm"),
                    new SampleObservationValue(name: "Längd (mm)", value: "92", unit: "mm"),
                    new SampleObservationValue(name: "Bredd (mm)", value: "32", unit: "mm"),
                    new SampleObservationValue(name: "Höjd (mm)",  value: "51", unit: "mm"),
                    new SampleObservationValue(name: "Längd (mm)", value: "102", unit: "mm"),
                    new SampleObservationValue(name: "Bredd (mm)", value: "33", unit: "mm"),
                    new SampleObservationValue(name: "Höjd (mm)",  value: "53", unit: "mm"),
                    new SampleObservationValue(name: "Längd (mm)", value: "105", unit: "mm"),
                    new SampleObservationValue(name: "Bredd (mm)", value: "27", unit: "mm"),
                    new SampleObservationValue(name: "Höjd (mm)",  value: "49", unit: "mm"),
                    new SampleObservationValue(name: "Längd (mm)", value: "96", unit: "mm"),
                    new SampleObservationValue(name: "Bredd (mm)", value: "32", unit: "mm"),
                    new SampleObservationValue(name: "Höjd (mm)",  value: "49", unit: "mm"),
                    new SampleObservationValue(name: "Längd (mm)", value: "97", unit: "mm")
                ],
                ObservationMetadata = []
            }
        ];

        // Act
        var observations = _factory.CreateProcessedObservations(verbatim, false)?.ToList();

        // Assert
        observations.Should().HaveCount(1);
        var obs = observations!.First();
        obs.Occurrence.IsPositiveObservation.Should().BeTrue();
        obs.Occurrence.OrganismQuantityInt.Should().Be(6);
        obs.Occurrence.OrganismQuantity.Should().Be("6");
        obs.Occurrence.IndividualCount.Should().Be("6");
    }

    /// <summary>
    /// IsQualitative=False, Antal levande=4, with 6 individual
    /// Bredd/Höjd/Längd measurements. OrganismQuantityInt should equal the max count
    /// per measurement type (6).
    /// </summary>
    [Fact]
    public void CreateProcessedObservations_IndividualMeasurements_QuantityEqualsMaxMeasurementCountPlusAntalLevande()
    {
        // Arrange
        var verbatim = CreateBaseSample(960915, "2011-05-26");
        verbatim.Observations =
        [
            new SampleObservation
            {
                ObservationId = 17072178,
                PropertyRepresentationType = "TaxonProperty",
                IsQualitative = "False",
                PropertyCode = "101268",
                ObservationValues =
                [
                    new SampleObservationValue(name: "Antal levande", value: "4", unit: "Antal"),
                    new SampleObservationValue(name: "Ej återfunnen", value: "0", valueType: "BooleanValue"),
                    // 6 individuals measured — one Bredd/Höjd/Längd triplet per individual
                    new SampleObservationValue(name: "Bredd (mm)", value: "33", unit: "mm"),
                    new SampleObservationValue(name: "Höjd (mm)",  value: "50", unit: "mm"),
                    new SampleObservationValue(name: "Längd (mm)", value: "103", unit: "mm"),
                    new SampleObservationValue(name: "Bredd (mm)", value: "28", unit: "mm"),
                    new SampleObservationValue(name: "Höjd (mm)",  value: "49", unit: "mm"),
                    new SampleObservationValue(name: "Längd (mm)", value: "92", unit: "mm"),
                    new SampleObservationValue(name: "Bredd (mm)", value: "32", unit: "mm"),
                    new SampleObservationValue(name: "Höjd (mm)",  value: "51", unit: "mm"),
                    new SampleObservationValue(name: "Längd (mm)", value: "102", unit: "mm"),
                    new SampleObservationValue(name: "Bredd (mm)", value: "33", unit: "mm"),
                    new SampleObservationValue(name: "Höjd (mm)",  value: "53", unit: "mm"),
                    new SampleObservationValue(name: "Längd (mm)", value: "105", unit: "mm"),
                    new SampleObservationValue(name: "Bredd (mm)", value: "27", unit: "mm"),
                    new SampleObservationValue(name: "Höjd (mm)",  value: "49", unit: "mm"),
                    new SampleObservationValue(name: "Längd (mm)", value: "96", unit: "mm"),
                    new SampleObservationValue(name: "Bredd (mm)", value: "32", unit: "mm"),
                    new SampleObservationValue(name: "Höjd (mm)",  value: "49", unit: "mm"),
                    new SampleObservationValue(name: "Längd (mm)", value: "97", unit: "mm")
                ],
                ObservationMetadata = []
            }
        ];

        // Act
        var observations = _factory.CreateProcessedObservations(verbatim, false)?.ToList();

        // Assert
        observations.Should().HaveCount(1);
        var obs = observations!.First();
        obs.Occurrence.IsPositiveObservation.Should().BeTrue();
        obs.Occurrence.OrganismQuantityInt.Should().Be(10);
        obs.Occurrence.OrganismQuantity.Should().Be("10");
        obs.Occurrence.IndividualCount.Should().Be("10");
    }

    /// <summary>
    /// Based on sample 64033 (Växtplankton): IsQualitative=False, antal=43.
    /// OrganismQuantityInt should be taken directly from the antal field.
    /// </summary>
    [Fact]
    public void CreateProcessedObservations_AntalPresent_QuantityFromAntal()
    {
        // Arrange
        var verbatim = CreateBaseSample(64033, "1986-08-12");
        verbatim.Observations =
        [
            new SampleObservation
            {
                ObservationId = 1739386,
                PropertyRepresentationType = "TaxonProperty",
                IsQualitative = "False",
                PropertyCode = "264148",
                ObservationValues =
                [
                    new SampleObservationValue(name: "antal", value: "43", unit: "Antal")
                ],
                ObservationMetadata = []
            }
        ];

        // Act
        var observations = _factory.CreateProcessedObservations(verbatim, false)?.ToList();

        // Assert
        observations.Should().HaveCount(1);
        var obs = observations!.First();
        obs.Occurrence.IsPositiveObservation.Should().BeTrue();
        obs.Occurrence.OrganismQuantityInt.Should().Be(43);
        obs.Occurrence.OrganismQuantity.Should().Be("43");
        obs.Occurrence.IndividualCount.Should().Be("43");
    }

    /// <summary>
    /// IsQualitative=False with Observerad=1: the observation is positive but the
    /// quantity is unknown, so all quantity fields should be null.
    /// </summary>
    [Fact]
    public void CreateProcessedObservations_NonQualitative_Observerad1_QuantityIsNull()
    {
        // Arrange
        var verbatim = CreateBaseSample(241990, "2006-10-04");
        verbatim.Observations =
        [
            new SampleObservation
            {
                ObservationId = 6273045,
                PropertyRepresentationType = "TaxonProperty",
                IsQualitative = "False",
                PropertyCode = "101268",
                ObservationValues =
                [
                    new SampleObservationValue(name: "Observerad", value: "1", valueType: "BooleanValue")
                ],
                ObservationMetadata = []
            }
        ];

        // Act
        var observations = _factory.CreateProcessedObservations(verbatim, false)?.ToList();

        // Assert
        observations.Should().HaveCount(1);
        var obs = observations!.First();
        obs.Occurrence.IsPositiveObservation.Should().BeTrue();
        obs.Occurrence.OrganismQuantityInt.Should().BeNull();
        obs.Occurrence.OrganismQuantity.Should().BeNull();
        obs.Occurrence.IndividualCount.Should().BeNull();
        obs.Occurrence.OrganismQuantityUnit.Should().BeNull();
    }

    [Fact]
    public void CreateProcessedObservations_DynamicProperties_PreservesSwedishCharacters()
    {
        // Arrange
        var verbatim = CreateBaseSample(241991, "2006-10-04");
        verbatim.StudyName = "Räksmörgås åäö";
        verbatim.SubProgramName = "Mätning öst";
        verbatim.ProductName = "Blåbärssoppa ä";
        verbatim.DeliveryTitle = "Fältbesök Åland";
        verbatim.Observations =
        [
            new SampleObservation
            {
                ObservationId = 6273046,
                PropertyRepresentationType = "TaxonProperty",
                IsQualitative = "False",
                PropertyCode = "101268",
                ObservationValues =
                [
                    new SampleObservationValue(name: "Observerad", value: "1", valueType: "BooleanValue")
                ],
                ObservationMetadata = []
            }
        ];

        // Act
        var observations = _factory.CreateProcessedObservations(verbatim, false)?.ToList();

        // Assert
        observations.Should().HaveCount(1);
        var obs = observations!.First();
        obs.DynamicProperties.Should().Contain("Räksmörgås åäö");
        obs.DynamicProperties.Should().Contain("Mätning öst");
        obs.DynamicProperties.Should().Contain("Blåbärssoppa ä");
        obs.DynamicProperties.Should().Contain("Fältbesök Åland");
        obs.DynamicProperties.Should().NotContain("\\u00");
    }

    [Fact]
    public void CreateProcessedObservations_MetadataCoordinatesWithRt90_UsesRt90CoordinateSystem()
    {
        // Arrange
        const double northing = 6580822;
        const double easting = 1627548;
        var expectedPoint = new Point(easting, northing)
            .Transform(CoordinateSys.Rt90_25_gon_v, CoordinateSys.WGS84) as Point;

        var verbatim = CreateBaseSample(241992, "2006-10-04");
        verbatim.Observations =
        [
            new SampleObservation
            {
                ObservationId = 6273047,
                PropertyRepresentationType = "TaxonProperty",
                IsQualitative = "False",
                PropertyCode = "101268",
                ObservationValues =
                [
                    new SampleObservationValue(name: "Observerad", value: "1", valueType: "BooleanValue")
                ],
                ObservationMetadata =
                [
                    new Metadata { Label = "Provplatskoordinat START N/X", Value = northing.ToString() },
                    new Metadata { Label = "Provplatskoordinat START E/Y", Value = easting.ToString() },
                    new Metadata { Label = "Provplatskoordinat SLUT N/X", Value = northing.ToString() },
                    new Metadata { Label = "Provplatskoordinat SLUT E/Y", Value = easting.ToString() },
                    new Metadata { Label = "Provplatskoordinaternas koordinatsystem", Value = "RT90" }
                ]
            }
        ];

        // Act
        var observations = _factory.CreateProcessedObservations(verbatim, false)?.ToList();

        // Assert
        observations.Should().HaveCount(1);
        var obs = observations!.First();
        obs.Location.VerbatimSRS.Should().Be(CoordinateSys.Rt90_25_gon_v.EpsgCode());
        obs.Location.VerbatimLongitude.Should().Be(easting.ToString("0"));
        obs.Location.VerbatimLatitude.Should().Be(northing.ToString("0"));
        obs.Location.DecimalLongitude.Should().BeApproximately(expectedPoint!.X, 0.00001);
        obs.Location.DecimalLatitude.Should().BeApproximately(expectedPoint.Y, 0.00001);
    }

    private static MvmSampleVerbatim CreateBaseSample(int sampleId, string samplingDate) =>
        new MvmSampleVerbatim
        {
            Id = sampleId,
            SampleId = sampleId,
            SamplingDate = samplingDate,
            InsertDate = samplingDate,
            StationId = 1,
            StationName = "Test Station",
            StationCoordinateX = 500000,
            StationCoordinateY = 6600000,
            SampleMetadata = [],
            GroupCalculations = []
        };
}
