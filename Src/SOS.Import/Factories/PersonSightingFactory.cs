using System.Collections.Generic;
using System.Linq;
using System.Text;
using SOS.Import.Extensions;
using SOS.Import.Repositories.Source.Artportalen.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Import.Factories
{
    public static class PersonSightingFactory
    {
        public static Dictionary<int, PersonSighting> CreatePersonSightingDictionary(
            ISet<int> sightingIds,
            IDictionary<int, Person> personByUserId,
            IDictionary<int, Organization> organizationById,
            IList<SpeciesCollectionItem> speciesCollectionItems,
            IList<SightingRelation> sightingRelations)
        {
            if (!sightingIds?.Any() ?? true)
            {
                return null;
            }

            var personSightingBySightingId = new Dictionary<int, PersonSighting>();

            if (speciesCollectionItems?.Any() ?? false)
            {
                if (speciesCollectionItems?.Any() ?? false)
                {
                    //------------------------------------------------------------------------------
                    // Add SpeciesCollection values
                    //------------------------------------------------------------------------------
                    var speciesCollectionBySightingId = CreateSpeciesCollectionDictionary(personByUserId,
                        organizationById,
                        speciesCollectionItems);

                    if (speciesCollectionBySightingId?.Any() ?? false)
                    {
                        foreach (var pair in speciesCollectionBySightingId)
                        {
                            personSightingBySightingId.Add(pair.Key, new PersonSighting { SpeciesCollection = pair.Value });
                        }
                    }
                    
                    //------------------------------------------------------------------------------
                    // Add VerifiedBy values
                    //------------------------------------------------------------------------------
                    var verifiedByStringBySightingId = CreateVerifiedByStringDictionary(personByUserId,
                        speciesCollectionItems,
                        sightingRelations);

                    if (verifiedByStringBySightingId?.Any() ?? false)
                    {
                        foreach (var pair in verifiedByStringBySightingId)
                        {
                            var users = new List<UserInternal>();
                            if (pair.Value.determiner != null)
                            {
                                users.Add(pair.Value.determiner);
                            }

                            if (pair.Value.confirmator != null)
                            {
                                users.Add(pair.Value.confirmator);
                            } 
                             
                            if (!personSightingBySightingId.TryGetValue(pair.Key, out var personSighting))
                            {
                                personSighting = new PersonSighting();
                                personSightingBySightingId.Add(pair.Key, personSighting);
                            }

                            personSighting.VerifiedBy = pair.Value.names;
                            personSighting.VerifiedByInternal = users;
                        }
                    }
                }
            }
            
            //------------------------------------------------------------------------------
            // Add Observers values
            //------------------------------------------------------------------------------
            var observersBySightingId = CreateObserversDictionary(
                sightingRelations,
                personByUserId);

            if (observersBySightingId?.Any() ?? false)
            {
                foreach (var pair in observersBySightingId)
                {
                    if (!personSightingBySightingId.TryGetValue(pair.Key, out var personSighting))
                    {
                        personSighting = new PersonSighting();
                        personSightingBySightingId.Add(pair.Key, personSighting);
                    }

                    personSighting.Observers = pair.Value.names;
                    personSighting.ObserversInternal = pair.Value.users;
                }
            }

            //------------------------------------------------------------------------------
            // Add ReportedBy values
            //------------------------------------------------------------------------------
            var reportedBySightingId = CreateReportedByDictionary(
                sightingRelations,
                personByUserId);

            if (reportedBySightingId?.Any() ?? false)
            {
                foreach (var pair in reportedBySightingId)
                {
                    if (!personSightingBySightingId.TryGetValue(pair.Key, out var personSighting))
                    {
                        personSighting = new PersonSighting();
                        personSightingBySightingId.Add(pair.Key, personSighting);
                    }

                    personSighting.ReportedBy = pair.Value.FullName;
                    personSighting.ReportedByUserId = pair.Value.UserId;
                    personSighting.ReportedByUserAlias = pair.Value.Alias;

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
                    pair.Value.ObserversInternal = new List<UserInternal>
                    {
                        new UserInternal {Id = pair.Value.ReportedByUserId, UserAlias = pair.Value.ReportedByUserAlias}
                    };
                }
            }

            return personSightingBySightingId;
        }

        private static Dictionary<int, string> CreateSpeciesCollectionDictionary(
            IDictionary<int, Person> personById,
            IDictionary<int, Organization> organizationById,
            IList<SpeciesCollectionItem> speciesCollectionItems)
        {
            var speciesCollectionBySightingId = new Dictionary<int, string>();

            if (!speciesCollectionItems?.Any() ?? true)
            {
                return speciesCollectionBySightingId;
            }

            foreach (var speciesCollectionItem in speciesCollectionItems)
            {
                // Collection is collector
                if ((personById?.Any() ?? false) && speciesCollectionItem.CollectorId.HasValue &&
                    personById.TryGetValue(speciesCollectionItem.CollectorId.Value, out var person))
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
                if ((speciesCollectionItems?.Any() ?? false) && (organizationById?.Any() ?? false) && speciesCollectionItem.OrganizationId.HasValue &&
                    organizationById.TryGetValue(speciesCollectionItem.OrganizationId.Value, out var organization))
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

        private static IDictionary<int, (string names, IEnumerable<UserInternal> users)> CreateObserversDictionary(
            IEnumerable<SightingRelation> sightingRelations,
            IDictionary<int, Person> personsByUserId)
        {
            var observersBySightingId = new Dictionary<int, (string name, IEnumerable<UserInternal> alias)>();

            if ((!sightingRelations?.Any() ?? true) || (!personsByUserId?.Any() ?? true))
            {
                return observersBySightingId;
            }

            var query = sightingRelations
                .Where(y => y.SightingRelationTypeId == (int) SightingRelationTypeId.Observer && y.IsPublic)
                .GroupBy(y => y.SightingId);
            foreach (var grouping in query)
            {
                var persons = grouping.Where(p => personsByUserId.ContainsKey(p.UserId))
                    .OrderBy(ob => ob.Sort)
                    .Select(v => personsByUserId[v.UserId]);
                var observers = string.Join(", ", persons.Select(n => n.FullName)).WithMaxLength(256);
                observersBySightingId.Add(grouping.Key,
                    (observers, persons.Select(g => new UserInternal {Id = g.Id, UserAlias = g.Alias})));
            }

            return observersBySightingId;
        }

        private static IDictionary<int, Person> CreateReportedByDictionary(
            IEnumerable<SightingRelation> sightingRelations,
            IDictionary<int, Person> personsByUserId)
        {
            var reportedBySightingId = new Dictionary<int, Person>();

            if (!sightingRelations?.Any() ?? true)
            {
                return reportedBySightingId;
            }

            var query = sightingRelations
                .Where(y => y.SightingRelationTypeId == (int) SightingRelationTypeId.Reporter && y.IsPublic);
            foreach (var sightingRelation in query)
            {
                if (personsByUserId.TryGetValue(sightingRelation.UserId, out var person))
                {
                    if (!reportedBySightingId.ContainsKey(sightingRelation.SightingId))
                    {
                        reportedBySightingId.Add(sightingRelation.SightingId, person);
                    }
                }
            }

            return reportedBySightingId;
        }


        private static Dictionary<int, (string names, UserInternal determiner, UserInternal confirmator)>
            CreateVerifiedByStringDictionary(
                IDictionary<int, Person> personById,
                IList<SpeciesCollectionItem> speciesCollectionItems,
                IList<SightingRelation> sightingRelations
            )
        {
            var verifiedByDataSightingId = CreateVerifiedByDataDictionary(
                personById,
                speciesCollectionItems,
                sightingRelations);

            return verifiedByDataSightingId?.ToDictionary(x => x.Key,
                x => (ConcatenateVerifiedByString(x.Value), x.Value.DeterminerInternal, x.Value.ConfirmatorInternal));
        }

        private static Dictionary<int, VerifiedByData> CreateVerifiedByDataDictionary(
            IDictionary<int, Person> personById,
            IList<SpeciesCollectionItem> speciesCollectionItems,
            IList<SightingRelation> sightingRelations)
        {
            var verifiedByDataSightingId = new Dictionary<int, VerifiedByData>();

            if (!sightingRelations?.Any() ?? true)
            {
                return verifiedByDataSightingId;
            }

            var determinerQuery = sightingRelations.Where(x =>
                x.SightingRelationTypeId == (int) SightingRelationTypeId.Determiner
                && x.IsPublic
                && x.Sort == 0);

            foreach (var determinerRelation in determinerQuery)
            {
                var vbd = new VerifiedByData
                {
                    SightingId = determinerRelation.SightingId,
                    SightingRelationDeterminationYear = determinerRelation.DeterminationYear
                };

                if (!verifiedByDataSightingId.ContainsKey(determinerRelation.SightingId))
                {
                    verifiedByDataSightingId.Add(determinerRelation.SightingId, vbd);
                }

                if (personById?.Any() ?? false)
                {
                    if (personById.TryGetValue(determinerRelation.UserId, out var person))
                    {
                        vbd.DeterminerName = person.FullName;
                        vbd.DeterminerInternal = new UserInternal { Id = person.Id, UserAlias = person.Alias };
                    }
                }
            }

            var confirmatorQuery = sightingRelations.Where(x =>
                x.SightingRelationTypeId == (int) SightingRelationTypeId.Confirmator
                && x.IsPublic
                && x.Sort == 0);

            foreach (var confirmatorRelation in confirmatorQuery)
            {
                if (!verifiedByDataSightingId.TryGetValue(confirmatorRelation.SightingId, out var vbd))
                {
                    vbd = new VerifiedByData
                    {
                        SightingId = confirmatorRelation.SightingId,
                        SightingRelationConfirmationYear = confirmatorRelation.DeterminationYear
                    };
                    verifiedByDataSightingId.Add(confirmatorRelation.SightingId, vbd);
                }

                if (personById.TryGetValue(confirmatorRelation.UserId, out var person))
                {
                    vbd.ConfirmatorName = person.FullName;
                    vbd.ConfirmatorInternal = new UserInternal {Id = person.Id, UserAlias = person.Alias};
                }
            }

            if (speciesCollectionItems?.Any() ?? false)
            {
                foreach (var speciesCollectionItem in speciesCollectionItems)
                {
                    if (!verifiedByDataSightingId.TryGetValue(speciesCollectionItem.SightingId, out var vbd))
                    {
                        vbd = new VerifiedByData
                        {
                            SightingId = speciesCollectionItem.SightingId,
                            DeterminerText = speciesCollectionItem.DeterminerText,
                            SpeciesCollectionItemDeterminerYear = speciesCollectionItem.DeterminerYear,
                            DeterminationDescription = speciesCollectionItem.Description,
                            ConfirmatorText = speciesCollectionItem.ConfirmatorText,
                            SpeciesCollectionItemConfirmatorYear = speciesCollectionItem.ConfirmatorYear
                        };
                        verifiedByDataSightingId.Add(speciesCollectionItem.SightingId, vbd);
                    }
                }
            }

            return verifiedByDataSightingId;
        }

        private static string ConcatenateVerifiedByString(VerifiedByData vbd)
        {
            return vbd == null ? null : ConcatenateVerifiedByString(
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
            var sb = new StringBuilder();

            //----------------------------------------------------------------------
            // Set determiner text
            //----------------------------------------------------------------------
            var determinerNameTrimmed = determinerName?.Trim();
            var determinerTextTrimmed = determinerText?.Trim();
            var determinationDescriptionTrimmed = determinationDescription?.Trim();

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
            var confirmatorNameTrimmed = confirmatorName?.Trim();
            var confirmatorTextTrimmed = confirmatorText?.Trim();

            if (!string.IsNullOrEmpty(confirmatorNameTrimmed)
                || !string.IsNullOrEmpty(confirmatorTextTrimmed))
            {
                sb.Append($"{(sb.Length > 0 ? " # " : "")}Conf. ");

                if (!string.IsNullOrEmpty(confirmatorNameTrimmed))
                {
                    sb.Append(confirmatorNameTrimmed);
                }

                if (!string.IsNullOrEmpty(confirmatorTextTrimmed))
                {
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
            return sb.Length == 0 ? null : sb.ToString();
        }
    }
}