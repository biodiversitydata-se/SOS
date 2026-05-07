using Microsoft.Extensions.Logging;
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
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using System.Text.RegularExpressions;
using Location = SOS.Lib.Models.Processed.Observation.Location;
using VocabularyValue = SOS.Lib.Models.Processed.Observation.VocabularyValue;

namespace SOS.Harvest.Processors.VirtualHerbarium;

/// <summary>
///     Factory for processing Virtual Herbarium observations from NDJSON format
/// </summary>
public class VirtualHerbariumObservationFactory : ObservationFactoryBase, IObservationFactory<VirtualHerbariumObservationVerbatim>
{
    private readonly IAreaHelper _areaHelper;
    private readonly ILogger _logger;
    private IDictionary<string, (double longitude, double latitude, int precision)>? _communities;
    private IDictionary<string, (double longitude, double latitude, int precision)>? _parishes;
    private IDictionary<string, (double longitude, double latitude, int? precision, string? locationRemarks)>? _matchLookup;
    private string _englishOrganizationName;
    private string _englishOrganizationNameLowerCase;
    private VocabularyValue? _institutionCodeVocabularyValue;

    // Geo statistics counters    
    private readonly Dictionary<int, int> _communityPrecisionDistribution = new();
    private readonly Dictionary<int, int> _parishPrecisionDistribution = new();

    private enum GeoCoordinateSource { NotFound, VerbatimCoordinates, DistrictCommunity, DistrictParish, LocationCommunity, LocationParish, MatchFile }

    private static string GetMatchDescription(GeoCoordinateSource source) => source switch
    {
        GeoCoordinateSource.VerbatimCoordinates => "Verbatim coordinates",
        GeoCoordinateSource.DistrictCommunity => "District=>Community",
        GeoCoordinateSource.DistrictParish => "District=>Parish",
        GeoCoordinateSource.LocationCommunity => "Location=>Community",
        GeoCoordinateSource.LocationParish => "Location=>Parish",
        GeoCoordinateSource.MatchFile => "MatchFile",
        _ => "No match"
    };

    private readonly List<Models.VirtualHerbariumGeoRow> _geoRows = new();

    // Regex for removing content within square brackets. E.g. "Råda [Härryda kommun]" -> "Råda"    
    private static readonly Regex BracketContentRegex = new Regex(@"\s*\[.*?\]\s*", RegexOptions.Compiled);

    // Regex for matching unicode escape sequences like \uXXXX
    private static readonly Regex UnicodeEscapeRegex = new Regex(@"\\u([0-9A-Fa-f]{4})", RegexOptions.Compiled);

