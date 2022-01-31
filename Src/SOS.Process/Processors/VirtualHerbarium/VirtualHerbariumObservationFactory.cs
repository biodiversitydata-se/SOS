using System;
using System.Collections.Generic;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Process.Processors.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Process.Processors.VirtualHerbarium
{
    public class VirtualHerbariumObservationFactory : ObservationFactoryBase, IObservationFactory<VirtualHerbariumObservationVerbatim>
    {
        private readonly DataProvider _dataProvider;
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="areaHelper"></param>
        public VirtualHerbariumObservationFactory(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, IAreaHelper areaHelper) : base(taxa)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <summary>
        /// Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="diffuseIfSupported"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(VirtualHerbariumObservationVerbatim verbatim, bool diffuseIfSupported)
        {
            if (verbatim == null)
            {
                return null;
            }

            var taxon = GetTaxon(verbatim.DyntaxaId);
            var accessRights = new VocabularyValue { Id = (int)AccessRightsId.FreeUsage };

            var defects = new Dictionary<string, string>();
            DateTime? dateCollected = null;
            if (DateTime.TryParse(verbatim.DateCollected, out var date))
            {
                dateCollected = date;
            }
            else // In correct date, add it to defects
            {
                defects.Add("DateCollected", verbatim.DateCollected);
            }

            var obs = new Observation
            {
                AccessRights = accessRights,
                DataProviderId = _dataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation},
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.VirtualHerbarium}",
                DatasetName = "Virtual Herbarium",
                Defects = defects.Count == 0 ? null : defects,
                DiffusionStatus = DiffusionStatus.NotDiffused,
                Event = new Event
                {
                    EndDate = dateCollected?.ToUniversalTime(),
                    StartDate = dateCollected?.ToUniversalTime(),
                    PlainStartDate = dateCollected?.ToLocalTime().ToString("yyyy-MM-dd"),
                    PlainEndDate = dateCollected?.ToLocalTime().ToString("yyyy-MM-dd"),
                    PlainStartTime = null,
                    PlainEndTime = null,
                    VerbatimEventDate = DwcFormatter.CreateDateString(dateCollected?.ToLocalTime())
                },
                Identification = new Identification
                {
                    UncertainIdentification = false,
                    Validated = false,
                    Verified = false,
                    ValidationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert },
                    VerificationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                Location = new Location
                {
                    Locality = verbatim.Locality
                },
                Occurrence = new Occurrence
                {
                    BirdNestActivityId = taxon?.IsBird() ?? false ? 1000000 : 0,
                    CatalogNumber = $"{verbatim.InstitutionCode}#{verbatim.AccessionNo}#{verbatim.DyntaxaId}",
                    OccurrenceId =  $"urn:lsid:herbarium.emg.umu.se:Sighting:{verbatim.InstitutionCode}#{verbatim.AccessionNo}#{verbatim.DyntaxaId}",
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = GetIsPositiveObservation(verbatim.DyntaxaId),
                    OccurrenceStatus = GetOccurrenceStatusId(verbatim.DyntaxaId),
                    ProtectionLevel = CalculateProtectionLevel(taxon, (AccessRightsId)accessRights.Id),
                    SensitivityCategory = CalculateProtectionLevel(taxon, (AccessRightsId)accessRights.Id),
                    RecordedBy = verbatim.Collector,
                    OccurrenceRemarks = verbatim.Notes
                },
                OwnerInstitutionCode = verbatim.InstitutionCode,
                Taxon = taxon
            };

            AddPositionData(obs.Location, verbatim.DecimalLongitude, verbatim.DecimalLatitude, CoordinateSys.WGS84, verbatim.CoordinatePrecision, taxon?.Attributes?.DisturbanceRadius);

            _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

            return obs;
        }

        /// <summary>
        ///     Gets the occurrence status. Set to Present if DyntaxaTaxonId from provider is greater than 0 and Absent if
        ///     DyntaxaTaxonId is 0
        /// </summary>
        private VocabularyValue GetOccurrenceStatusId(int dyntaxaTaxonId)
        {
            if (dyntaxaTaxonId == 0)
            {
                return new VocabularyValue {Id = (int) OccurrenceStatusId.Absent};
            }

            return new VocabularyValue {Id = (int) OccurrenceStatusId.Present};
        }

        /// <summary>
        ///     Set to False if DyntaxaTaxonId from provider is greater than 0 and True if DyntaxaTaxonId is 0.
        /// </summary>
        private bool GetIsNeverFoundObservation(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId == 0;
        }

        /// <summary>
        ///     Set to True if DyntaxaTaxonId from provider is greater than 0 and False if DyntaxaTaxonId is 0.
        /// </summary>
        private bool GetIsPositiveObservation(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId != 0;
        }
    }
}