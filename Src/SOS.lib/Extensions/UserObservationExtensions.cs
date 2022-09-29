using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Extensions
{
    public static class UserObservationExtensions
    {
        /// <summary>
        ///  Create user observations
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<UserObservation> ToUserObservations(this IEnumerable<Observation> observations)
        {
            return observations?.SelectMany(ToUserObservations) ?? Enumerable.Empty<UserObservation>();
        }

        /// <summary>
        ///  Create user observations
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<UserObservation> ToUserObservations(this Observation observation)
        {
            if (observation.ArtportalenInternal?.OccurrenceRecordedByInternal == null ||
                !observation.ArtportalenInternal.OccurrenceRecordedByInternal.Any())
            {
                return Enumerable.Empty<UserObservation>();
            }

            return observation.ArtportalenInternal.OccurrenceRecordedByInternal.Select(m =>
                ToUserObservation(observation, m.Id, m.UserServiceUserId));
        }

        /// <summary>
        /// Create user observation
        /// </summary>
        /// <param name="observation"></param>
        /// <param name="userId"></param>
        /// <param name="userServiceUserId"></param>
        /// <returns></returns>
        public static UserObservation ToUserObservation(this Observation observation, int userId, int? userServiceUserId)
        {
            try
            {
                var userObservation = new UserObservation();
                userObservation.Id = UserObservation.CreateId();
                userObservation.UserId = userId;
                userObservation.UserServiceUserId = userServiceUserId;
                userObservation.SightingId = observation.ArtportalenInternal.SightingId;
                userObservation.TaxonId = observation.Taxon.Id;
                userObservation.TaxonSpeciesGroupId = (int)observation.Taxon.Attributes.SpeciesGroup; // todo Check if correct
                userObservation.ProvinceFeatureId = observation?.Location?.Province?.FeatureId; // int.Parse(observation.Location.Province.FeatureId);
                userObservation.MunicipalityFeatureId = observation?.Location?.Municipality?.FeatureId; // int.Parse(observation.Location.Municipality.FeatureId);
                                                                                                        //userObservation.CountryRegionFeatureId = // todo
                var regex = new Regex(@"\d+$");
                var match = regex.Match(observation.Location?.LocationId);
                if (int.TryParse(match.Value, out var siteId))
                {
                    userObservation.SiteId = siteId;
                }
                   
                userObservation.StartDate = observation.Event.StartDate.Value;
                userObservation.ObservationYear = userObservation.StartDate.Year;
                userObservation.ObservationMonth = userObservation.StartDate.Month;
                if (observation.Projects != null && observation.Projects.Any())
                {
                    userObservation.ProjectIds = observation.Projects.Select(m => m.Id).ToList();
                }
                userObservation.ProtectedBySystem = observation.Sensitive;
                //userObservation.ProtectedByUser = // todo

                return userObservation;
            }
            catch (Exception e)
            {
                int sightingId = observation?.ArtportalenInternal?.SightingId ?? -1;
                string provinceId = observation?.Location?.Province?.FeatureId ?? "null";
                string municipalityId = observation?.Location?.Municipality?.FeatureId ?? "null";
                throw new Exception($"ToUserObservation exception. userId={userId}, sightingId={sightingId}, provinceId={provinceId}, municipalityId={municipalityId}", e);
            }
        }
    }
}