    // Regex for normalizing whitespace (removing leading/trailing and replacing multiple spaces with single space)
    private static readonly Regex WhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="dataProvider"></param>
    /// <param name="taxa"></param>
    /// <param name="dwcaVocabularyById"></param>
    /// <param name="areaHelper"></param>
    /// <param name="processTimeManager"></param>
    /// <param name="processConfiguration"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public VirtualHerbariumObservationFactory(
        DataProvider dataProvider,
        IDictionary<int, Lib.Models.Processed.Observation.Taxon>? taxa,
        IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,
        IAreaHelper areaHelper,
        IProcessTimeManager processTimeManager,
        ProcessConfiguration processConfiguration,
        ILogger logger) : base(dataProvider, taxa, dwcaVocabularyById, processTimeManager, processConfiguration)
    {
        _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _englishOrganizationName = dataProvider?.Organizations?.Translate("en-GB")!;
        _englishOrganizationNameLowerCase = _englishOrganizationName?.ToLower()!;
        if (VocabularyById != null && VocabularyById.ContainsKey(VocabularyId.Institution))
        {
            _institutionCodeVocabularyValue = GetSosId(
                _englishOrganizationName,
                VocabularyById[VocabularyId.Institution],
                null,
                MappingNotFoundLogic.UseSourceValue,
                _englishOrganizationNameLowerCase);
        }
    }

    /// <summary>
    ///     Initialize async resources (e.g., community coordinates lookup)
    /// </summary>
    public async Task InitializeAsync()
    {
        _communities = await GetAreaLookupAsync(AreaType.Community);
        _parishes = await GetAreaLookupAsync(AreaType.Parish);
        _matchLookup = LoadMatchFileLookup();
    }

    /// <summary>
    ///     Create a processed observation from a verbatim observation
    /// </summary>
    /// <param name="verbatim">The verbatim observation from NDJSON</param>
    /// <param name="diffuseIfSupported">Whether to diffuse coordinates if supported</param>
    /// <returns>Processed observation</returns>
    public Observation CreateProcessedObservation(VirtualHerbariumObservationVerbatim verbatim, bool diffuseIfSupported)
    {
        if (verbatim == null)
        {
            return null!;
        }

        // Normalize taxon name by decoding unicode escape sequences
        var normalizedTaxaName = NormalizeTaxonName(verbatim.TaxaName);
        var normalizedFullScientificName = NormalizeTaxonName(GetFullScientificName(verbatim));
        var normalizedGenus = NormalizeTaxonName(verbatim.Genus);
        
        var taxonNames = new[] { normalizedTaxaName, normalizedFullScientificName, normalizedGenus }
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray();

        var taxon = GetTaxon(0, taxonNames, null, true);
        
        if (taxon == null && !string.IsNullOrWhiteSpace(verbatim.TaxaName))
        {
            _logger.LogDebug(
                "Taxon not found for verbatim observation {Id}. Original: '{Original}', Normalized: '{Normalized}'",
                verbatim.Id,
                verbatim.TaxaName,
                normalizedTaxaName);
        }
        
        var defects = new Dictionary<string, string>();
        var eventDates = GetStartAndEndDate(verbatim);
        
        if (eventDates.StartDate == null && eventDates.EndDate == null)
        {
            defects.Add("Datum", verbatim.Datum ?? string.Empty);
        }

        var obs = new Observation
        {
            DataProviderId = DataProvider.Id,
            MongoDbId = verbatim.Id,
            BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation },
            DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.VirtualHerbarium}",
            DatasetName = "Virtual Herbarium",
            Defects = defects.Count == 0 ? null : defects,
            DiffusionStatus = DiffusionStatus.NotDiffused,
            Event = new Event(eventDates.StartDate, null, eventDates.EndDate, null)
            {
                Habitat = verbatim.Habitat?.Clean()
            },
            Identification = new Identification
            {
                UncertainIdentification = false,
                Verified = true,
                VerificationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
            },
            Location = new Location(LocationType.Point)
            {
                Locality = GetLocality(verbatim),
                VerbatimLocality = verbatim.Location,
                Attributes = new LocationAttributes
                {
                    VerbatimProvince = verbatim.Province
                }
            },
            Occurrence = new Occurrence
            {
                BirdNestActivityId = taxon?.IsBird() ?? false ? 1000000 : 0,
                CatalogNumber = $"{verbatim.InstCode}-{verbatim.AccessionNo}-{taxon?.Id ?? 0}",
                OccurrenceId = $"urn:lsid:herbarium.emg.umu.se:observation:{verbatim.InstCode}*{verbatim.AccessionNo}*{taxon?.Id ?? 0}",
                IsNaturalOccurrence = true,
                IsNeverFoundObservation = false,
                IsNotRediscoveredObservation = false,
                IsPositiveObservation = true,
                OccurrenceStatus = new VocabularyValue { Id = (int)OccurrenceStatusId.Present },
                SensitivityCategory = CalculateProtectionLevel(taxon),
                RecordedBy = verbatim.Collector,
                OccurrenceRemarks = BuildOccurrenceRemarks(verbatim)
            },
            OwnerInstitutionCode = verbatim.InstCode,
            Taxon = taxon
        };

        // Try to get coordinates from verbatim, or fall back to community lookup
        double? latitude = null;
        double? longitude = null;
        int? coordinatePrecision = null;
        var geoSource = GeoCoordinateSource.NotFound;

        if (double.TryParse(verbatim.Wgs84_Lat, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedLat))
        {
            latitude = parsedLat;
        }
        if (double.TryParse(verbatim.Wgs84_Lng, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedLng))
        {
            longitude = parsedLng;
        }

        if (longitude.HasValue && latitude.HasValue && IsInSweden(longitude, latitude) == false)
        {
            latitude = null;
            longitude = null;
        }

        if (latitude.HasValue && latitude != 0 && longitude.HasValue && longitude != 0)
        {
            geoSource = GeoCoordinateSource.VerbatimCoordinates;
        }

        if ((latitude == null || latitude == 0 || longitude == null || longitude == 0) && 
            !string.IsNullOrEmpty(verbatim.District) && !string.IsNullOrEmpty(verbatim.Province))
        {
            // Remove bracket content from District, t.ex. "Råda [Härryda kommun]" -> "Råda"
            var cleanDistrict = RemoveBracketContent(verbatim.District);
            var lookupKey = $"{verbatim.Province}-{cleanDistrict}".ToLower();

            if (_communities?.TryGetValue(lookupKey, out var community) ?? false)
            {
                longitude = community.longitude;
                latitude = community.latitude;
                coordinatePrecision = community.precision;
                geoSource = GeoCoordinateSource.DistrictCommunity;
            }
            else if (_parishes?.TryGetValue(lookupKey, out var parish) ?? false)
            {
                longitude = parish.longitude;
                latitude = parish.latitude;
                coordinatePrecision = parish.precision;
                geoSource = GeoCoordinateSource.DistrictParish;
            }
            else if (!string.IsNullOrEmpty(verbatim.Location))
            {
                var cleanLocation = RemoveBracketContent(verbatim.Location);
                var locationLookupKey = $"{verbatim.Province}-{cleanLocation}".ToLower();

                if (_communities?.TryGetValue(locationLookupKey, out var community2) ?? false)
                {
                    longitude = community2.longitude;
                    latitude = community2.latitude;
                    coordinatePrecision = community2.precision;
                    geoSource = GeoCoordinateSource.LocationCommunity;
                }
                else if (_parishes?.TryGetValue(locationLookupKey, out var parish2) ?? false)
                {
                    longitude = parish2.longitude;
                    latitude = parish2.latitude;
                    coordinatePrecision = parish2.precision;
                    geoSource = GeoCoordinateSource.LocationParish;
                }
            }
        }
        
        if (geoSource == GeoCoordinateSource.NotFound && !string.IsNullOrEmpty(verbatim.Province))
        {
            var districtKey = !string.IsNullOrEmpty(verbatim.District)
                ? $"{verbatim.Province}-{RemoveBracketContent(verbatim.District)}".ToLower()
                : null;
            var locationKey = !string.IsNullOrEmpty(verbatim.Location)
                ? $"{verbatim.Province}-{RemoveBracketContent(verbatim.Location)}".ToLower()
                : null;

            (double longitude, double latitude, int? precision, string? locationRemarks) matchEntry = default;
            if (locationKey != null && (_matchLookup?.TryGetValue(locationKey, out matchEntry) ?? false) || 
                districtKey != null && (_matchLookup?.TryGetValue(districtKey, out matchEntry) ?? false))
            {
                longitude = matchEntry.longitude;
                latitude = matchEntry.latitude;
                coordinatePrecision = matchEntry.precision;
                obs.Location.LocationRemarks = matchEntry.locationRemarks;
                geoSource = GeoCoordinateSource.MatchFile;
            }
        }

        obs.AccessRights = GetAccessRightsFromSensitivityCategory(obs.Occurrence.SensitivityCategory);
        obs.InstitutionCode = _institutionCodeVocabularyValue;

        if (latitude.HasValue && longitude.HasValue && latitude != 0 && longitude != 0)
        {
            AddPositionData(obs.Location, longitude.Value, latitude.Value, CoordinateSys.WGS84, coordinatePrecision, taxon?.Attributes?.DisturbanceRadius);
        }

        _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

        // Update geo statistics       
        _geoRows.Add(new Models.VirtualHerbariumGeoRow
        {
            VerbatimProvince = verbatim.Province,
            VerbatimDistrict = verbatim.District,
            VerbatimLocation = verbatim.Location,
            Match = GetMatchDescription(geoSource),
            CoordinateUncertaintyInMeters = obs.Location?.CoordinateUncertaintyInMeters,
            Province = obs.Location?.Province?.Name,
            Municipality = obs.Location?.Municipality?.Name,
            Parish = obs.Location?.Parish?.Name
        });

        // Populate generic data
        PopulateGenericData(obs);
        CalculateOrganismQuantity(obs);

        return obs;
    }    

    /// <summary>
    ///     Returns per-observation geographic resolution rows for Excel export.
    /// </summary>
    public IReadOnlyList<Models.VirtualHerbariumGeoRow> GetGeoRows() => _geoRows;    

    private bool IsInSweden(double? longitude, double? latitude)
    {
        if (longitude == null || latitude == null) return false;        

        var lon = longitude.Value;
        var lat = latitude.Value;

        // Basic latitude check for Sweden (approximately 55°N to 69.1°N)
        if (lat < 55.0 || lat > 69.1)
        {
            return false;
        }

        // Detailed longitude ranges by latitude zones
        // Sweden is narrower in the south and wider in the north, but also shifts eastward

        // Zone 1: Skåne and southernmost Sweden (55-56°N)
        if (lat >= 55.0 && lat < 56.0)
        {
            return lon >= 12.5 && lon <= 14.5; // Approximately Skåne
        }
        
        // Zone 2: Småland, Öland, Gotland (56-58°N)
        if (lat >= 56.0 && lat < 58.0)
        {
            return lon >= 11.0 && lon <= 19.0; // Includes Gotland (18-19°E)
        }
        
        // Zone 3: Östergötland, Stockholm, Västernorrland (58-60°N)
        if (lat >= 58.0 && lat < 60.0)
        {
            return lon >= 11.5 && lon <= 19.5;
        }
        
        // Zone 4: Gävleborg, Jämtland (60-62°N)
        if (lat >= 60.0 && lat < 62.0)
        {
            return lon >= 12.0 && lon <= 20.0;
        }
        
        // Zone 5: Västerbotten, southern Norrbotten (62-64°N)
        if (lat >= 62.0 && lat < 64.0)
        {
            return lon >= 12.5 && lon <= 22.0;
        }
        
        // Zone 6: Mid Norrbotten (64-66°N)
        if (lat >= 64.0 && lat < 66.0)
        {
            return lon >= 13.0 && lon <= 23.0;
        }
        
        // Zone 7: Northern Norrbotten (66-68°N)
        if (lat >= 66.0 && lat < 68.0)
        {
            return lon >= 15.0 && lon <= 24.0;
        }
        
        // Zone 8: Northernmost Sweden (68-69.1°N)
        if (lat >= 68.0 && lat <= 69.1)
        {
            return lon >= 17.0 && lon <= 24.2; // Treriksröset area
        }

        return false;
    }

    /// <inheritdoc />
    public bool IsVerbatimObservationDiffusedByProvider(VirtualHerbariumObservationVerbatim verbatim)
    {
        return false;
    }

    /// <summary>
    ///     Build locality string from verbatim data
    /// </summary>
    private string GetLocality(VirtualHerbariumObservationVerbatim verbatim)
    {
        if (verbatim == null)
        {
            return null!;
        }

        // Prefer explicit location if available
        if (!string.IsNullOrEmpty(verbatim.Location))
        {
            return verbatim.Location;
        }

        // Build locality from available fields
        var parts = new List<string>();
        
        if (!string.IsNullOrEmpty(verbatim.Province))
        {
            parts.Add(verbatim.Province);
        }
        
        if (!string.IsNullOrEmpty(verbatim.District))
        {
            parts.Add(verbatim.District);
        }

        if (parts.Count > 0)
        {
            return string.Join(", ", parts);
        }

        return "Se kommentarsfält för mer exakt lokal.";
    }

    /// <summary>
    ///     Get full scientific name including infraspecific epithet if available
    /// </summary>
    private string? GetFullScientificName(VirtualHerbariumObservationVerbatim verbatim)
    {
        if (string.IsNullOrEmpty(verbatim.Genus))
        {
            return null;
        }

        var parts = new List<string> { verbatim.Genus };

        if (!string.IsNullOrEmpty(verbatim.Species))
        {
            parts.Add(verbatim.Species);
        }

        if (!string.IsNullOrEmpty(verbatim.InomArtsRang) && !string.IsNullOrEmpty(verbatim.InomArtsEpitet))
        {
            parts.Add(verbatim.InomArtsRang);
            parts.Add(verbatim.InomArtsEpitet);
        }

        return parts.Count > 0 ? string.Join(" ", parts) : null;
    }

    /// <summary>
    ///     Build occurrence remarks from verbatim data
    /// </summary>
    private string? BuildOccurrenceRemarks(VirtualHerbariumObservationVerbatim verbatim)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(verbatim.OriginalText))
        {
            parts.Add(verbatim.OriginalText.Clean()!);
        }

        if (!string.IsNullOrEmpty(verbatim.Notes))
        {
            parts.Add(verbatim.Notes.Clean()!);
        }

        if (!string.IsNullOrEmpty(verbatim.Comment))
        {
            parts.Add(verbatim.Comment.Clean()!);
        }

        return parts.Count > 0 ? string.Join(" ", parts) : null;
    }

    /// <summary>
    ///     Parse date from verbatim fields
    /// </summary>
    private (DateTime? StartDate, DateTime? EndDate) GetStartAndEndDate(VirtualHerbariumObservationVerbatim verbatim)
    {
        // First try to use the datum field if available
        if (!string.IsNullOrEmpty(verbatim.Datum))
        {
            return ParseDateString(verbatim.Datum);
        }

        // Fall back to individual year/month/day fields
        if (!int.TryParse(verbatim.Ar, out var year) || year < 1628 || year > DateTime.Today.Year)
        {
            return (null, null);
        }

        if (int.TryParse(verbatim.Manad, out var month) && month >= 1 && month <= 12)
        {
            if (int.TryParse(verbatim.Dag, out var day) && day >= 1 && day <= 31)
            {
                try
                {
                    var date = new DateTime(year, month, day);
                    return (date, date);
                }
                catch
                {
                    // Invalid date combination
                }
            }
            
            // Only year and month available
            var startDate = new DateTime(year, month, 1);
            var endDate = month == 12 
                ? new DateTime(year + 1, 1, 1).AddDays(-1) 
                : new DateTime(year, month + 1, 1).AddDays(-1);
            return (startDate, endDate);
        }

        // Only year available
        return (new DateTime(year, 1, 1), new DateTime(year, 12, 31));
    }

    /// <summary>
    ///     Parse a date string in format "YYYY-MM-DD", "YYYY-MM", or "YYYY"
    /// </summary>
    private (DateTime? StartDate, DateTime? EndDate) ParseDateString(string dateString)
    {
        if (string.IsNullOrEmpty(dateString))
        {
            return (null, null);
        }

        var parts = dateString.Replace("&", "6").Split('-');
        var firstPossibleYear = 1628;

        try
        {
            if (int.TryParse(parts[0], out var year) && year > firstPossibleYear && year <= DateTime.Today.Year)
            {
                return parts.Length switch
                {
                    3 when int.TryParse(parts[1], out var month) && int.TryParse(parts[2], out var day) 
                        => (new DateTime(year, month, day), new DateTime(year, month, day)),
                    2 when int.TryParse(parts[1], out var month2) 
                        => (new DateTime(year, month2, 1), 
                            month2 == 12 ? new DateTime(year + 1, 1, 1).AddDays(-1) : new DateTime(year, month2 + 1, 1).AddDays(-1)),
                    _ => (new DateTime(year, 1, 1), new DateTime(year, 12, 31))
                };
            }
        }
        catch
        {
            // Parsing failed
        }

        return (null, null);
    }

    /// <summary>
    ///     Load the VirtualHerbariumMatch.tsv file and build a lookup dictionary keyed by
    ///     "{Province}-{District}" and "{Province}-{Location}" (lower-case) for rows with Match == "Match".
    /// </summary>
    private static IDictionary<string, (double longitude, double latitude, int? precision, string? locationRemarks)>? LoadMatchFileLookup()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "VirtualHerbarium", "VirtualHerbariumMatch.tsv");
        if (!File.Exists(filePath))
        {
            return null;
        }

        var dic = new Dictionary<string, (double longitude, double latitude, int? precision, string? locationRemarks)>();
        using var reader = new StreamReader(filePath, System.Text.Encoding.UTF8);
        var header = reader.ReadLine();
        if (header == null) return dic;

        var headers = header.Split('\t');
        int idxProvince = Array.IndexOf(headers, "Province (verbatim)");
        int idxDistrict = Array.IndexOf(headers, "District (verbatim)");
        int idxLocation = Array.IndexOf(headers, "Location (verbatim)");
        int idxUncertainty = Array.IndexOf(headers, "CoordinateUncertaintyInMeters");
        int idxLat = Array.IndexOf(headers, "DecimalLatitude");
        int idxLon = Array.IndexOf(headers, "DecimalLongitude");
        int idxLocationRemarks = Array.IndexOf(headers, "LocationRemarks");
        int idxMatch = Array.IndexOf(headers, "Match");

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var cols = line.Split('\t');
            if (cols.Length <= idxMatch) continue;
            if (!"Match".Equals(cols[idxMatch].Trim(), StringComparison.OrdinalIgnoreCase)) continue;

            if (!double.TryParse(cols[idxLat], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat)) continue;
            if (!double.TryParse(cols[idxLon], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon)) continue;
            int.TryParse(cols[idxUncertainty], out var uncertaintyParsed);
            int? uncertainty = idxUncertainty >= 0 && idxUncertainty < cols.Length && !string.IsNullOrWhiteSpace(cols[idxUncertainty])
                ? uncertaintyParsed
                : null;

            var province = cols[idxProvince].Trim();
            var district = idxDistrict >= 0 && idxDistrict < cols.Length ? cols[idxDistrict].Trim() : string.Empty;
            var location = idxLocation >= 0 && idxLocation < cols.Length ? cols[idxLocation].Trim() : string.Empty;
            var locationRemarks = idxLocationRemarks >= 0 && idxLocationRemarks < cols.Length
                ? cols[idxLocationRemarks].Trim()
                : null;
            if (string.IsNullOrEmpty(locationRemarks)) locationRemarks = null;

            if (!string.IsNullOrEmpty(district))
            {
                var key = $"{province}-{district}".ToLower();
                dic.TryAdd(key, (lon, lat, uncertainty, locationRemarks));
            }

            if (!string.IsNullOrEmpty(location))
            {
                var key = $"{province}-{location}".ToLower();
                dic.TryAdd(key, (lon, lat, uncertainty, locationRemarks));
            }
        }

        return dic;
    }

    /// <summary>
    ///     Get area lookup dictionary for given area type
    /// </summary>
    private async Task<IDictionary<string, (double longitude, double latitude, int precision)>?> GetAreaLookupAsync(AreaType areaType)
    {
        var dic = new Dictionary<string, (double longitude, double latitude, int precision)>();
        IEnumerable<Lib.Models.Shared.Area>? areas = await _areaHelper.GetAreasAsync(areaType);

        if (!areas?.Any() ?? true)
        {
            return null;
        }

        var duplicates = new HashSet<string>();

        foreach (var area in areas!)
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
                    maxPoint = new NetTopologySuite.Geometries.Point(coordinate);
                }
            }

            var maxDistanceInM = 0;
            if (maxDistance > 0)
            {
                maxDistanceInM = (int)centroid
                    .Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM)
                    .Distance(maxPoint.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM));
            }

            // Get point county, parish etc
            var features = _areaHelper.GetPointFeatures(geometry.Centroid);
            if (!features?.Any() ?? true)
            {
                continue;
            }

            var provinceName = string.Empty;
            foreach (var feature in features!)
            {
                var attributes = feature.Attributes as NetTopologySuite.Features.AttributesTable;
                if (((AreaType)attributes!["areaType"]) == AreaType.Province)
                {
                    provinceName = attributes["name"]?.ToString();
                    break;
                }
            }

            if (!string.IsNullOrEmpty(provinceName))
            {
                var key = $"{provinceName.Trim()}-{area.Name.Trim()}".ToLower();

                // If combination of province and community exists more than once, we can't use it to determine position
                if (duplicates.Contains(key))
                {
                    continue;
                }

                if (dic.ContainsKey(key))
                {
                    dic.Remove(key);
                    duplicates.Add(key);
                    continue;
                }

                dic.Add(key, (centroid.X, centroid.Y, maxDistanceInM));
            }
        }

        return dic;
    }    

    /// <summary>
    ///     Normalizes a scientific name by replacing unicode escape sequences and cleaning whitespace
    ///     Example: "Protoparmelia\\u000b ochrococca" -> "Protoparmelia ochrococca"
    /// </summary>
    private static string? NormalizeTaxonName(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        // Replace unicode escape sequences and normalize whitespace
        var normalized = UnicodeEscapeRegex.Replace(input, match =>
        {
            var hexValue = match.Groups[1].Value;
            var charCode = Convert.ToInt32(hexValue, 16);
            var character = (char)charCode;

            // If it is a whitespace (tab, newline, vertical tab, etc) or control character, replace with space
            if (char.IsWhiteSpace(character) || char.IsControl(character))
            {
                return " ";
            }
            
            return character.ToString();
        });

        // Normalize all whitespace to single spaces and trim
        normalized = WhitespaceRegex.Replace(normalized, " ").Trim();

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    /// <summary>
    ///     Removes text within square brackets from a string
    ///     Example: "Råda [Härryda kommun]" -> "Råda"
    /// </summary>
    private static string RemoveBracketContent(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }
        
        return BracketContentRegex.Replace(input, string.Empty).Trim();
    }

    public IEnumerable<Observation>? CreateProcessedObservations(VirtualHerbariumObservationVerbatim verbatim, bool diffuseIfSupported)
    {
        var observation = CreateProcessedObservation(verbatim, diffuseIfSupported);
        return observation == null ? null : [observation];
    }
}

