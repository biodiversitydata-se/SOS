using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SOS.Import.Extensions;
using SOS.Import.Repositories.Source.Artportalen.Enums;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Import.Factories
{
    public static class PersonSightingFactory
    {
        public static Dictionary<int, PersonSighting> CalculatePersonSightingDictionary(
           ISet<int> sightingIds,
           IDictionary<int, Person> personByUserId,
           IDictionary<int, Organization> organizationById,
           IList<SpeciesCollectionItem> speciesCollectionItems,
           IList<SightingRelation> sightingRelations)
        {
            var personSightingBySightingId = new Dictionary<int, PersonSighting>();
            var filteredSpeciesCollectionItems = speciesCollectionItems.Where(x => sightingIds.Contains(x.SightingId)).ToArray();

            //------------------------------------------------------------------------------
            // Add SpeciesCollection values
            //------------------------------------------------------------------------------
            var speciesCollectionBySightingId = CalculateSpeciesCollectionDictionary(personByUserId,
                organizationById,
                filteredSpeciesCollectionItems);

            foreach (var pair in speciesCollectionBySightingId)
            {
                personSightingBySightingId.Add(pair.Key, new PersonSighting { SpeciesCollection = pair.Value });
            }

            //------------------------------------------------------------------------------
            // Add Observers values
            //------------------------------------------------------------------------------
            var observersBySightingId = CalculateObserversDictionary(
                sightingRelations,
                personByUserId);

            foreach (var pair in observersBySightingId)
            {
                if (personSightingBySightingId.TryGetValue(pair.Key, out PersonSighting personSighting))
                {
                    personSighting.Observers = pair.Value;
                }
                else
                {
                    personSightingBySightingId.Add(pair.Key, new PersonSighting { Observers = pair.Value });
                }
            }

            //------------------------------------------------------------------------------
            // Add VerifiedBy values
            //------------------------------------------------------------------------------
            var verifiedByStringBySightingId = CalculateVerifiedByStringDictionary(personByUserId,
                filteredSpeciesCollectionItems,
                sightingRelations);

            foreach (var pair in verifiedByStringBySightingId)
            {
                if (personSightingBySightingId.TryGetValue(pair.Key, out PersonSighting personSighting))
                {
                    personSighting.VerifiedBy = pair.Value;
                }
                else
                {
                    personSightingBySightingId.Add(pair.Key, new PersonSighting { VerifiedBy = pair.Value });
                }
            }

            //------------------------------------------------------------------------------
            // Add ReportedBy values
            //------------------------------------------------------------------------------
            var reportedBySightingId = CalculateReportedByDictionary(
                sightingRelations,
                personByUserId);

            foreach (var pair in reportedBySightingId)
            {
                if (personSightingBySightingId.TryGetValue(pair.Key, out PersonSighting personSighting))
                {
                    personSighting.ReportedBy = pair.Value;
                }
                else
                {
                    personSightingBySightingId.Add(pair.Key, new PersonSighting { ReportedBy = pair.Value });
                }
            }

            //------------------------------------------------------------------------------
            // Set Observers to ReportedBy when Observers value is null
            //------------------------------------------------------------------------------
            foreach (var pair in personSightingBySightingId)
            {
                if (string.IsNullOrEmpty(pair.Value.Observers) && !string.IsNullOrEmpty(pair.Value.ReportedBy))
                {
                    pair.Value.Observers = "Via " + pair.Value.ReportedBy;
                }
            }

            return personSightingBySightingId;
        }

        private static Dictionary<int, string> CalculateSpeciesCollectionDictionary(
            IDictionary<int, Person> personById,
            IDictionary<int, Organization> organizationById,
            IList<SpeciesCollectionItem> speciesCollectionItems)
        {
            var speciesCollectionBySightingId = new Dictionary<int, string>();

            foreach (var speciesCollectionItem in speciesCollectionItems)
            {
                // Collection is collector
                if (speciesCollectionItem.CollectorId.HasValue && personById.TryGetValue(speciesCollectionItem.CollectorId.Value, out Person person))
                {
                    if (speciesCollectionBySightingId.ContainsKey(speciesCollectionItem.SightingId))
                    {
                        speciesCollectionBySightingId[speciesCollectionItem.SightingId] = person.FullName;
                    }
                    else
                    {
                        speciesCollectionBySightingId.Add(speciesCollectionItem.SightingId, person.FullName);
                    }
                }

                // Collection is Organization
                if (speciesCollectionItem.OrganizationId.HasValue && organizationById.TryGetValue(speciesCollectionItem.OrganizationId.Value, out Organization organization))
                {
                    if (speciesCollectionBySightingId.ContainsKey(speciesCollectionItem.SightingId))
                    {
                        speciesCollectionBySightingId[speciesCollectionItem.SightingId] = organization.Name;
                    }
                    else
                    {
                        speciesCollectionBySightingId.Add(speciesCollectionItem.SightingId, organization.Name);
                    }
                }
            }

            return speciesCollectionBySightingId;
        }

        private static IDictionary<int, string> CalculateObserversDictionary(
            IEnumerable<SightingRelation> sightingRelations,
            IDictionary<int, Person> personsByUserId)
        {
            Dictionary<int, string> observersBySightingId = new Dictionary<int, string>();
            var query = sightingRelations
                .Where(y => y.SightingRelationTypeId == (int)SightingRelationTypeId.Observer && y.IsPublic)
                .GroupBy(y => y.SightingId);
            foreach (var grouping in query)
            {
                IEnumerable<Person> persons = grouping.Where(p => personsByUserId.ContainsKey(p.UserId)).Select(v => personsByUserId[v.UserId]);
                string observers = string.Join(", ", persons.Select(n => n.FullName)).WithMaxLength(256);
                observersBySightingId.Add(grouping.Key, observers);
            }

            return observersBySightingId;
        }

        private static IDictionary<int, string> CalculateReportedByDictionary(
            IEnumerable<SightingRelation> sightingRelations,
            IDictionary<int, Person> personsByUserId)
        {
            Dictionary<int, string> reportedBySightingId = new Dictionary<int, string>();
            var query = sightingRelations
                .Where(y => y.SightingRelationTypeId == (int)SightingRelationTypeId.Reporter && y.IsPublic);
            foreach (var sightingRelation in query)
            {
                if (personsByUserId.TryGetValue(sightingRelation.UserId, out Person person))
                {
                    if (!reportedBySightingId.ContainsKey(sightingRelation.SightingId))
                    {
                        reportedBySightingId.Add(sightingRelation.SightingId, person.FullName);
                    }
                }
            }

            return reportedBySightingId;
        }


        private static Dictionary<int, string> CalculateVerifiedByStringDictionary(
            IDictionary<int, Person> personById,
            IList<SpeciesCollectionItem> speciesCollectionItems,
            IList<SightingRelation> sightingRelations
        )
        {
            var verifiedByDataSightingId = CalculateVerifiedByDataDictionary(
                personById,
                speciesCollectionItems,
                sightingRelations);

            return verifiedByDataSightingId.ToDictionary(x => x.Key, x => ConcatenateVerifiedByString(x.Value));
        }

        private static Dictionary<int, VerifiedByData> CalculateVerifiedByDataDictionary(
            IDictionary<int, Person> personById,
            IList<SpeciesCollectionItem> speciesCollectionItems,
            IList<SightingRelation> sightingRelations)
        {
            var verifiedByDataSightingId = new Dictionary<int, VerifiedByData>();
            var determinerQuery = sightingRelations.Where(x =>
                x.SightingRelationTypeId == (int)SightingRelationTypeId.Determiner
                && x.IsPublic
                && x.Sort == 0);

            foreach (var determinerRelation in determinerQuery)
            {
                var vbd = new VerifiedByData { SightingId = determinerRelation.SightingId };

                if (!verifiedByDataSightingId.ContainsKey(determinerRelation.SightingId))
                {
                    verifiedByDataSightingId.Add(determinerRelation.SightingId, vbd);
                }

                if (personById.TryGetValue(determinerRelation.UserId, out Person person))
                {
                    vbd.DeterminerName = person.FullName;
                }

                vbd.SightingRelationDeterminationYear = determinerRelation.DeterminationYear;
            }

            var confirmatorQuery = sightingRelations.Where(x =>
                x.SightingRelationTypeId == (int)SightingRelationTypeId.Confirmator
                && x.IsPublic
                && x.Sort == 0);

            foreach (var confirmatorRelation in confirmatorQuery)
            {
                if (!verifiedByDataSightingId.TryGetValue(confirmatorRelation.SightingId, out var vbd))
                {
                    vbd = new VerifiedByData { SightingId = confirmatorRelation.SightingId };
                    verifiedByDataSightingId.Add(confirmatorRelation.SightingId, vbd);
                }

                if (personById.TryGetValue(confirmatorRelation.UserId, out Person person))
                {
                    vbd.ConfirmatorName = person.FullName;
                }

                vbd.SightingRelationConfirmationYear = confirmatorRelation.DeterminationYear;
            }

            foreach (var speciesCollectionItem in speciesCollectionItems)
            {
                if (!verifiedByDataSightingId.TryGetValue(speciesCollectionItem.SightingId, out var vbd))
                {
                    vbd = new VerifiedByData { SightingId = speciesCollectionItem.SightingId };
                    verifiedByDataSightingId.Add(speciesCollectionItem.SightingId, vbd);
                }

                vbd.DeterminerText = speciesCollectionItem.DeterminerText;
                vbd.SpeciesCollectionItemDeterminerYear = speciesCollectionItem.DeterminerYear;
                vbd.DeterminationDescription = speciesCollectionItem.Description;
                vbd.ConfirmatorText = speciesCollectionItem.ConfirmatorText;
                vbd.SpeciesCollectionItemConfirmatorYear = speciesCollectionItem.ConfirmatorYear;
            }

            return verifiedByDataSightingId;
        }

        public static string ConcatenateVerifiedByString(VerifiedByData vbd)
        {
            return ConcatenateVerifiedByString(
                vbd.DeterminerName,
                vbd.DeterminerText,
                vbd.DeterminerYear,
                vbd.DeterminationDescription,
                vbd.ConfirmatorName,
                vbd.ConfirmatorText,
                vbd.ConfirmatorYear);
        }

        public static string ConcatenateVerifiedByString(
            string determinerName,
            string determinerText,
            int? determinerYear,
            string determinationDescription,
            string confirmatorName,
            string confirmatorText,
            int? confirmatorYear)
        {
            StringBuilder sb = new StringBuilder();

            //----------------------------------------------------------------------
            // Set determiner text
            //----------------------------------------------------------------------
            string determinerNameTrimmed = determinerName?.Trim();
            string determinerTextTrimmed = determinerText?.Trim();
            string determinationDescriptionTrimmed = determinationDescription?.Trim();

            if (!string.IsNullOrEmpty(determinerNameTrimmed))
            {
                sb.Append(determinerNameTrimmed);
            }

            if (!string.IsNullOrEmpty(determinerTextTrimmed))
            {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(determinerTextTrimmed);
            }

            if (determinerYear.HasValue)
            {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(determinerYear.Value);
            }

            if (!string.IsNullOrEmpty(determinationDescriptionTrimmed))
            {
                if (sb.Length > 0) sb.Append(" # ");
                sb.Append(determinationDescriptionTrimmed);
            }


            //----------------------------------------------------------------------
            // Set confirmator text
            //----------------------------------------------------------------------
            string confirmatorNameTrimmed = confirmatorName?.Trim();
            string confirmatorTextTrimmed = confirmatorText?.Trim();

            if (!string.IsNullOrEmpty(confirmatorNameTrimmed)
                || !string.IsNullOrEmpty(confirmatorTextTrimmed))
            {
                sb.Append(sb.Length == 0 ? "Conf." : " # Conf.");

                if (!string.IsNullOrEmpty(confirmatorNameTrimmed))
                {
                    sb.Append(" ");
                    sb.Append(confirmatorNameTrimmed);
                }

                if (!string.IsNullOrEmpty(confirmatorTextTrimmed))
                {
                    sb.Append(" ");
                    sb.Append(confirmatorTextTrimmed);
                }

                if (confirmatorYear.HasValue)
                {
                    sb.Append(" ");
                    sb.Append(confirmatorYear.Value);
                }
            }

            //----------------------------------------------------------------------
            // Return result
            //----------------------------------------------------------------------
            if (sb.Length == 0)
            {
                return null;
            }

            return sb.ToString();
        }
    }
}
