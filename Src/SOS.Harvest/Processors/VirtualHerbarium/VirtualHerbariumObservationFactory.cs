using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;
using Location = SOS.Lib.Models.Processed.Observation.Location;
using SOS.Lib.Configuration.Process;

namespace SOS.Harvest.Processors.VirtualHerbarium
{
    public class VirtualHerbariumObservationFactory : ObservationFactoryBase, IObservationFactory<VirtualHerbariumObservationVerbatim>
    {
        private readonly IAreaHelper _areaHelper;
        private IDictionary<string, (double longitude, double latitude, int precision)> _communities;
        private IDictionary<string, (double longitude, double latitude, int precision)> _parishes;

        private string GetLocality(VirtualHerbariumObservationVerbatim verbatim)
        {
            if (verbatim == null)
            {
                return null!;
            }

            // 1. Locality has value
            if (!string.IsNullOrEmpty(verbatim.Locality))
            {
                return verbatim.Locality;
            }

            // 2. CoordinateSource = "District" and District != ""
            // 3. District != "" och koordinater saknas men har slagits upp med hjälp av Excel-fil för distrikt. Detta villkor gäller oberoende av värdet på CoordinateSource
            if ((verbatim.CoordinateSource?.ToLower() == "district" || verbatim.CoordinateOverrideDistrict) && !string.IsNullOrEmpty(verbatim.District))
            {
                return $"Sockenkoordinat för {verbatim.District} sn. Se kommentarsfält för mer exakt lokal";
            }

            if (!string.IsNullOrEmpty(verbatim.CoordinateValue))
            {
                // 4. CoordinateSource = "LocalityVH" and CoordinateValue != ""
                if (verbatim.CoordinateSource?.ToLower() == "localityvh")
                {
                    return verbatim.CoordinateValue;
                }

                // 5. CoordinateValue != "" och coordinateSource != "None" och coordinateSource != ""
                if (verbatim.CoordinateSource?.ToLower() != "none" && !string.IsNullOrEmpty(verbatim.CoordinateSource))
                {
                    return $"{(string.IsNullOrEmpty(verbatim.Province) ? $"{verbatim.Province}, " : string.Empty)}{verbatim.CoordinateSource}, {verbatim.CoordinateValue}, Se kommentarsfält för mer exakt lokal.";
                }
            }

            // Fallback?
            return $"{(string.IsNullOrEmpty(verbatim.Province) ? string.Empty : $"{verbatim.Province}, ")}{(string.IsNullOrEmpty(verbatim.CoordinateSource) || verbatim.CoordinateSource.ToLower() == "none" ? string.Empty : $"{verbatim.CoordinateSource}, ")}Se kommentarsfält för mer exakt lokal.";
        }

