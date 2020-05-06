using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.GeoJsonObjectModel;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Enum;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    /// Observation manager class
    /// </summary>
    public class ObservationManager : Interfaces.IObservationManager
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IFieldMappingManager _fieldMappingManager;
        private readonly IAreaManager _areaManager;
        private readonly ITaxonManager _taxonManager;
        private readonly ILogger<ObservationManager> _logger;
        
        private const int BiotaTaxonId = 0;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fieldMappingManager"></param>
        /// <param name="taxonManager"></param>
        /// <param name="areaManager"></param>
        /// <param name="logger"></param>
        public ObservationManager(
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingManager fieldMappingManager,
            IAreaManager areaManager,
            ITaxonManager taxonManager,
            ILogger<ObservationManager> logger)
        {
            _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _fieldMappingManager = fieldMappingManager ?? throw new ArgumentNullException(nameof(fieldMappingManager));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
            _areaManager = areaManager ?? throw new ArgumentNullException(nameof(areaManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy, SearchSortOrder sortOrder)
        {
            try
            {
                filter = await PrepareFilter(filter);

                var processedObservations = (await _processedObservationRepository.GetChunkAsync(filter, skip, take, sortBy, sortOrder));
                ProcessLocalizedFieldMappings(filter, processedObservations.Records);
                ProcessNonLocalizedFieldMappings(filter, processedObservations.Records);
                return processedObservations;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of observations");
                return null;
            }
        }

        private async Task<SearchFilter> PrepareFilter(SearchFilter filter)
        {
            var preparedFilter = filter.Clone();

            if (preparedFilter.IncludeUnderlyingTaxa && preparedFilter.TaxonIds != null && preparedFilter.TaxonIds.Any())
            {
                if (preparedFilter.TaxonIds.Contains(BiotaTaxonId)) // If Biota, then clear taxon filter
                {
                    preparedFilter.TaxonIds = new List<int>();
                }
                else
                {
                    preparedFilter.TaxonIds = _taxonManager.TaxonTree.GetUnderlyingTaxonIds(preparedFilter.TaxonIds, true);
                }
            }
            // handle the area ids search
            if(preparedFilter.AreaIds != null && preparedFilter.AreaIds.Any())
            {
                var area = await _areaManager.GetAreaInternalAsync(preparedFilter.AreaIds.First());
                if (area != null)
                {
                    //if we already have the info needed for the search we skip polygon searches
                    if (area.AreaType == AreaType.County ||
                        area.AreaType == AreaType.Municipality ||
                        area.AreaType == AreaType.Province)
                    {
                        if (area.AreaType == AreaType.County)
                        {
                            if (preparedFilter.CountyIds == null)
                            {
                                preparedFilter.CountyIds = new List<int>();
                            }
                            var list = preparedFilter.CountyIds.ToList();
                            list.Add(area.Id);
                            preparedFilter.CountyIds = list;
                        }
                        else if (area.AreaType == AreaType.Municipality)
                        {
                            if (preparedFilter.MunicipalityIds == null)
                            {
                                preparedFilter.MunicipalityIds = new List<int>();
                            }
                            var list = preparedFilter.MunicipalityIds.ToList();
                            list.Add(area.Id);
                            preparedFilter.MunicipalityIds = list;
                        }
                        else if (area.AreaType == AreaType.Province)
                        {
                            if (preparedFilter.ProvinceIds == null)
                            {
                                preparedFilter.ProvinceIds = new List<int>();
                            }
                            var list = preparedFilter.ProvinceIds.ToList();
                            list.Add(area.Id);
                            preparedFilter.ProvinceIds = list;
                        }
                    }
                    // we need to use the geometry filter
                    else
                    {
                        var geomList = new List<Lib.Models.Shared.GeometryGeoJson>();
                        if (preparedFilter.GeometryFilter == null) 
                        {                            
                            preparedFilter.GeometryFilter = new GeometryFilter();
                            preparedFilter.GeometryFilter.MaxDistanceFromPoint = 0;
                        }                                                                      

                        var geom = ((GeoJsonMultiPolygon<GeoJson2DGeographicCoordinates>)area.Geometry);
                        foreach (var polygon in geom.Coordinates.Polygons)
                        {
                            //create the polygon
                            var inputGeom = new GeometryGeoJson();
                            inputGeom.Type = "polygon";
                            inputGeom.Coordinates = new System.Collections.ArrayList();
                            var str = "[";
                            foreach (var coord in polygon.Exterior.Positions)
                            {
                                str += $"[{coord.Longitude.ToString(CultureInfo.InvariantCulture)}, {coord.Latitude.ToString(CultureInfo.InvariantCulture)}],";
                                
                            }
                            str = str.Substring(0, str.Length - 1);
                            inputGeom.Coordinates.Add(JsonDocument.Parse(str + "]").RootElement);
                            geomList.Add(inputGeom);

                            //add the holes
                            if (polygon.Holes != null && polygon.Holes.Count > 0)
                            {
                                foreach(var hole in polygon.Holes)
                                {
                                    var inputHoleGeom = new GeometryGeoJson();
                                    inputHoleGeom.Type = "holepolygon";
                                    inputHoleGeom.Coordinates = new System.Collections.ArrayList();
                                    str = "[";

                                    foreach (var coord in hole.Positions)
                                    {
                                        str += $"[{coord.Longitude.ToString(CultureInfo.InvariantCulture)}, {coord.Latitude.ToString(CultureInfo.InvariantCulture)}],";
                                    }
                                    str = str.Substring(0, str.Length - 1);
                                    inputHoleGeom.Coordinates.Add(JsonDocument.Parse(str + "]").RootElement);
                                    geomList.Add(inputHoleGeom);

                                }
                            }
                        }
                        //if we already have a geometry filter then we can just add the area polygons onto those
                        if(preparedFilter.GeometryFilter.Geometries != null)
                        {
                            var list = preparedFilter.GeometryFilter.Geometries.ToList();
                            list.AddRange(geomList);
                            preparedFilter.GeometryFilter.Geometries = list;
                        }
                        else 
                        {                            
                            preparedFilter.GeometryFilter.Geometries = geomList;
                        }
                    }
                }
            }

            return preparedFilter;
        }

        private void ProcessNonLocalizedFieldMappings(SearchFilter filter, IEnumerable<object> processedObservations)
        {
            if (!filter.TranslateFieldMappedValues) return;
           
            foreach (var observation in processedObservations)
            {
                if (observation is IDictionary<string, object> obs)
                {
                    ResolveFieldMappedValue(obs, FieldMappingFieldId.BasisOfRecord, nameof(ProcessedObservation.BasisOfRecord));
                    ResolveFieldMappedValue(obs, FieldMappingFieldId.Type, nameof(ProcessedObservation.Type));
                    ResolveFieldMappedValue(obs, FieldMappingFieldId.AccessRights, nameof(ProcessedObservation.AccessRights));
                    ResolveFieldMappedValue(obs, FieldMappingFieldId.Institution, nameof(ProcessedObservation.InstitutionId));

                    if (obs.TryGetValue(nameof(ProcessedObservation.Location), out object locationObject))
                    {
                        var locationDictionary = locationObject as IDictionary<string, object>;
                        ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.County, nameof(ProcessedObservation.Location.County));
                        ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Municipality, nameof(ProcessedObservation.Location.Municipality));
                        ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Province, nameof(ProcessedObservation.Location.Province));
                        ResolveFieldMappedValue(locationDictionary, FieldMappingFieldId.Parish, nameof(ProcessedObservation.Location.Parish));
                    }

                    if (obs.TryGetValue(nameof(ProcessedObservation.Occurrence), out object occurrenceObject))
                    {
                        var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                        ResolveFieldMappedValue(occurrenceDictionary, FieldMappingFieldId.EstablishmentMeans, nameof(ProcessedObservation.Occurrence.EstablishmentMeans));
                        ResolveFieldMappedValue(occurrenceDictionary, FieldMappingFieldId.OccurrenceStatus, nameof(ProcessedObservation.Occurrence.OccurrenceStatus));
                    }
                }
            }
        }

        private void ProcessLocalizedFieldMappings(SearchFilter filter, IEnumerable<dynamic> processedObservations)
        {
            if (!filter.TranslateFieldMappedValues) return;
            string cultureCode = filter.TranslationCultureCode;           
            ProcessLocalizedFieldMappedReturnValues(processedObservations, cultureCode);            
        }

        private void ProcessLocalizedFieldMappedReturnValues(
            IEnumerable<ProcessedObservation> processedObservations,
            string cultureCode)
        {
            foreach (var observation in processedObservations)
            {
                TranslateLocalizedValue(observation.Occurrence?.Activity, FieldMappingFieldId.Activity, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.Gender, FieldMappingFieldId.Gender, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.LifeStage, FieldMappingFieldId.LifeStage, cultureCode);
                TranslateLocalizedValue(observation.Occurrence?.OrganismQuantityUnit, FieldMappingFieldId.Unit, cultureCode);
                TranslateLocalizedValue(observation.Event?.Biotope, FieldMappingFieldId.Biotope, cultureCode);
                TranslateLocalizedValue(observation.Event?.Substrate, FieldMappingFieldId.Substrate, cultureCode);
                TranslateLocalizedValue(observation.Identification?.ValidationStatus, FieldMappingFieldId.ValidationStatus, cultureCode);
            }
        }


        private void ProcessLocalizedFieldMappedReturnValues(
            IEnumerable<dynamic> processedObservations,
            string cultureCode)
        {
            try
            {
                foreach (var observation in processedObservations)
                {
                    if (observation is IDictionary<string, object> obs)
                    {
                        if (obs.TryGetValue(nameof(ProcessedObservation.Occurrence).ToLower(), out object occurrenceObject))
                        {
                            var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Activity, nameof(ProcessedObservation.Occurrence.Activity), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Gender, nameof(ProcessedObservation.Occurrence.Gender), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.LifeStage, nameof(ProcessedObservation.Occurrence.LifeStage), cultureCode);
                            TranslateLocalizedValue(occurrenceDictionary, FieldMappingFieldId.Unit, nameof(ProcessedObservation.Occurrence.OrganismQuantityUnit), cultureCode);
                        }

                        if (obs.TryGetValue(nameof(ProcessedObservation.Event).ToLower(), out object eventObject))
                        {
                            var eventDictionary = eventObject as IDictionary<string, object>;
                            TranslateLocalizedValue(eventDictionary, FieldMappingFieldId.Biotope, nameof(ProcessedObservation.Event.Biotope), cultureCode);
                            TranslateLocalizedValue(eventDictionary, FieldMappingFieldId.Substrate, nameof(ProcessedObservation.Event.Substrate), cultureCode);
                        }

                        if (obs.TryGetValue(nameof(ProcessedObservation.Identification).ToLower(), out object identificationObject))
                        {
                            var identificationDictionary = identificationObject as IDictionary<string, object>;
                            TranslateLocalizedValue(identificationDictionary, FieldMappingFieldId.ValidationStatus, nameof(ProcessedObservation.Identification.ValidationStatus), cultureCode);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void ResolveFieldMappedValue(
            IDictionary<string, object> observationNode, 
            FieldMappingFieldId fieldMappingFieldId, 
            string fieldName)
        {
            if (observationNode == null) return;

            if (observationNode.ContainsKey(fieldName))
            {
                if (observationNode[fieldName] is IDictionary<string, object> fieldNode && fieldNode.ContainsKey("Value") && fieldNode.ContainsKey("_id"))
                {
                    int id = (int)fieldNode["_id"];
                    if (id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId && _fieldMappingManager.TryGetValue(fieldMappingFieldId, id, out var translatedValue))
                    {
                        fieldNode["Value"] = translatedValue;
                    }
                }
            }
        }
        private void ResolveFieldMappedValue(ProcessedFieldMapValue fieldMapValue, FieldMappingFieldId fieldMappingFieldId)
        {
            if (fieldMapValue == null) return;
            if (fieldMapValue.Id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId
                && _fieldMappingManager.TryGetValue(fieldMappingFieldId, fieldMapValue.Id, out var translatedValue))
            {
                fieldMapValue.Value = translatedValue;
            }
        }

        private void TranslateLocalizedValue(
            ProcessedFieldMapValue fieldMapValue,
            FieldMappingFieldId fieldMappingFieldId,
            string cultureCode)
        {
            if (fieldMapValue == null) return;

            if (fieldMapValue.Id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId
                && _fieldMappingManager.TryGetTranslatedValue(fieldMappingFieldId, cultureCode, fieldMapValue.Id, out var translatedValue))
            {
                fieldMapValue.Value = translatedValue;
            }
        }

        private void TranslateLocalizedValue(
            IDictionary<string, object> observationNode, 
            FieldMappingFieldId fieldMappingFieldId, 
            string fieldName,
            string cultureCode)
        {
            if (observationNode == null) return;
            var lowerCaseName = Char.ToLower(fieldName[0]) + fieldName.Substring(1);
            if (observationNode.ContainsKey(lowerCaseName))
            {
                if (observationNode[lowerCaseName] is IDictionary<string, object> fieldNode && fieldNode.ContainsKey("id"))
                {
                    Int64 id = (Int64)fieldNode["id"];
                    if (id != FieldMappingConstants.NoMappingFoundCustomValueIsUsedId && _fieldMappingManager.TryGetTranslatedValue(fieldMappingFieldId, cultureCode, (int)id, out var translatedValue))
                    {
                        fieldNode["Value"] = translatedValue;
                    }
                }
            }
        }
    }
}