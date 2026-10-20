using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Skogsskada;

namespace SOS.Harvest.Processors.Skogsskada;

/// <summary>
/// Skogsskada observation factory
/// </summary>
public class SkogsskadaObservationFactory : ObservationFactoryBase, IObservationFactory<SkogsskadaObservationVerbatim>
{
    private readonly IAreaHelper _areaHelper;
    private VocabularyValue? _institutionCodeVocabularyValue;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dataProvider"></param>
    /// <param name="taxa"></param>
    /// <param name="dwcaVocabularyById"></param>
    /// <param name="areaHelper"></param>
    /// <param name="processTimeManager"></param>
    /// <param name="processConfiguration"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SkogsskadaObservationFactory(DataProvider dataProvider,
        IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
        IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,
        IAreaHelper areaHelper,
        IProcessTimeManager processTimeManager,
        ProcessConfiguration processConfiguration) : base(dataProvider, taxa, dwcaVocabularyById, processTimeManager, processConfiguration)
    {
        _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));

        var englishOrganizationName = dataProvider?.Organizations?.Translate("en-GB");
        if (VocabularyById != null && VocabularyById.ContainsKey(VocabularyId.Institution) && !string.IsNullOrEmpty(englishOrganizationName))
        {
            _institutionCodeVocabularyValue = GetSosId(englishOrganizationName,
                VocabularyById[VocabularyId.Institution],
                null,
                MappingNotFoundLogic.UseSourceValue,
                englishOrganizationName.ToLower());
        }
    }

    /// <inheritdoc />
    public IEnumerable<Observation>? CreateProcessedObservations(SkogsskadaObservationVerbatim verbatim, bool diffuseIfSupported)
    {
        var observation = CreateProcessedObservation(verbatim, diffuseIfSupported);
        return observation == null ? null : [observation];
    }

    /// <summary>
    /// Cast verbatim observations to processed data model
    /// </summary>
    /// <param name="verbatim"></param>
    /// <param name="diffuseIfSupported"></param>
    /// <returns></returns>
    public Observation CreateProcessedObservation(SkogsskadaObservationVerbatim verbatim, bool diffuseIfSupported)
    {
        var taxon = GetTaxon(-1, new[] { verbatim.scientificName ?? string.Empty });
        var obs = new Observation
        {
            MongoDbId = verbatim.Id,
            AccessRights = GetSosId(verbatim.accessRights,
                VocabularyById[VocabularyId.AccessRights],
                (int)AccessRightsId.NotForPublicUsage),
            DataProviderId = DataProvider.Id,
            BasisOfRecord = GetSosId(verbatim.basisOfRecord,
                VocabularyById[VocabularyId.BasisOfRecord],
                (int)BasisOfRecordId.HumanObservation),
            DatasetId = $"urn:slu:dataprovider:{DataProviderIdentifiers.Skogsskada}",
            DatasetName = "Skogsskada",
            DiffusionStatus = DiffusionStatus.NotDiffused,
            Event = new Event(verbatim.eventDate, null, verbatim.eventDate, null)
            {
                SamplingProtocol = verbatim.samplingProtocol
            },
            Identification = new Identification
            {
                UncertainIdentification = false,
                Verified = false,
                VerificationStatus = GetSosId(verbatim.identificationVerificationStatus,
                    VocabularyById[VocabularyId.VerificationStatus],
                    (int)ValidationStatusId.Unvalidated)
            },
            Location = new Location(LocationType.Point)
            {
                VerbatimCoordinates = verbatim.verbatimCoordinates,
                VerbatimCoordinateSystem = verbatim.verbatimSRS
            },
            Occurrence = new Occurrence
            {
                BirdNestActivityId = 0,
                IsNaturalOccurrence = true,
                IsNeverFoundObservation = false,
                IsNotRediscoveredObservation = false,
                IsPositiveObservation = true,
                OccurrenceId = verbatim.occurrenceID,
                OccurrenceRemarks = verbatim.occurrenceRemarks,
                OccurrenceStatus = new VocabularyValue { Id = (int)OccurrenceStatusId.Present },
                RecordedBy = verbatim.recordedBy,
                SensitivityCategory = 1,
                CatalogNumber = verbatim.occurrenceID
            },
            OwnerInstitutionCode = verbatim.ownerInstitutionCode,
            RightsHolder = verbatim.rightsHolder,
            Taxon = taxon
        };

        obs.InstitutionCode = !string.IsNullOrEmpty(verbatim.institutionCode)
            ? GetSosId(verbatim.institutionCode, VocabularyById[VocabularyId.Institution])
            : _institutionCodeVocabularyValue;

        AddPositionData(obs.Location, verbatim.decimalLongitude, verbatim.decimalLatitude, CoordinateSys.WGS84,
            null, taxon?.Attributes?.DisturbanceRadius);

        if (!string.IsNullOrWhiteSpace(verbatim.country))
        {
            obs.Location.Country = GetSosId(verbatim.country, VocabularyById[VocabularyId.Country]);
        }
        if (!string.IsNullOrWhiteSpace(verbatim.countryCode))
        {
            obs.Location.CountryCode = verbatim.countryCode;
        }

        _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

        // Populate generic data
        PopulateGenericData(obs);
        CalculateOrganismQuantity(obs);
        return obs;
    }

    public bool IsVerbatimObservationDiffusedByProvider(SkogsskadaObservationVerbatim verbatim)
    {
        return false;
    }
}