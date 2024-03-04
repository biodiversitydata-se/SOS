﻿using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Extensions;
using SOS.Harvest.Repositories.Source.Artportalen.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using System.Text;

namespace SOS.Harvest.Factories
{
    public static class PersonSightingFactory
    {
        public static Dictionary<int, PersonSighting>? CreatePersonSightingDictionary(
            ISet<int> sightingIds,
            IDictionary<int, Person>? personsByUserId,
            IDictionary<int, Metadata<int>>? organizations,
            IDictionary<int, ICollection<SpeciesCollectionItemEntity>> speciesCollectionItemsBySightingId,
            IEnumerable<SightingRelation>? sightingRelations)
        {
            if (!sightingIds?.Any() ?? true)
            {
                return null;
            }

            var personSightingBySightingId = new Dictionary<int, PersonSighting>();

            if (speciesCollectionItemsBySightingId?.Any() ?? false)
            {
                //------------------------------------------------------------------------------
                // Add SpeciesCollection values
                //------------------------------------------------------------------------------
                var speciesCollectionBySightingId = CreateSpeciesCollectionDictionary(personsByUserId,
                    organizations,
                    speciesCollectionItemsBySightingId);

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
                var verifiedByStringBySightingId = CreateVerifiedByStringDictionary(personsByUserId,
                    speciesCollectionItemsBySightingId,
                    sightingRelations!);

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

            //------------------------------------------------------------------------------
            // Add Observers values
            //------------------------------------------------------------------------------
            var observersBySightingId = CreateObserversDictionary(
                sightingRelations!,
                personsByUserId);

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
            // Add ConfirmedBy, DeterminedBy and ReportedBy values
            //------------------------------------------------------------------------------
            Populate(
                personSightingBySightingId,
                sightingRelations!,
                personsByUserId);
            

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

        private static void Populate(
            IDictionary<int, PersonSighting> personSightingBySightingId,
            IEnumerable<SightingRelation>? sightingRelations,
            IDictionary<int, Person>? personsByUserId)
        {
           
            if ((!sightingRelations?.Any() ?? true) || (!personsByUserId?.Any() ?? true))
            {
                return;
            }
            
            foreach (var sightingRelation in sightingRelations!)
            {
                if (personsByUserId!.TryGetValue(sightingRelation.UserId, out var person))
                {
                    if (!personSightingBySightingId.TryGetValue(sightingRelation.SightingId, out var personSighting))
                    {
                        personSighting = new PersonSighting();
                        personSightingBySightingId.Add(sightingRelation.SightingId, personSighting);
                    }

                    switch ((SightingRelationTypeId)sightingRelation.SightingRelationTypeId)
                    {
                        case SightingRelationTypeId.Confirmator:
                            personSighting.ConfirmedBy = person.FullName;
                            personSighting.ConfirmationYear = sightingRelation.DeterminationYear;
                            break;
                        case SightingRelationTypeId.Determiner:
                            personSighting.DeterminedBy = person.FullName;
                            personSighting.DeterminationYear = sightingRelation.DeterminationYear;
                            break;
                        case SightingRelationTypeId.Reporter:
                            personSighting.ReportedBy = person.FullName;
                            personSighting.ReportedByUserId = person.UserId;
                            personSighting.ReportedByUserServiceUserId = person.UserServiceUserId;
                            personSighting.ReportedByUserAlias = person.Alias;
                            break;
                    }
                }
            }
        }

        private static IDictionary<int, (Person Person, int? determinationYear)> CreateSightingRelationDictionary(SightingRelationTypeId sightingRelationType,
           IEnumerable<SightingRelation>? sightingRelations,
           IDictionary<int, Person>? personsByUserId)
        {
            var reportedBySightingId = new Dictionary<int, (Person Person, int? determinationYear)>();

            if ((!sightingRelations?.Any() ?? true) || (!personsByUserId?.Any() ?? true))
            {
                return reportedBySightingId;
            }

            var query = sightingRelations!
                .Where(y => y.SightingRelationTypeId == (int)sightingRelationType);
            foreach (var sightingRelation in query)
            {
                if (personsByUserId!.TryGetValue(sightingRelation.UserId, out var person))
                {
                    if (!reportedBySightingId.ContainsKey(sightingRelation.SightingId))
                    {
                        reportedBySightingId.Add(sightingRelation.SightingId, (person, sightingRelation.DeterminationYear));
                    }
                }
            }

            return reportedBySightingId;
        }

        private static Dictionary<int, string> CreateSpeciesCollectionDictionary(
            IDictionary<int, Person>? personsByUserId,
            IDictionary<int, Metadata<int>>? organizations,
            IDictionary<int, ICollection<SpeciesCollectionItemEntity>>? speciesCollectionItemsBySightingId)
        {
            var speciesCollectionBySightingId = new Dictionary<int, string>();
            if (!speciesCollectionItemsBySightingId?.Any() ?? true)
            {
                return speciesCollectionBySightingId;
            }

            foreach (var item in speciesCollectionItemsBySightingId!)
            {
                foreach (var speciesCollectionItem in item.Value)
                {
                    // Collection is collector
                    if ((personsByUserId?.Any() ?? false) && speciesCollectionItem.CollectorId.HasValue &&
                        personsByUserId.TryGetValue(speciesCollectionItem.CollectorId.Value, out var person))
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
                    if ((organizations?.Any() ?? false) && speciesCollectionItem.OrganizationId.HasValue &&
                        organizations.TryGetValue(speciesCollectionItem.OrganizationId.Value, out var organization))
                    {
                        var name = organization?.Translations?.FirstOrDefault()?.Value ?? "";
                        if (speciesCollectionBySightingId.ContainsKey(speciesCollectionItem.SightingId))
                        {
                            speciesCollectionBySightingId[speciesCollectionItem.SightingId] = name;
                        }
                        else
                        {
                            speciesCollectionBySightingId.Add(speciesCollectionItem.SightingId, name);
                        }
                    }
                }
            }

            return speciesCollectionBySightingId;
        }

        private static IDictionary<int, (string? names, IEnumerable<UserInternal> users)> CreateObserversDictionary(
            IEnumerable<SightingRelation>? sightingRelations,
            IDictionary<int, Person>? personsByUserId)
        {
            var observersBySightingId = new Dictionary<int, (string? names, IEnumerable<UserInternal> alias)>();

            if ((!sightingRelations?.Any() ?? true) || (!personsByUserId?.Any() ?? true))
            {
                return observersBySightingId;
            }

            var query = sightingRelations!
                .Where(y => y.SightingRelationTypeId == (int)SightingRelationTypeId.Observer)
                .GroupBy(y => y.SightingId);
            foreach (var grouping in query)
            {
                var persons = grouping.Where(p => personsByUserId!.ContainsKey(p.UserId))
                    .OrderByDescending(ob => ob.Sort)
                    .Select(v => (person: personsByUserId![v.UserId], viewAccess: v.SightingRelationTypeId.Equals(2), discover: v.Discover));
                var observers = string.Join(", ", persons.Select(n => n.person.FullName)).WithMaxLength(256);
                observersBySightingId.Add(grouping.Key,
                    (observers, persons.Select(g => new UserInternal
                    {
                        Discover = g.discover,
                        Id = g.person.UserId,
                        PersonId = g.person.Id,
                        UserServiceUserId = g.person.UserServiceUserId,
                        UserAlias = g.person.Alias,
                        ViewAccess = g.viewAccess
                    })));
            }

            return observersBySightingId;
        }


        private static Dictionary<int, (string? names, UserInternal? determiner, UserInternal? confirmator)>?
            CreateVerifiedByStringDictionary(
                IDictionary<int, Person>? personsByUserId,
                IDictionary<int, ICollection<SpeciesCollectionItemEntity>>? speciesCollectionItemsBySightingId,
                IEnumerable<SightingRelation>? sightingRelations
            )
        {
            var verifiedByDataSightingId = CreateVerifiedByDataDictionary(
                personsByUserId,
                speciesCollectionItemsBySightingId,
                sightingRelations);

            return verifiedByDataSightingId?.ToDictionary(x => x.Key,
                x => (ConcatenateVerifiedByString(x.Value), x.Value.DeterminerInternal, x.Value.ConfirmatorInternal))!;
        }

        private static Dictionary<int, VerifiedByData> CreateVerifiedByDataDictionary(
            IDictionary<int, Person>? personsByUserId,
            IDictionary<int, ICollection<SpeciesCollectionItemEntity>>? speciesCollectionItemsBySightingId,
            IEnumerable<SightingRelation>? sightingRelations)
        {
            var verifiedByDataSightingId = new Dictionary<int, VerifiedByData>();

            if (!sightingRelations?.Any() ?? true)
            {
                return verifiedByDataSightingId;
            }

            var determinerQuery = sightingRelations!.Where(x =>
                x.SightingRelationTypeId == (int)SightingRelationTypeId.Determiner
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

                if (personsByUserId?.Any() ?? false)
                {
                    if (personsByUserId.TryGetValue(determinerRelation.UserId, out var person))
                    {
                        vbd.DeterminerName = person.FullName;
                        vbd.DeterminerInternal = new UserInternal
                        {
                            Id = person.UserId,
                            PersonId = person.Id,
                            UserAlias = person.Alias,
                            UserServiceUserId = person.UserServiceUserId
                        };
                    }
                }
            }

            var confirmatorQuery = sightingRelations!.Where(x =>
                x.SightingRelationTypeId == (int)SightingRelationTypeId.Confirmator
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

                if (personsByUserId?.TryGetValue(confirmatorRelation.UserId, out var person) ?? false)
                {
                    vbd.ConfirmatorName = person.FullName;
                    vbd.ConfirmatorInternal = new UserInternal
                    {
                        Id = person.UserId,
                        PersonId = person.Id,
                        UserAlias = person.Alias,
                        UserServiceUserId = person.UserServiceUserId
                    };
                }
            }

            if (speciesCollectionItemsBySightingId?.Any() ?? false)
            {
                foreach (var item in speciesCollectionItemsBySightingId)
                {
                    if (!verifiedByDataSightingId.TryGetValue(item.Key, out var vbd))
                    {
                        var speciesCollectionItem = item.Value.First();
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

        private static string? ConcatenateVerifiedByString(VerifiedByData vbd)
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

        public static string? ConcatenateVerifiedByString(
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
                sb.Append($"{(sb.Length > 0 ? " # " : "")}Conf.");

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
            return sb.Length == 0 ? null : sb.ToString();
        }
    }
}