using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SOS.Lib.Helpers
{
    /// <summary>
    ///     Class that can be used for resolve generalized values.
    /// </summary>
    public class GeneralizationResolver : Interfaces.IGeneralizationResolver
    {        
        private readonly ILogger<GeneralizationResolver> _logger;
        private readonly IFilterManager _filterManager;
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;

        public GeneralizationResolver()
        {

        }

        public GeneralizationResolver(ILogger<GeneralizationResolver> logger, IFilterManager filterManager, IProcessedObservationCoreRepository processedObservationRepository)
        {
            _logger = logger;
            _filterManager = filterManager;
            _processedObservationRepository = processedObservationRepository;
        }

        public async Task ResolveGeneralizedObservationsAsync(SearchFilter filter, IEnumerable<JsonNode> observations)
        {            
            try
            {
                if (!(filter.ExtendedAuthorization.ProtectionFilter == ProtectionFilter.BothPublicAndSensitive && filter.ExtendedAuthorization != null && filter.ExtendedAuthorization.UserId != 0))
                {
                    return;
                }

                List<JsonNode> generalizedObservations = GetGeneralizedObservations(observations);
                var generalizedOccurrenceIds = GetOccurrenceIds(generalizedObservations);
                var protectedFilter = filter.Clone();
                protectedFilter.ExtendedAuthorization.ProtectionFilter = ProtectionFilter.Sensitive;
                protectedFilter.IncludeSensitiveGeneralizedObservations = true;
                protectedFilter.IsPublicGeneralizedObservation = false;
                protectedFilter.OccurrenceIds = generalizedOccurrenceIds;
                if (protectedFilter.Output.Fields != null)
                {
                    if (!protectedFilter.Output.Fields.Contains("Occurrence.OccurrenceId"))
                    {
                        protectedFilter.Output.Fields.Add("Occurrence.OccurrenceId");
                    }
                    if (!protectedFilter.Output.Fields.Contains("IsGeneralized"))
                    {
                        protectedFilter.Output.Fields.Add("IsGeneralized");
                    }
                }

                await _filterManager.PrepareFilterAsync(protectedFilter.RoleId, protectedFilter.AuthorizationApplicationIdentifier, protectedFilter);
                var dynamicSensitiveObservationsResult = await _processedObservationRepository.GetChunkAsync(protectedFilter, 0, 1000);

                var sensitiveObservations = dynamicSensitiveObservationsResult.Records.Cast<JsonNode>().ToList();
                UpdateGeneralizedWithRealValues(generalizedObservations, sensitiveObservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when resolving generalized observations");
            }
        }

        public async Task ResolveGeneralizedObservationsAsync(SearchFilter filter, IEnumerable<Observation> observations)
        {
            try
            {
                if (!(filter.ExtendedAuthorization.ProtectionFilter == ProtectionFilter.BothPublicAndSensitive && filter.ExtendedAuthorization != null && filter.ExtendedAuthorization.UserId != 0))
                {
                    return;
                }

                List<Observation> generalizedObservations = observations
                                                               .Where(obs => obs != null && obs.IsGeneralized)
                                                               .ToList();
                var generalizedOccurrenceIds = generalizedObservations.Select(obs => obs.Occurrence.OccurrenceId).ToList();
                var protectedFilter = filter.Clone();
                protectedFilter.ExtendedAuthorization.ProtectionFilter = ProtectionFilter.Sensitive;
                protectedFilter.IncludeSensitiveGeneralizedObservations = true;
                protectedFilter.IsPublicGeneralizedObservation = false;
                protectedFilter.OccurrenceIds = generalizedOccurrenceIds;
                if (protectedFilter.Output.Fields != null)
                {
                    if (!protectedFilter.Output.Fields.Contains("Occurrence.OccurrenceId"))
                    {
                        protectedFilter.Output.Fields.Add("Occurrence.OccurrenceId");
                    }
                    if (!protectedFilter.Output.Fields.Contains("IsGeneralized"))
                    {
                        protectedFilter.Output.Fields.Add("IsGeneralized");
                    }
                }

                await _filterManager.PrepareFilterAsync(filter.RoleId, filter.AuthorizationApplicationIdentifier, protectedFilter);
                var dynamicSensitiveObservationsResult = await _processedObservationRepository.GetChunkAsync(protectedFilter, 0, 1000);

                var sensitiveObservations = CastDynamicsToObservations(dynamicSensitiveObservationsResult.Records);
                UpdateGeneralizedWithRealValues(generalizedObservations, sensitiveObservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when resolving generalized observations");
            }
        }

        private void UpdateGeneralizedWithRealValues(IEnumerable<JsonNode> observations, IEnumerable<JsonNode> realObservations)
        {
            var obsByOccurrenceId = GetObservationsByOccurrenceIdDictionary(observations);
            var realObsByOccurrenceId = GetObservationsByOccurrenceIdDictionary(realObservations);
            int nrUpdated = 0;
            foreach (var kvp in realObsByOccurrenceId)
            {
                if (obsByOccurrenceId.TryGetValue(kvp.Key, out var observation))
                {
                    UpdateGeneralizedWithRealValues(observation, kvp.Value);
                    nrUpdated++;
                }
            }

            if (nrUpdated > 0)
            {
                _logger.LogInformation($"{nrUpdated} generalized observations were updated with real coordinates.");
            }
        }       

        private void UpdateGeneralizedWithRealValues(List<Observation> observations, List<Observation> realObservations)
        {
            var obsByOccurrenceId = observations.ToDictionary(m => m.Occurrence.OccurrenceId, m => m);
            var realObsByOccurrenceId = realObservations.ToDictionary(m => m.Occurrence.OccurrenceId, m => m);
            int nrUpdated = 0;
            foreach (var kvp in realObsByOccurrenceId)
            {
                if (obsByOccurrenceId.TryGetValue(kvp.Key, out var observation))
                {
                    UpdateGeneralizedWithRealValues(observation, kvp.Value);
                    nrUpdated++;
                }
            }

            if (nrUpdated > 0)
            {
                _logger.LogInformation($"{nrUpdated} generalized observations were updated with real coordinates.");
            }
        }

        private IDictionary<string, JsonNode> GetObservationsByOccurrenceIdDictionary(IEnumerable<JsonNode> observations)
        {
            var nodes = new Dictionary<string, JsonNode>();
            foreach (var obs in observations)
            {
                var occurrenceObject = obs["occurrence"];
                if (occurrenceObject != null)
                {
                    var occurrenceId = (string)occurrenceObject["occurrenceId"];
                    if (occurrenceId != null)
                    {
                        nodes.Add(occurrenceId, obs);
                    }
                }
            }

            return nodes;
        }

        private T GetValue<T>(IDictionary<string, object> obs, string propertyPath)
        {
            var parts = propertyPath
                .Split(".")
                .Select(m => m.ToCamelCase());

            var currentVal = obs;
            foreach (var part in parts)
            {
                if (currentVal.TryGetValue(part, out var currentValObject))
                {
                    if (currentValObject is IDictionary<string, object>)
                    {
                        currentVal = (IDictionary<string, object>)currentValObject;
                    }
                    else
                    {
                        return (T)currentValObject;
                    }
                }
            }

            return default;
        }

        private void UpdateValue<T>(IDictionary<string, object> obs, string propertyPath, T newValue)
        {
            var parts = propertyPath
                .Split(".")
                .Select(m => m.ToCamelCase())
                .ToList();

            var currentVal = obs;
            for (int i = 0; i < parts.Count; i++)
            {
                string part = parts[i];
                if (i == parts.Count - 1)
                {
                    if (currentVal.ContainsKey(part))
                    {
                        currentVal[part] = newValue;
                    }
                    return;
                }

                if (currentVal.TryGetValue(part, out var currentValObject))
                {
                    if (currentValObject is IDictionary<string, object>)
                    {
                        currentVal = (IDictionary<string, object>)currentValObject;
                    }
                }
            }
        }

        private void UpdateGeneralizedWithRealValues(JsonNode obs, JsonNode realObs)
        {
            // isGeneralized
            var isGeneralized = (bool?)obs["isGeneralized"];
            var isGeneralizedReal = (bool?)realObs["isGeneralized"];
            if ((isGeneralized ?? false) && (isGeneralizedReal ?? false))
            {
                obs["isGeneralized"] = realObs["isGeneralized"];
            }

            var location = obs["location"];
            var locationReal = realObs["location"];
            if (location != null && locationReal != null)
            {
                obs["location"] = realObs["location"];
            }

            var sensitive = obs["sensitive"];
            var sensitiveReal = realObs["sensitive"];
            if (sensitive != null && sensitiveReal != null)
            {
                obs["sensitive"] = realObs["sensitive"];
            }

            var occurrence = obs["occurrence"];
            var occurrenceReal = realObs["occurrence"];
            if (occurrence != null && occurrenceReal != null)
            {
                var sensitiveCategory = occurrence["sensitivityCategory"];
                var sensitiveCategoryReal = occurrenceReal["sensitivityCategory"];
                if (sensitiveCategory != null && sensitiveCategoryReal != null)
                {
                    occurrenceReal["sensitivityCategory"] = occurrence["sensitivityCategory"];
                }
            }
        }

        private void UpdateGeneralizedWithRealValues(Observation obs, Observation realObs)
        {
            // isGeneralized                        
            obs.IsGeneralized = realObs.IsGeneralized;
            obs.Location = realObs.Location;
            obs.Sensitive = realObs.Sensitive;
            obs.Occurrence.SensitivityCategory = realObs.Occurrence.SensitivityCategory;
        }       

        private List<Observation> CastDynamicsToObservations(IEnumerable<dynamic> dynamicObjects)
        {
            if (dynamicObjects == null) return null;
            return JsonSerializer.Deserialize<List<Observation>>(JsonSerializer.Serialize(dynamicObjects),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private List<string> GetOccurrenceIds(List<JsonNode> observations)
        {
            var occurrenceIds = new List<string>();
            foreach (var obs in observations)
            {
                // Occurrence
                var occurrenceObject = obs["occurrence"];
                if (occurrenceObject != null)
                {
                    var occurrenceId = (string)occurrenceObject["occurrenceId"];
                   
                    if (!string.IsNullOrEmpty(occurrenceId))
                    {
                        occurrenceIds.Add((string)occurrenceId);
                    }
                }
            }

            return occurrenceIds;
        }

        private List<JsonNode> GetGeneralizedObservations(IEnumerable<JsonNode> observations)
        {
            var generalizedObservations = new List<JsonNode>();
            try
            {
                foreach (var obs in observations)
                {
                    if (obs == null) continue;
                    var isGeneralized = (bool?)obs["isGeneralized"];
                    if (isGeneralized ?? false)
                    {
                        generalizedObservations.Add(obs);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when getting generalized observations");
            }

            return generalizedObservations;
        }        
    }
}