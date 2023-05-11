using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Extensions
{
    public static class UserObservationExtensions
    {
        /// <summary>
        /// Create user observation
        /// </summary>
        /// <param name="observation"></param>
        /// <param name="recordedBy"></param>
        /// <returns></returns>
        private static UserObservation ToUserObservation(this Observation observation, UserInternal recordedBy)
        {
            try
            {
                var userObservation = new UserObservation();
                userObservation.Id = UserObservation.CreateId();
                userObservation.UserId = recordedBy.Id;
                userObservation.UserServiceUserId = recordedBy.UserServiceUserId;
                userObservation.SightingId = observation.ArtportalenInternal.SightingId;
                userObservation.TaxonId = observation.Taxon.Id;
                userObservation.TaxonSpeciesGroupId = (int)observation.Taxon.Attributes.SpeciesGroup; // todo Check if correct
                userObservation.ProvinceFeatureId = observation.Location?.Province?.FeatureId; 
                userObservation.MunicipalityFeatureId = observation.Location?.Municipality?.FeatureId;
                userObservation.CountryRegionFeatureId = observation.Location?.CountryRegion?.FeatureId;                                                                                  //userObservation.CountryRegionFeatureId = // todo
                userObservation.IsBirdsite = observation.Taxon?.Attributes?.SpeciesGroup.Equals(SpeciesGroup.Birds) ?? false;
                userObservation.ReporterId = observation.ArtportalenInternal?.ReportedByUserId ?? 0;

                int? siteId = null;
                if (userObservation.IsBirdsite)
                {
                    siteId = observation.Location?.Attributes?.IsPrivate ?? false ? observation.ArtportalenInternal?.ParentLocationId : observation.ArtportalenInternal?.IncludedByLocationId;
                }
                userObservation.SiteId = siteId;

                if (observation.Event.StartDate.HasValue)
                {
                    userObservation.StartDate = observation.Event.StartDate.Value;
                    userObservation.ObservationYear = userObservation.StartDate.Year;
                    userObservation.ObservationMonth = userObservation.StartDate.Month;
                }
               
                if (observation.Projects?.Any() ?? false)
                {
                    userObservation.ProjectIds = observation.Projects.Select(m => m.Id).ToList();
                }
                userObservation.ProtectedBySystem = observation.Sensitive;
                //userObservation.ProtectedByUser = // todo

                // Debug info
                userObservation.UserAlias = recordedBy.UserAlias;
                userObservation.ReporterName = observation.Occurrence?.ReportedBy;
                userObservation.TaxonScientificName = observation.Taxon?.ScientificName;
                userObservation.TaxonVernacularName = observation.Taxon?.VernacularName;
                userObservation.TaxonSortOrder = observation.Taxon?.Attributes.SortOrder ?? 0;

                return userObservation;
            }
            catch (Exception e)
            {
                int sightingId = observation?.ArtportalenInternal?.SightingId ?? -1;
                string provinceId = observation?.Location?.Province?.FeatureId ?? "null";
                string municipalityId = observation?.Location?.Municipality?.FeatureId ?? "null";
                throw new Exception($"ToUserObservation exception. userId={recordedBy.Id}, sightingId={sightingId}, provinceId={provinceId}, municipalityId={municipalityId}", e);
            }
        }

        /// <summary>
        ///  Create user observations
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<UserObservation> ToUserObservations(this IEnumerable<Observation> observations)
        {
            return observations?.Where(o =>
                o.Occurrence.IsNaturalOccurrence &&
                o.Occurrence.IsPositiveObservation &&
                o.Identification.Verified &&
                !o.Identification.UncertainIdentification &&
                new[] { 0, 1, 2, 3, 4, 5 }.Contains(o.ArtportalenInternal?.ActivityCategoryId ?? -1) &&
                new[] { 17, 18, 19, 20, 32 }.Contains(o.Taxon.Attributes?.TaxonCategory?.Id ?? -1) &&
                new[] { 0, 8 }.Contains(o.ArtportalenInternal?.SightingTypeId ?? -1)
            ).SelectMany(ToUserObservations) ?? Enumerable.Empty<UserObservation>();
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
                ToUserObservation(observation, m));
        }

        
    }
}