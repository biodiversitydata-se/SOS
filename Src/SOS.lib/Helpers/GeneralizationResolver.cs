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

        public GeneralizationResolver(ILogger<GeneralizationResolver> logger, IFilterManager filterManager, IProcessedObservationCoreRepository processedObservationRepository)
        {
            _logger = logger;
            _filterManager = filterManager;
            _processedObservationRepository = processedObservationRepository;
        }

        public async Task ResolveGeneralizedObservationsAsync(SearchFilter filter, IEnumerable<IDictionary<string, object>> observations)
        {            
            try
            {
                if (!(filter.ExtendedAuthorization.ProtectionFilter == ProtectionFilter.BothPublicAndSensitive && filter.ExtendedAuthorization != null && filter.ExtendedAuthorization.UserId != 0))
                {
                    return;
                }

                List<IDictionary<string, object>> generalizedObservations = GetGeneralizedObservations(observations);
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

                List<IDictionary<string, object>> sensitiveObservations = dynamicSensitiveObservationsResult.Records.Cast<IDictionary<string, object>>().ToList();
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

        private void UpdateGeneralizedWithRealValues(List<IDictionary<string, object>> observations, List<IDictionary<string, object>> realObservations)
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

        private Dictionary<string, IDictionary<string, object>> GetObservationsByOccurrenceIdDictionary(List<IDictionary<string, object>> observations)
        {
            var dict = new Dictionary<string, IDictionary<string, object>>();
            foreach (var obs in observations)
            {
                string? occurrenceId = GetValue<string?>(obs, "occurrence.occurrenceId");
                if (occurrenceId != null)
                {
                    dict.Add(occurrenceId, obs);
                }
            }

            return dict;
        }

        private T? GetValue<T>(IDictionary<string, object> obs, string propertyPath)
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

        private void UpdateGeneralizedWithRealValues(IDictionary<string, object> obs, IDictionary<string, object> realObs)
        {
            // isGeneralized
            if (obs.ContainsKey("isGeneralized"))
            {
                if (realObs.ContainsKey("isGeneralized"))
                {
                    obs["isGeneralized"] = realObs["isGeneralized"];
                }
            }

            if (obs.ContainsKey("location"))
            {
                if (realObs.ContainsKey("location"))
                {
                    obs["location"] = realObs["location"];
                }
            }
            
            if (obs.ContainsKey("sensitive"))
            {
                if (realObs.ContainsKey("sensitive"))
                {
                    obs["sensitive"] = realObs["sensitive"];
                }
            }

            if (obs.ContainsKey("sensitive"))
            {
                if (realObs.ContainsKey("sensitive"))
                {
                    obs["sensitive"] = realObs["sensitive"];
                }
            }
            
            if (obs.TryGetValue(nameof(Observation.Occurrence).ToLower(),
                            out var occurrenceObject))
            {
                var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                if (occurrenceDictionary.TryGetValue("sensitivityCategory", out var sensitivityCategory))
                {
                    if (realObs.TryGetValue(nameof(Observation.Occurrence).ToLower(),
                            out var realOccurrenceObject))
                    {
                        var realOccurrenceDictionary = realOccurrenceObject as IDictionary<string, object>;
                        if (realOccurrenceDictionary.ContainsKey("sensitivityCategory"))
                        {
                            realOccurrenceDictionary["sensitivityCategory"] = sensitivityCategory;
                        }
                    }
                }
            }



            // Replace all fields
            //foreach (var key in obs.Keys)
            //{
            //    if (realObs.ContainsKey(key))
            //    {
            //        obs[key] = realObs[key];
            //    }
            //}
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

        private List<string> GetOccurrenceIds(List<IDictionary<string, object>> observations)
        {
            var occurrenceIds = new List<string>();
            foreach (var obs in observations)
            {
                // Occurrence
                if (obs.TryGetValue(nameof(Observation.Occurrence).ToLower(),
                                out var occurrenceObject))
                {
                    var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                    if (occurrenceDictionary.TryGetValue("occurrenceId", out var occurrenceId))
                    {
                        occurrenceIds.Add((string)occurrenceId);
                    }
                }
            }

            return occurrenceIds;
        }

        private List<IDictionary<string, object>> GetGeneralizedObservations(IEnumerable<IDictionary<string, object>> observations)
        {
            var generalizedObservations = new List<IDictionary<string, object>>();
            try
            {
                foreach (var obs in observations)
                {
                    if (obs == null) continue;
                    if (obs.ContainsKey("isGeneralized"))
                    {
                        bool isGeneralized = (bool)obs["isGeneralized"];
                        if (isGeneralized)
                        {
                            generalizedObservations.Add(obs);
                        }
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