        private (DateTime? StartDate, DateTime? EndDate) GetStartAndEndDate(string datecollected)
        {
            if (string.IsNullOrEmpty(datecollected))
            {
                return (null, null);
            }

            var dateArray = datecollected.Replace("&", "6").Split('-');
            var firstPossibleYear = 1628;

            try
            {
               
                int month;
                int day;
                if (int.TryParse(dateArray[0], out var year) && year > firstPossibleYear && year <= DateTime.Today.Year)
                {
                    return dateArray.Length switch
                    {
                        3 => int.TryParse(dateArray[1], out month) && int.TryParse(dateArray[2], out day) ? (new DateTime(year, month, day), new DateTime(year, month, day)) : (null, null),
                        2 => int.TryParse(dateArray[1], out month) ? (new DateTime(year, month, 1), month == 12 ? new DateTime(year + 1, 1, 1).AddDays(-1) : new DateTime(year, month + 1, 1).AddDays(-1)) : (null, null),
                        _ => (new DateTime(year, 1, 1), new DateTime(year, 12, 31))
                    };
                }
                
            }
            catch (Exception)
            {}

            return (null, null);
        }

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
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration) : base(dataProvider, taxa, processTimeManager, processConfiguration)
        {
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));            
        }

        public async Task InitializeAsync()
        {
            _communities = await GetCommunitiesAsync();
            _parishes = await GetAreaCentroidsAsync(AreaType.Parish);
        }

        private async Task<IDictionary<string, (double longitude, double latitude, int precision)>> GetCommunitiesAsync()
        {
            var dic = new Dictionary<string, (double longitude, double latitude, int precision)>();
            IEnumerable<Lib.Models.Shared.Area>? communities = await _areaHelper.GetAreasAsync(AreaType.Community);
            if (!communities?.Any() ?? true)
            {
                return null;
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
                foreach (var feature in features)
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
                    if (dic.ContainsKey(key))
                    {
                        dic.Remove(key);
                        duplictes.Add(key);
                        continue;
                    }

                    dic.Add(key, (centroid.X, centroid.Y, maxDistanceInM));
                }
            }

            return dic;
        }


        private async Task<IDictionary<string, (double longitude, double latitude, int precision)>> GetAreaCentroidsAsync(AreaType areaType)
        {
            IEnumerable<Lib.Models.Shared.Area>? areas = await _areaHelper.GetAreasAsync(areaType);
            var dic = new Dictionary<string, (double longitude, double latitude, int precision)>();
            if (!areas?.Any() ?? true)
            {
                return null;
            }
            var duplicates = new HashSet<string>();
            foreach (var area in areas)
            {
                var geometry = await _areaHelper.GetGeometryAsync(area.AreaType, area.FeatureId);
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
                
                string key = area.Name.Trim().ToLower();
                if (!dic.TryAdd(key, (centroid.X, centroid.Y, maxDistanceInM)))
                {
                    duplicates.Add(key);
                }                    
            }

            foreach(var key in duplicates)
            {
                dic.Remove(key);
            }

            return dic;
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

            var taxon = GetTaxon(verbatim.DyntaxaId, new[] { verbatim.ScientificName }, true);
            var defects = new Dictionary<string, string>();
            var eventDates = GetStartAndEndDate(verbatim.DateCollected);
            if (eventDates.StartDate == null && eventDates.EndDate == null)
            {
                defects.Add("DateCollected", verbatim.DateCollected);
            }

            var obs = new Observation
            {                
                DataProviderId = DataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation},
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.VirtualHerbarium}",
                DatasetName = "Virtual Herbarium",
                Defects = defects.Count == 0 ? null : defects,
                DiffusionStatus = DiffusionStatus.NotDiffused,
                Event = new Event(eventDates.StartDate, null, eventDates.EndDate, null),
                Identification = new Identification
                {
                    UncertainIdentification = false,
                    Verified = false,
                    VerificationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                Location = new Location
                {
                    Locality = GetLocality(verbatim),
                    VerbatimLocality = verbatim.Locality
                },
                Occurrence = new Occurrence
                {
                    BirdNestActivityId = taxon?.IsBird() ?? false ? 1000000 : 0,
                    CatalogNumber = $"{verbatim.InstitutionCode}-{verbatim.AccessionNo}-{taxon?.Id ?? verbatim.DyntaxaId}",
                    OccurrenceId =  $"urn:lsid:herbarium.emg.umu.se:observation:{verbatim.InstitutionCode}*{verbatim.AccessionNo}*{taxon?.Id ?? verbatim.DyntaxaId}",
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = GetIsPositiveObservation(verbatim.DyntaxaId),
                    OccurrenceStatus = GetOccurrenceStatusId(verbatim.DyntaxaId),
                    //ProtectionLevel = CalculateProtectionLevel(taxon),
                    SensitivityCategory = CalculateProtectionLevel(taxon),
                    RecordedBy = verbatim.Collector,
                    OccurrenceRemarks = string.IsNullOrEmpty(verbatim.OriginalText) ? verbatim.Notes.Clean() : $"{verbatim.OriginalText} {verbatim.Notes}".Clean()
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
                else if (_parishes.TryGetValue(verbatim.District.ToLower(), out var par))
                {
                    verbatim.DecimalLongitude = par.longitude;
                    verbatim.DecimalLatitude = par.latitude;
                    verbatim.CoordinatePrecision = par.precision;
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