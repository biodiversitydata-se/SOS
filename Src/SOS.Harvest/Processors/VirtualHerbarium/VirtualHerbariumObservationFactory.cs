using System.Text.RegularExpressions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Harvest.Processors.VirtualHerbarium
{
    public class VirtualHerbariumObservationFactory : ObservationFactoryBase, IObservationFactory<VirtualHerbariumObservationVerbatim>
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IDictionary<string, (double longitude, double latitude, int precision)> _communities;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processTimeManager"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public VirtualHerbariumObservationFactory(DataProvider dataProvider, 
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, 
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager) : base(dataProvider, taxa, processTimeManager)
        {
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _communities = new Dictionary<string, (double longitude, double latitude, int precision)>();
        }

        public async Task InitializeAsync()
        {
            var communities = await _areaHelper.GetAreasAsync(AreaType.Community);
            if (!communities?.Any() ?? true)
            {
                return;
            }
            var duplictes = new HashSet<string>();
            foreach (var community in communities)
            {
                var geometry = await _areaHelper.GetGeometryAsync(community.AreaType, community.FeatureId);
                if (geometry == null)
                {
                    continue;
                }

                var centroid = geometry.Centroid;
                var maxDistance = 0d;
                var maxPoint = centroid;
                // Find point most far away from centroid
                foreach (var coordinate in geometry.Coordinates)
                {
                    var distance = centroid.Coordinate.Distance(coordinate);

                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        maxPoint = new Point(coordinate);
                    }
                }
                var maxDistanceInM = 0;
                
                //If we have found a point, get distance in meters
                if (maxDistance > 0)
                {
                    maxDistanceInM = (int)centroid.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM).Distance(maxPoint.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM));
                }

                // Get point county, parish etc
                var features = _areaHelper.GetPointFeatures(geometry.Centroid);
                
                if (!features?.Any() ?? true)
                {
                    continue;
                }
                var proviceName = string.Empty;
                foreach(var feature in features)
                {
                    var attributes = feature.Attributes as AttributesTable;

                    if (((AreaType)attributes["areaType"]) == AreaType.Province)
                    {
                        proviceName = attributes["name"]?.ToString();
                        break;
                    }
                }
                
                if (!string.IsNullOrEmpty(proviceName))
                {
                    var key = $"{proviceName.Trim()}-{community.Name.Trim()}".ToLower();

                    // If combination of province and community exists more than once, we can't use it to determine position
                    if (duplictes.Contains(key))
                    {
                        continue;
                    }

                    // If combination of province and community exists more than once, we can't use it to determine position
                    if (_communities.ContainsKey(key))
                    {
                        _communities.Remove(key);
                        duplictes.Add(key);
                        continue;
                    }

                    _communities.Add(key, (centroid.X, centroid.Y, maxDistanceInM));
                }
            }
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
            var defects = new Dictionary<string, string>();
            DateTime? dateCollected = DwcParser.ParseDate(verbatim.DateCollected?.Replace("&", "6"));
            if (dateCollected == null)
            {
                defects.Add("DateCollected", verbatim.DateCollected);
            }

            var notes = verbatim.Notes;
            if (!string.IsNullOrEmpty(notes))
            {
                // Remove invalid charaters.
                notes = Regex.Replace(verbatim.Notes, @"[\x00-\x1F]", string.Empty);
                notes = Regex.Replace(notes, @"\s\&\s", " och "); // replace ' & ' with ' och '
            }

            var obs = new Observation
            {                
                DataProviderId = DataProvider.Id,
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
                    Locality = verbatim.Locality,
                    VerbatimLocality = verbatim.Locality
                },
                Occurrence = new Occurrence
                {
                    BirdNestActivityId = taxon?.IsBird() ?? false ? 1000000 : 0,
                    CatalogNumber = $"{verbatim.InstitutionCode}-{verbatim.AccessionNo}-{verbatim.DyntaxaId}",
                    OccurrenceId =  $"urn:lsid:herbarium.emg.umu.se:observation:{verbatim.InstitutionCode}*{verbatim.AccessionNo}*{verbatim.DyntaxaId}",
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = GetIsPositiveObservation(verbatim.DyntaxaId),
                    OccurrenceStatus = GetOccurrenceStatusId(verbatim.DyntaxaId),
                    ProtectionLevel = CalculateProtectionLevel(taxon),
                    SensitivityCategory = CalculateProtectionLevel(taxon),
                    RecordedBy = verbatim.Collector,
                    OccurrenceRemarks = notes.Clean()
                },
                OwnerInstitutionCode = verbatim.InstitutionCode,
                Taxon = taxon
            };

            if ((verbatim.DecimalLongitude == 0 || verbatim.DecimalLatitude == 0) && !string.IsNullOrEmpty(verbatim.District))
            {
                if (_communities.TryGetValue($"{verbatim.Province}-{verbatim.District}".ToLower(), out var parish))
                {
                    verbatim.DecimalLongitude = parish.longitude;
                    verbatim.DecimalLatitude = parish.latitude;
                    verbatim.CoordinatePrecision = parish.precision;
                }
            }

            obs.AccessRights = GetAccessRightsFromSensitivityCategory(obs.Occurrence.SensitivityCategory);
            AddPositionData(obs.Location, verbatim.DecimalLongitude, verbatim.DecimalLatitude, CoordinateSys.WGS84, verbatim.CoordinatePrecision, taxon?.Attributes?.DisturbanceRadius);

            _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

            // Populate generic data
            PopulateGenericData(obs);

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