using SOS.Api.Contracts.Observations;
using SOS.Api.Contracts.Common.Geo;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace SOS.Harvest.Processors.Mvm;

public class MvmObservationFactory : ObservationFactoryBase, IObservationFactory<MvmSampleVerbatim>
{
    private static readonly JsonSerializerOptions DynamicPropertiesSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement), // Display å,ä,ö e.t.c. properly
    };

    private readonly IAreaHelper _areaHelper;
    private string _englishOrganizationName;
    private string _englishOrganizationNameLowerCase;
    private VocabularyValue? _institutionCodeVocabularyValue;    

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dataProvider"></param>
    /// <param name="taxa"></param>
    /// <param name="areaHelper"></param>
    /// <param name="processTimeManager"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MvmObservationFactory(DataProvider dataProvider,
        IDictionary<int, Lib.Models.Processed.Observation.Taxon>? taxa,
        IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,
        IAreaHelper areaHelper,
        IProcessTimeManager processTimeManager,
        IVocabularyRepository processedVocabularyRepository,
        ProcessConfiguration processConfiguration) : base(dataProvider, taxa, dwcaVocabularyById, processTimeManager, processConfiguration)
    {
        _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        _englishOrganizationName = dataProvider?.Organizations?.Translate("en-GB")!;
        _englishOrganizationNameLowerCase = _englishOrganizationName?.ToLower()!;
        if (VocabularyById != null && VocabularyById.ContainsKey(VocabularyId.Institution))
        {
            _institutionCodeVocabularyValue = GetSosId(_englishOrganizationName,
                VocabularyById[VocabularyId.Institution],
                null,
                MappingNotFoundLogic.UseSourceValue,
                _englishOrganizationNameLowerCase);
        }
    }

    /// <summary>
    ///  Cast verbatim observations to processed data model
    /// </summary>
    /// <param name="verbatim"></param>
    /// <param name="diffuseIfSupported"></param>
    /// <returns></returns>
    public Observation CreateProcessedObservation(MvmSampleVerbatim verbatim, bool diffuseIfSupported)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Observation>? CreateProcessedObservations(MvmSampleVerbatim verbatim, bool diffuseIfSupported)
    {
        if (verbatim == null || verbatim.Observations == null)
        {
            return null;
        }

        List<Observation> observations = new List<Observation>();        

        var dates = ParseDates(verbatim);

        if (dates.SamplingDate == null)
        {
            return observations;
        }

        var accessRights = VocabularyValue.Create((int)AccessRightsId.FreeUsage);

        foreach (var verbatimObservation in verbatim.Observations)
        {
            if (string.IsNullOrEmpty(verbatimObservation.PropertyRepresentationType))
            {
                continue;
            }

            if (!verbatimObservation.PropertyRepresentationType.Equals("TaxonProperty", StringComparison.InvariantCultureIgnoreCase))
            {                                
                continue;
            }

            var obs = new Observation
            {
                AccessRights = accessRights,
                DataProviderId = DataProvider.Id,
                DiffusionStatus = DiffusionStatus.NotDiffused,
                MongoDbId = verbatim.Id
            };

            // Record level
            obs.BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation };
            obs.DatasetName = "MVM";
            obs.Modified = dates.EditDate != null ? dates.EditDate.Value.ToUniversalTime() : dates.InsertDate != null ? dates.InsertDate.Value.ToUniversalTime() : dates.SamplingDate!.Value.ToUniversalTime();
            obs.InstitutionCode = _institutionCodeVocabularyValue;
            obs.OwnerInstitutionCode = "MVM";
            obs.MeasurementOrFacts = CreateMeasurementOrFacts(verbatim, verbatimObservation);
            obs.DynamicProperties = JsonSerializer.Serialize(new
            {
                StudyName = verbatim.StudyName?.Clean(),
                SubProgram = verbatim.SubProgramName?.Clean(),
                ProductName = verbatim.ProductName?.Clean(),
                DeliveryTitle = verbatim.DeliveryTitle?.Clean()
            }, DynamicPropertiesSerializerOptions);

            // Event
            obs.Event = CreateProcessedEvent(verbatim, dates.SamplingDate!.Value);

            // Identification
            obs.Identification = CreateProcessedIdentification(verbatim);

            // Taxon
            int? taxonId = int.TryParse(verbatimObservation.PropertyCode, out var parsedTaxonId) ? parsedTaxonId : null; 
            var taxon = GetTaxon(taxonId.HasValue ? taxonId.Value : -1, [verbatimObservation.PropertyName]);
            obs.Taxon = taxon;

            // Location
            obs.Location = CreateProcessedLocation(verbatim, verbatimObservation, obs.Taxon);

            // Occurrence
            obs.Occurrence = CreateProcessedOccurrence(verbatim, verbatimObservation, dates, obs.Taxon, obs.AccessRights != null ? (AccessRightsId)obs.AccessRights.Id : null);

            if (obs.ShallBeProtected())
            {
                obs.Sensitive = true;

                if (obs.AccessRights?.Id == (int)AccessRightsId.NotForPublicUsage && obs.Occurrence.SensitivityCategory < 3 && (obs.Taxon?.Attributes?.SensitivityCategory?.Id ?? 0) < 3)
                {
                    obs.Occurrence.SensitivityCategory = 3;
                }
            }

            // Populate generic data
            PopulateGenericData(obs);
            obs.Occurrence.BirdNestActivityId = GetBirdNestActivityId(obs.Occurrence.Activity, obs.Taxon);
            PopulateQuantityDetails(verbatimObservation, obs);
            PopulateLifeStageAndSex(verbatimObservation, obs);
            if (obs.Occurrence.IsNotRediscoveredObservation) // Don't include observations that are not rediscovered
            {
                continue;
            }

            obs.Occurrence.OccurrenceStatus = obs.Occurrence.IsPositiveObservation ?
                new VocabularyValue { Id = (int)OccurrenceStatusId.Present } :
                new VocabularyValue { Id = (int)OccurrenceStatusId.Absent };

            observations.Add(obs);
        }

        return observations;
    }

    private static void PopulateLifeStageAndSex(SampleObservation verbatimObservation, Observation obs)
    {
        Dictionary<string, int> sexMappings = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "Hane", (int)SexId.Male },
                { "Hona", (int)SexId.Female }
            };

        Dictionary<string, int> lifeStageMappings = new Dictionary<string, int>
            {
                { "Juvenil", (int)LifeStageId.Juvenile },
                { "copepodit", (int)LifeStageId.Copepodite },
                { "copepodit 2", (int)LifeStageId.Copepodite },
                { "copepodit 3", (int)LifeStageId.Copepodite },
                { "copepodit 4", (int)LifeStageId.Copepodite },
                { "copepodit 5", (int)LifeStageId.Copepodite },
                { "Fria ägg", (int)LifeStageId.Egg },
                { "Hona med ägg", (int)LifeStageId.Egg },
                { "Adult", (int)LifeStageId.Adult },
                { "Nauplie", (int)LifeStageId.NaupliusLarva }
            };

        SampleObservationValue? fysiologiValue = verbatimObservation.ObservationValues?
            .Where(m => m != null && m.Name != null)
            .FirstOrDefault(m => m.Name.Equals("fysiologi", StringComparison.InvariantCultureIgnoreCase));
        if (fysiologiValue != null && !string.IsNullOrEmpty(fysiologiValue.Value))
        {
            if (sexMappings.TryGetValue(fysiologiValue.Value, out var sexId))
            {
                obs.Occurrence.Sex = VocabularyValue.Create(sexId);
            }
            if (lifeStageMappings.TryGetValue(fysiologiValue.Value, out var lifeStageId))
            {
                obs.Occurrence.LifeStage = VocabularyValue.Create(lifeStageId);
            }
        }
    }

    private void PopulateQuantityDetails(SampleObservation verbatimObservation, Observation obs)
    {
        var antalValue = GetObservationValue(verbatimObservation, "antal");
        var antalLevandeValue = GetObservationValue(verbatimObservation, "antal levande");
        var observeradValue = GetObservationValue(verbatimObservation, "observerad");

        var isPositiveObservation = true;
        int? quantity = null;

        if (antalValue != null && int.TryParse(antalValue.Value, out var antalQuantity))
        {
            quantity = antalQuantity;
        }
        else if (antalLevandeValue != null && int.TryParse(antalLevandeValue.Value, out var antalLevandeQuantity))
        {
            if (antalLevandeQuantity > 0)
            {
                quantity = antalLevandeQuantity;
            }
            else
            {
                isPositiveObservation = false;
            }
        }

        if (observeradValue != null)
        {
            isPositiveObservation = observeradValue.Value == "1";
            if (isPositiveObservation && IsQualitativeObservation(verbatimObservation))
            {
                quantity = null;
            }
        }
        else
        {
            var individualMeasurementCount = CountIndividualMeasurements(verbatimObservation);
            if (individualMeasurementCount > 0)
            {
                quantity = quantity.GetValueOrDefault() + individualMeasurementCount;
                isPositiveObservation = true;
            }
        }

        SetQuantity(obs, quantity);
        obs.Occurrence.IsPositiveObservation = isPositiveObservation;
        obs.Occurrence.IsNeverFoundObservation = !isPositiveObservation;

        CalculateOrganismQuantity(obs);
    }

    private static SampleObservationValue? GetObservationValue(SampleObservation verbatimObservation, string name)
    {
        return verbatimObservation.ObservationValues?
            .FirstOrDefault(m => m != null && m.Name != null && m.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
    }

    private static bool IsQualitativeObservation(SampleObservation verbatimObservation)
    {
        return string.Equals(verbatimObservation.IsQualitative, "true", StringComparison.OrdinalIgnoreCase)
            || verbatimObservation.IsQualitative == "1";
    }

    private static void SetQuantity(Observation obs, int? quantity)
    {
        if (!quantity.HasValue)
        {
            obs.Occurrence.OrganismQuantityInt = null;
            obs.Occurrence.OrganismQuantity = null;
            obs.Occurrence.OrganismQuantityUnit = null;
            obs.Occurrence.IndividualCount = null;
            return;
        }

        obs.Occurrence.OrganismQuantityInt = quantity.Value;
        obs.Occurrence.OrganismQuantity = quantity.Value.ToString();
        obs.Occurrence.OrganismQuantityUnit = new VocabularyValue { Id = (int)UnitId.Individuals };
        obs.Occurrence.IndividualCount = quantity.Value.ToString();
    }

    private static int CountIndividualMeasurements(SampleObservation verbatimObservation)
    {
        if (verbatimObservation.ObservationValues == null || verbatimObservation.ObservationValues.Count == 0)
        {
            return 0;
        }

        var measurementPrefixes = new[] { "Bredd", "Höjd", "Längd" };

        var validValues = verbatimObservation.ObservationValues
            .Where(m => m != null && !string.IsNullOrEmpty(m.Name) && !string.IsNullOrEmpty(m.Value)
                && double.TryParse(m.Value, out var val) && val > 0)
            .ToList();

        if (validValues.Count == 0)
            return 0;

        return measurementPrefixes
            .Select(prefix => validValues.Count(m => m.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .Max();
    }

    private string FormatMeasurementValue(SampleObservationValue observationValue)
    {
        if (observationValue?.ValueType == "BooleanValue")
        {
            if (observationValue?.Value == "1")
            {
                return "True";
            }
            else if (observationValue?.Value == "0")
            {
                return "False";
            }
        }
        
        return observationValue.Value;
    }

    private (DateTime? SamplingDate, DateTime? InsertDate, DateTime? EditDate) ParseDates(MvmSampleVerbatim verbatim)
    {
        DateTime? samplingDate = null;
        DateTime? insertDate = null;
        DateTime? editDate = null;
        if (DateTime.TryParse(verbatim.SamplingDate, out var parsedSamplingDate))
        {
            samplingDate = parsedSamplingDate;
        }
        if (DateTime.TryParse(verbatim.InsertDate, out var parsedInsertDate))
        {
            insertDate = parsedInsertDate;
        }
        if (DateTime.TryParse(verbatim.EditDate, out var parsedEditDate))
        {
            editDate = parsedEditDate;
        }

        return (samplingDate, insertDate, editDate);
    }

    private List<ExtendedMeasurementOrFact>? CreateMeasurementOrFacts(MvmSampleVerbatim verbatim, SampleObservation verbatimObservation)
    {
        if ((verbatimObservation?.ObservationValues == null || verbatimObservation?.ObservationValues.Count == 0) && 
            (verbatimObservation?.ObservationMetadata == null || verbatimObservation?.ObservationMetadata.Count == 0))
        {
            return null;
        }

        var facts = new List<ExtendedMeasurementOrFact>();
        foreach (var observationValue in verbatimObservation!.ObservationValues)
        {
            if (observationValue == null || string.IsNullOrEmpty(observationValue.Name) || string.IsNullOrEmpty(observationValue.Value))
            {
                continue;
            }

            facts.Add(new ExtendedMeasurementOrFact
            {
                MeasurementID = observationValue.ObservationValueId,
                MeasurementType = observationValue.Name,
                MeasurementUnit = observationValue.Unit,
                MeasurementValue = FormatMeasurementValue(observationValue)
            });
        }         

        foreach (var observationMetaData in verbatimObservation!.ObservationMetadata)
        {
            if (observationMetaData == null || string.IsNullOrEmpty(observationMetaData.Label) || string.IsNullOrEmpty(observationMetaData.Value))
            {
                continue;
            }
            facts.Add(new ExtendedMeasurementOrFact
            {                    
                MeasurementType = observationMetaData.Label,
                MeasurementValue = observationMetaData.Value
            });
        }

        return facts;
    }

    private HashSet<string> _ignoreLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Rapportör Musselportalen"
    };

    private List<ExtendedMeasurementOrFact>? CreateEventMeasurementOrFacts(MvmSampleVerbatim verbatim)
    {
        if ((verbatim?.GroupCalculations == null || verbatim?.GroupCalculations.Count == 0) &&
            (verbatim?.SampleMetadata == null || verbatim?.SampleMetadata.Count == 0))
        {
            return null;
        }

        var facts = new List<ExtendedMeasurementOrFact>();
        foreach (var observationMetaData in verbatim!.SampleMetadata)
        {
            if (observationMetaData == null || string.IsNullOrEmpty(observationMetaData.Label) || string.IsNullOrEmpty(observationMetaData.Value))
            {
                continue;
            }
            if (_ignoreLabels.Contains(observationMetaData.Label))
            {
                continue;
            }

            facts.Add(new ExtendedMeasurementOrFact
            {
                MeasurementType = observationMetaData.Label,
                MeasurementValue = observationMetaData.Value
            });
        }

        // Don't include GroupCalculations in the event measurement or facts for now
        //foreach (var val in verbatim!.GroupCalculations)
        //{
        //    if (!string.IsNullOrEmpty(val.BioSum))
        //    {
        //        facts.Add(new ExtendedMeasurementOrFact
        //        {                        
        //            MeasurementType = $"BioSum: {val.GroupPropertyName}",
        //            MeasurementUnit = val.BioSumUnit,
        //            MeasurementValue = val.BioSum
        //        });
        //    }

        //    if (!string.IsNullOrEmpty(val.AbundanceSum))
        //    {
        //        facts.Add(new ExtendedMeasurementOrFact
        //        {
        //            MeasurementType = $"AbundanceSum: {val.GroupPropertyName}",
        //            MeasurementUnit = val.AbundanceSumUnit,
        //            MeasurementValue = val.AbundanceSum
        //        });
        //    }
        //}            

        return facts;
    }

    public bool IsVerbatimObservationDiffusedByProvider(MvmSampleVerbatim verbatim)
    {
        return false;
    }      

    private Event CreateProcessedEvent(MvmSampleVerbatim verbatim, DateTime samplingDate)
    {
        var processedEvent = new Event(samplingDate, ignoreTimePart: true);
        processedEvent.EventId = $"urn:lsid:mvm:event:{verbatim.SampleId}";
        processedEvent.Habitat = verbatim.MediumTypeName?.Clean();
        //processedEvent.EventRemarks = $"StudyName={verbatim.StudyName?.Clean()}, SubProgram={verbatim.SubProgramName?.Clean()}, ProductName={verbatim.ProductName?.Clean()}, DeliveryTitle={verbatim.DeliveryTitle?.Clean()}";
        processedEvent.MeasurementOrFacts = CreateEventMeasurementOrFacts(verbatim);
        return processedEvent;
    }

    private Lib.Models.Processed.Observation.Identification CreateProcessedIdentification(MvmSampleVerbatim verbatim)
    {
        var processedIdentification = new Lib.Models.Processed.Observation.Identification();
        processedIdentification.VerificationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert };
        processedIdentification.Verified = true;
        processedIdentification.UncertainIdentification = false;
        return processedIdentification;
    }        

    private Location CreateProcessedLocation(MvmSampleVerbatim verbatim, SampleObservation observation, SOS.Lib.Models.Processed.Observation.Taxon taxon)
    {
        /*
           "nationalStationId": "261111",
           "nationalSiteId": "120143",
           "samplingSiteId": 81,
           "samplingSiteName": "Norra Yngern",
           "samplingSiteX": "637680",
           "samplingSiteY": "6561452",
           "samplingSiteWkt": "POINT (637680 6561452)",
           "samplingSiteCoordinateSystem": "SWEREF99 TM",
           "stationName": "Yngern",
           "stationType": "Sjö",
           "stationEUID": "SE656206-159170",
           "stationCoordinateX": 633790,
           "stationCoordinateY": 6558114,
           "stationCoordinateSystem": "SWEREF99 TM",
           "stationIsBlurred": false,
        */

        // SamplingSiteX and SamplingSiteY doesn't always exists, but SamplingSiteId always exists.
        // StationCoordinateX and StationCoordinateY always exists and StationId always exists.
        //

        var processedLocation = new Location(LocationType.Point);
        //processedLocation.Locality = verbatim.SamplingSiteName?.Clean();
        //processedLocation.LocationId = $"urn:lsid:mvm.se:site:{verbatim.SamplingSiteId}";
        processedLocation.MinimumDepthInMeters = verbatim.MinDepth;
        processedLocation.MaximumDepthInMeters = verbatim.MaxDepth;
        processedLocation.Locality = verbatim.StationName?.Clean();
        processedLocation.LocationId = $"urn:lsid:mvm.se:site:{verbatim.StationId}";

        double? verbatimLongitude;
        double? verbatimLatitude;
        int? coordinateUncertaintyInMeters;
        var verbatimCoordinateSystem = CoordinateSys.SWEREF99_TM;

        if (GetCoordinatesFromObservationMetadata(observation.ObservationMetadata, out verbatimLongitude, out verbatimLatitude, out coordinateUncertaintyInMeters, out verbatimCoordinateSystem))
        {
            // Observation metadata contains coordinates                
        }
        else
        {
            verbatimLongitude = double.TryParse(verbatim.SamplingSiteX, out var parsedSamplingSiteX) ? parsedSamplingSiteX : null;
            verbatimLatitude = double.TryParse(verbatim.SamplingSiteY, out var parsedSamplingSiteY) ? parsedSamplingSiteY : null;
            if (verbatimLongitude.HasValue && verbatimLatitude.HasValue) // SamplingSiteX and SamplingSiteY exists
            {
                coordinateUncertaintyInMeters = 1000;
                processedLocation.LocationRemarks = verbatim.SamplingSiteName?.Clean();
            }
            else // Use StationCoordinateX and StationCoordinateY
            {
                verbatimLongitude = verbatim.StationCoordinateX;
                verbatimLatitude = verbatim.StationCoordinateY;
                coordinateUncertaintyInMeters = 5000;
            }
        }

        AddPositionData(
            processedLocation,
            verbatimLongitude,
            verbatimLatitude,
            verbatimCoordinateSystem,
            coordinateUncertaintyInMeters,
            taxon?.Attributes?.DisturbanceRadius);
        
        _areaHelper.AddAreaDataToProcessedLocation(processedLocation);

        return processedLocation;
    }

    private bool GetCoordinatesFromObservationMetadata(List<Metadata> metadata, out double? longitude, out double? latitude, out int? coordinateUncertaintyInMeters, out CoordinateSys coordinateSystem)
    {
        coordinateSystem = CoordinateSys.SWEREF99_TM;
        if (metadata == null || metadata.Count == 0)
        {
            longitude = null;
            latitude = null;
            coordinateUncertaintyInMeters = null;
            return false;
        }

        Metadata? sampleCoordinateXStart = metadata.FirstOrDefault(m => m.Label.Equals("Provplatskoordinat START N/X", StringComparison.OrdinalIgnoreCase));
        if (sampleCoordinateXStart == null)
        {
            longitude = null;
            latitude = null;
            coordinateUncertaintyInMeters = null;
            return false;
        }

        Metadata? sampleCoordinateYStart = metadata.FirstOrDefault(m => m.Label.Equals("Provplatskoordinat START E/Y", StringComparison.OrdinalIgnoreCase));
        Metadata? sampleCoordinateXEnd = metadata.FirstOrDefault(m => m.Label.Equals("Provplatskoordinat SLUT N/X", StringComparison.OrdinalIgnoreCase));
        Metadata? sampleCoordinateYEnd = metadata.FirstOrDefault(m => m.Label.Equals("Provplatskoordinat SLUT E/Y", StringComparison.OrdinalIgnoreCase));

        if (sampleCoordinateYStart == null || sampleCoordinateXEnd == null || sampleCoordinateYEnd == null ||
            string.IsNullOrWhiteSpace(sampleCoordinateXStart.Value) || string.IsNullOrWhiteSpace(sampleCoordinateYStart.Value) ||
            string.IsNullOrWhiteSpace(sampleCoordinateXEnd.Value) || string.IsNullOrWhiteSpace(sampleCoordinateYEnd.Value))
        {
            longitude = null;
            latitude = null;
            coordinateUncertaintyInMeters = null;
            return false;
        }

        if (!double.TryParse(sampleCoordinateXStart.Value, out var xStart) ||
            !double.TryParse(sampleCoordinateYStart.Value, out var yStart) ||
            !double.TryParse(sampleCoordinateXEnd.Value, out var xEnd) ||
            !double.TryParse(sampleCoordinateYEnd.Value, out var yEnd))
        {
            longitude = null;
            latitude = null;
            coordinateUncertaintyInMeters = null;
            return false;
        }

        coordinateSystem = GetObservationMetadataCoordinateSystem(metadata);        

        // Calculate center point between start and end coordinates
        latitude = (xStart + xEnd) / 2;
        longitude = (yStart + yEnd) / 2;

        // Calculate uncertainty as distance between start and end points
        var dx = Math.Abs(xStart - xEnd);
        var dy = Math.Abs(yStart - yEnd);
        coordinateUncertaintyInMeters = (int) Math.Ceiling(Math.Sqrt((dx / 2) * (dx / 2) + (dy / 2) * (dy / 2)));
        return true;
    }

    private static CoordinateSys GetObservationMetadataCoordinateSystem(List<Metadata> metadata)
    {
        var coordinateSystemMetadata = metadata.FirstOrDefault(m =>
            m.Label.Equals("Provplatskoordinaternas koordinatsystem", StringComparison.OrdinalIgnoreCase));

        return string.Equals(coordinateSystemMetadata?.Value, "RT90", StringComparison.OrdinalIgnoreCase)
            ? CoordinateSys.Rt90_25_gon_v
            : CoordinateSys.SWEREF99_TM;
    }


    private Occurrence CreateProcessedOccurrence(
        MvmSampleVerbatim verbatim, 
        SampleObservation verbatimObservation, 
        (DateTime? SamplingDate, DateTime? InsertDate, DateTime? EditDate) dates, 
        Lib.Models.Processed.Observation.Taxon? taxon, 
        AccessRightsId? accessRightsId)
    {
        var processedOccurrence = new Occurrence();
        processedOccurrence.CatalogNumber = verbatimObservation.ObservationId.ToString();
        processedOccurrence.OccurrenceId = $"urn:lsid:mvm:observation:{verbatimObservation.ObservationId}";
        processedOccurrence.ReportedDate = dates.InsertDate != null ? dates.InsertDate.Value.ToUniversalTime() : dates.SamplingDate!.Value.ToUniversalTime();
        processedOccurrence.RecordedBy = processedOccurrence.ReportedBy = "SLU, Institutionen för vatten och miljö"; // todo
        //processedOccurrence.ReportedBy = processedOccurrence.ReportedBy = "SLU, Institutionen för vatten och miljö"; // todo
        processedOccurrence.IsNaturalOccurrence = true;
        processedOccurrence.IsNeverFoundObservation = false;
        processedOccurrence.IsNotRediscoveredObservation = false;
        processedOccurrence.IsPositiveObservation = true;
        processedOccurrence.SensitivityCategory = CalculateProtectionLevel(taxon, accessRightsId);            
        return processedOccurrence;
    }       

    public void ValidateVerbatimData(DwcObservationVerbatim verbatim, DwcaValidationRemarksBuilder validationRemarksBuilder)
    {
        validationRemarksBuilder.NrValidatedObservations++;

        if (string.IsNullOrWhiteSpace(verbatim.CoordinateUncertaintyInMeters))
        {
            validationRemarksBuilder.NrMissingCoordinateUncertaintyInMeters++;
        }

        if (string.IsNullOrWhiteSpace(verbatim.IdentificationVerificationStatus))
        {
            validationRemarksBuilder.NrMissingIdentificationVerificationStatus++;
        }
    }      
}