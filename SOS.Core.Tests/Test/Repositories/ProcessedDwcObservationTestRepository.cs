using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SOS.Core.Models.Observations;
using SOS.Core.Tests.TestRepositories;

namespace SOS.Core.Tests.Test.Repositories
{
    public static class ProcessedDwcObservationTestRepository
    {
        private static readonly Random Random = new Random();
        private static int _counter = 1;

        private static readonly int[] TaxonIds =
        {
            103025, // Blåmes
            101656, // Trumgräshoppa
            100067, // Havsörn
            100024, // Varg
            220396 // Tussilago
        };

        private static readonly string[] dataProviders =
        {
            "Artportalen",
            "MVM",
            "Shark",
            "KUL",
            "WRAM"
        };

        public static ProcessedDwcObservation CreateRandomObservation()
        {
            ProcessedDwcObservation observation = new ProcessedDwcObservation();

            //-----------------------------------------------------------------------------------------------------------
            // Ids
            //-----------------------------------------------------------------------------------------------------------
            observation.DataProviderId = Random.Next(1, dataProviders.Length);
            observation.CatalogNumber = _counter++.ToString();
            observation.Id = long.Parse(observation.CatalogNumber); // todo - Remove property and use only DataProviderId and CatalogNumber?
            observation.DyntaxaTaxonId = TaxonIds[Random.Next(0, TaxonIds.Length)];
            observation.OccurrenceID = GetOccurrenceId(observation.DataProviderId, observation.CatalogNumber);
            observation.DatasetName = GetDatasetName(observation.DataProviderId, observation.CatalogNumber);
            observation.DatasetID = $"urn:lsid:swedishlifewatch.se:DataProvider:{observation.DataProviderId}";

            //-----------------------------------------------------------------------------------------------------------
            // Dates
            //-----------------------------------------------------------------------------------------------------------
            observation.ReportedDate = new DateTime(Random.Next(1970, 2019), Random.Next(1, 13), Random.Next(1, 29)).ToUniversalTime();
            observation.ObservationDateStart = new DateTime(Random.Next(1970, 2019), Random.Next(1, 13), Random.Next(1, 29)).ToUniversalTime();
            //observation.ObservationDateEnd = GetRandomObservationDateEnd(observation.ObservationDateStart);
            //observation.EventDate = CreateDateIntervalString(observation.ObservationDateStart, observation.ObservationDateEnd); // todo - Remove property and use only ObservationDateStart & ObservationDateEnd?
            //observation.EventTime = CreateTimeIntervalString(observation.ObservationDateStart, observation.ObservationDateEnd); // todo - Remove property and use only ObservationDateStart & ObservationDateEnd?
            //observation.Year = observation.ObservationDateEnd?.Year ?? observation.ObservationDateStart.Year;
            //observation.Month = observation.ObservationDateEnd?.Month ?? observation.ObservationDateStart.Month;
            //observation.Day = observation.ObservationDateEnd?.Day ?? observation.ObservationDateStart.Day;
            //observation.StartDayOfYear = observation.ObservationDateStart.DayOfYear;
            //observation.EndDayOfYear = observation.ObservationDateEnd?.DayOfYear ?? observation.ObservationDateStart.DayOfYear;
            observation.Modified = observation.ObservationDateEnd ?? observation.ObservationDateStart;
            //observation.VerbatimEventDate = null;

            //-----------------------------------------------------------------------------------------------------------
            // Coordinates
            //-----------------------------------------------------------------------------------------------------------
            observation.CoordinateX = Random.NextDouble() * 100;
            observation.CoordinateY = Random.NextDouble() * 100;
            observation.CoordinateUncertaintyInMeters = Random.Next(1, 5000);
            observation.DisturbanceRadius = Random.Next(0, 3001);
            observation.MaxAccuracyOrDisturbanceRadius = Math.Max(observation.CoordinateUncertaintyInMeters.GetValueOrDefault(0), observation.DisturbanceRadius);
            observation.DecimalLatitude = observation.CoordinateY;
            observation.DecimalLongitude = observation.CoordinateX;
            observation.Locality = GetRandomLocality();
            observation.Continent = "Europe";
            observation.Country = "Sweden";
            observation.CountryCode = "SE";
            observation.StateProvince = GetRandomStateProvince();
            observation.County = GetRandomCounty();
            observation.Municipality = GetRandomMunicipality();
            observation.Parish = GetRandomParish();
            //observation.CoordinatePrecision = null;
            //observation.CoordinateSystemWkt = null;
            //observation.GeodeticDatum = null;
            //observation.MaximumDepthInMeters = null;
            //observation.MinimumDepthInMeters = null;
            //observation.CoordinateX_RT90 = ;
            //observation.CoordinateY_RT90 = ;
            //observation.CoordinateX_SWEREF99TM = ;
            //observation.CoordinateY_SWEREF99TM = ;
            //observation.CoordinateX_WebMercator = ;
            //observation.CoordinateY_WebMercator = ;
            //observation.CountyIdByName = ;
            //observation.CountyPartIdByName = ;
            //observation.ProvinceIdByName = ;
            //observation.MunicipalityIdByName = ;
            //observation.ProvincePartIdByName = ;
            //observation.CountyIdByCoordinate = ;
            //observation.CountyPartIdByCoordinate = ;
            //observation.CountryPartIdByCoordinate = ;
            //observation.ProvinceIdByCoordinate = ;
            //observation.ProvincePartIdByCoordinate = ;
            //observation.MunicipalityIdByCoordinate = ;


            //-----------------------------------------------------------------------------------------------------------
            // Names
            //-----------------------------------------------------------------------------------------------------------
            observation.RecordedBy = PersonTestRepository.GetRandomPerson().FullName;
            observation.ReportedBy = PersonTestRepository.GetRandomPerson().FullName;

            //-----------------------------------------------------------------------------------------------------------
            // Occurrence
            //-----------------------------------------------------------------------------------------------------------
            observation.IsPublic = true;
            observation.Type = "Occurrence";
            observation.IdentificationVerificationStatus = GetRandomIdentificationVerificationStatus();
            //observation.ValidationStatus = null;
            observation.IndividualCount = GetRandomIndividualCount();
            observation.Sex = GetRandomSex();
            observation.Behavior = GetRandomBehavior();
            observation.EstablishmentMeans = GetRandomEstablishmentMeans();
            observation.LifeStage = GetRandomLifeStage();
            observation.BasisOfRecord = GetRandomBasisOfRecord();
            observation.OccurrenceStatus = GetRandomOccurrenceStatus();
            observation.OccurrenceRemarks = GetRandomOccurrenceRemarks();
            observation.Habitat = GetRandomHabitat();
            observation.InformationWithheld = "More information can be obtained from the Data Provider.";
            observation.BirdNestActivityId = GetRandomBirdNestActivityId();
            observation.ActivityId = GetRandomActivityId();
            observation.UncertainDetermination = false;
            observation.Quantity = null;
            observation.QuantityUnit = null;
            observation.IsNaturalOccurrence = true;
            observation.IsPositiveObservation = true;
            observation.IsNeverFoundObservation = false;
            observation.IsNotRediscoveredObservation = false;
            
            

            //-----------------------------------------------------------------------------------------------------------
            // Rights
            //-----------------------------------------------------------------------------------------------------------
            observation.Rights = GetRandomRights(); // todo - merge with AccessRights?
            observation.AccessRights = GetRandomAccessRights(); // todo - merge with Rights
            observation.RightsHolder = GetRandomRightsHolder();

            //-----------------------------------------------------------------------------------------------------------
            // Collection
            //-----------------------------------------------------------------------------------------------------------
            observation.InstitutionID = GetRandomInstitutionId();
            observation.InstitutionCode = GetRandomInstitutionCode();
            observation.CollectionCode = GetRandomCollectionCode();
            observation.OwnerInstitutionCode = GetRandomOwnerInstitutionCode();
            observation.Owner = GetRandomOwner();


            //-----------------------------------------------------------------------------------------------------------
            // Project
            //-----------------------------------------------------------------------------------------------------------
            observation.ProjectName = GetRandomProjectName();
            observation.ProjectDescription = GetRandomProjectDescription();

            //-----------------------------------------------------------------------------------------------------------
            // Other
            //-----------------------------------------------------------------------------------------------------------
            observation.Language = "Swedish";

            //-----------------------------------------------------------------------------------------------------------
            // Taxon
            //-----------------------------------------------------------------------------------------------------------
            observation.ScientificName = "Psophus stridulus";
            observation.VernacularName = "trumgräshoppa";
            observation.TaxonRank = "Species";
            observation.HigherClassification = "Biota;Animalia;Arthropoda;Hexapoda;Insecta;Orthoptera;Caelifera;Acrididae;Locustinae";
            observation.Kingdom = "Animalia";
            observation.Phylum = "Arthropoda";
            observation.Class = "Insecta";
            observation.Order = "Orthoptera";
            observation.Family = "Acrididae";
            observation.Genus = "Psophus";
            observation.TaxonID = "urn:lsid:dyntaxa.se:Taxon:101656";
            observation.TaxonConceptID = "urn:lsid:dyntaxa.se:Taxon:101656";
            observation.TaxonomicStatus = "?";
            observation.ScientificNameAuthorship = "(Linnaeus, 1758)";
            observation.ActionPlan = true;
            observation.ConservationRelevant = true;
            observation.Natura2000 = true;
            observation.ProtectedByLaw = true;
            observation.ProtectionLevel = 1;
            observation.RedlistCategory = "EN";
            observation.SwedishImmigrationHistory = "Spontan";
            observation.SwedishOccurrence = "Bofast och reproducerande";

            //observation.AcceptedNameUsage = null;
            //observation.AcceptedNameUsageID = null;
            //observation.InfraspecificEpithet = null;
            //observation.NameAccordingTo = null;
            //observation.NameAccordingToID = null;
            //observation.NamePublishedIn = null;
            //observation.NamePublishedInID = null;
            //observation.NamePublishedInYear = null;
            //observation.NomenclaturalCode = null;
            //observation.NomenclaturalStatus = null;
            //observation.OrganismGroup = null;
            //observation.OriginalNameUsage = null;
            //observation.OriginalNameUsageID = null;
            //observation.ParentNameUsage = null;
            //observation.ParentNameUsageID = null;
            //observation.ScientificNameAuthorship = null;
            //observation.ScientificNameID = null;
            //observation.SpecificEpithet = null;
            //observation.Subgenus = null;
            //observation.TaxonConceptID = null;
            //observation.TaxonConceptStatus = null;
            //observation.TaxonID = null;
            //observation.TaxonomicStatus = null;
            //observation.TaxonRemarks = null;
            //observation.TaxonSortOrder = 0;
            //observation.TaxonURL = null;
            //observation.VerbatimScientificName = null;
            //observation.VerbatimTaxonRank = null;



            //-----------------------------------------------------------------------------------------------------------
            // Not set properties
            //-----------------------------------------------------------------------------------------------------------
            //observation.SpeciesObservationURL = null;
            //observation.BibliographicCitation = null;
            //observation.CollectionCode = null;
            //observation.CollectionID = null;
            //observation.DataGeneralizations = null;
            //observation.DatasetID = null;
            //observation.DynamicProperties = null;
            //observation.InformationWithheld = null;
            //observation.InstitutionCode = null;
            //observation.InstitutionID = null;
            //observation.OwnerInstitutionCode = null;
            //observation.References = null;
            //observation.EventID = null;
            //observation.EventRemarks = null;
            //observation.FieldNotes = null;
            //observation.FieldNumber = null;
            //observation.SamplingEffort = null;
            //observation.SamplingProtocol = null;
            //observation.Bed = null;
            //observation.EarliestAgeOrLowestStage = null;
            //observation.EarliestEonOrLowestEonothem = null;
            //observation.EarliestEpochOrLowestSeries = null;
            //observation.EarliestEraOrLowestErathem = null;
            //observation.EarliestPeriodOrLowestSystem = null;
            //observation.Formation = null;
            //observation.GeologicalContextID = null;
            //observation.Group = null;
            //observation.HighestBiostratigraphicZone = null;
            //observation.LatestAgeOrHighestStage = null;
            //observation.LatestEonOrHighestEonothem = null;
            //observation.LatestEpochOrHighestSeries = null;
            //observation.LatestEraOrHighestErathem = null;
            //observation.LatestPeriodOrHighestSystem = null;
            //observation.LithostratigraphicTerms = null;
            //observation.LowestBiostratigraphicZone = null;
            //observation.Member = null;
            //observation.DateIdentified = null;
            //observation.IdentificationID = null;
            //observation.IdentificationQualifier = null;
            //observation.IdentificationReferences = null;
            //observation.IdentificationRemarks = null;
            //observation.IdentificationVerificationStatus = null;
            //observation.IdentifiedBy = null;
            //observation.TypeStatus = null;
            //observation.FootprintSpatialFit = null;
            //observation.FootprintSRS = null;
            //observation.FootprintWKT = null;
            //observation.GeoreferencedBy = null;
            //observation.GeoreferencedDate = null;
            //observation.GeoreferenceProtocol = null;
            //observation.GeoreferenceRemarks = null;
            //observation.GeoreferenceSources = null;
            //observation.GeoreferenceVerificationStatus = null;
            //observation.HigherGeography = null;
            //observation.HigherGeographyID = null;
            //observation.Island = null;
            //observation.IslandGroup = null;
            //observation.Locality = null;
            //observation.LocationAccordingTo = null;
            //observation.LocationId = null;
            //observation.LocationRemarks = null;
            //observation.LocationURL = null;
            //observation.MaximumDistanceAboveSurfaceInMeters = null;
            //observation.MaximumElevationInMeters = null;
            //observation.MinimumDistanceAboveSurfaceInMeters = null;
            //observation.MinimumElevationInMeters = null;
            //observation.PointRadiusSpatialFit = null;
            //observation.VerbatimCoordinates = null;
            //observation.VerbatimCoordinateSystem = null;
            //observation.VerbatimDepth = null;
            //observation.VerbatimElevation = null;
            //observation.VerbatimLatitude = null;
            //observation.VerbatimLocality = null;
            //observation.VerbatimLongitude = null;
            //observation.VerbatimSRS = null;
            //observation.WaterBody = null;
            //observation.AssociatedMedia = null;
            //observation.AssociatedOccurrences = null;
            //observation.AssociatedReferences = null;
            //observation.AssociatedSequences = null;
            //observation.AssociatedTaxa = null;
            //observation.Disposition = null;
            //observation.IndividualID = null;
            //observation.OccurrenceID = null;
            //observation.OccurrenceRemarks = null;
            //observation.OccurrenceStatus = null;
            //observation.OccurrenceURL = null;
            //observation.OtherCatalogNumbers = null;
            //observation.Preparations = null;
            //observation.PreviousIdentifications = null;
            //observation.RecordNumber = null;
            //observation.ReproductiveCondition = null;
            //observation.Substrate = null;
            //observation.ProjectCategory = null;
            //observation.ProjectEndDate = null;
            //observation.ProjectID = null;
            //observation.ProjectOwner = null;
            //observation.ProjectStartDate = null;
            //observation.ProjectURL = null;
            //observation.SurveyMethod = null;

            return observation;
        }

        private static string GetOccurrenceId(int dataProviderId, string catalogNumber)
        {
            string dataProviderName = dataProviders[dataProviderId - 1];
            return $"urn:lsid:{dataProviderName}.se:Sighting:{catalogNumber}";
        }

        private static string GetDatasetName(int dataProviderId, string catalogNumber)
        {
            string dataProviderName = dataProviders[dataProviderId - 1];
            return dataProviderName;
        }

        private static string CreateDateIntervalString(DateTime date1, DateTime? date2)
        {
            if (!date2.HasValue)
            {
                return date1.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
            }

            return string.Format(
                "{0}/{1}",
                date1.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture),
                date2.Value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture));
        }

        private static string CreateTimeIntervalString(DateTime date1, DateTime? date2)
        {
            if (!date2.HasValue)
            {
                return date1.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture);
            }

            return string.Format(
                "{0}/{1}",
                date1.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture),
                date2.Value.ToUniversalTime().ToString("HH:mm:ssK", CultureInfo.InvariantCulture));
        }

        private static int? GetRandomActivityId()
        {
            int?[] values =
            {
                null,
                0,
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                8,
                9,
                10
            };

            return values[Random.Next(0, values.Length)];
        }

        private static int? GetRandomBirdNestActivityId()
        {
            int?[] values =
            {
                null,
                0,
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                8,
                9,
                10
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomInstitutionId()
        {
            string[] values =
            {
                null,
                "urn:lsid:artdata.slu.se:organization:307",
                "urn:lsid:artdata.slu.se:organization:328",
                "urn:lsid:artdata.slu.se:organization:268",
                "urn:lsid:artdata.slu.se:organization:296",
                "urn:lsid:artdata.slu.se:organization:137",
                "urn:lsid:artdata.slu.se:organization:4",
                "urn:lsid:artdata.slu.se:organization:340",
                "urn:lsid:artdata.slu.se:organization:252",
                "urn:lsid:artdata.slu.se:organization:157",
                "urn:lsid:artdata.slu.se:organization:388",

            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomOwnerInstitutionCode()
        {
            string[] values =
            {
                null,
                "Länsstyrelsen Kalmar",
                "Jämtlands botaniska sällskap",
                "Hallands rapportkommitté",
                "Floraväkteri-Örebro län",
                "Floraväkteri-Södermanlands län",
                "Östra Smålands rapportkommitté",
                "Floraväkteri-Dalsland O-län",
                "Angarngruppen",
                "Entomologiska Föreningen i Östergötland",
                "Skogsstyrelsen - Värmland",
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomProjectDescription(int percentNulls = 90)
        {
            if (Random.Next(1, 101) <= percentNulls)
            {
                return null;
            }

            string[] values =
            {
                "All bottenfauna i Hallands län som Medins eller Ekologgruppen samlat in som ett led i kalkeffektuppföljningen, recipienkontroll eller andra projekt, År 2011 matades fynd tom 2010 in, dock ej rödlistade artereftersom artdatabanken redan hade matat in dessa i arbetet med nya rödlistan våren 2010. År 2016 matades fynd 2011-2015 in.",
                "20th Nordic Mycological Congress in Visby, Gotland 25/9 - 1/10 2011.",
                "2011-2013 samt 2016 var fönster och markfällor (färgskålar; blå, vit och gul) utplacerade i ekoparken.",
                "20-punkters vinterfågelräkning på Mörlundaslätten i Hultsfredskommun, Småland. Har inventerats sedan 1997",
                "Aktivt eftersök av förvildade växter på tippar, utkastplatser, ruderatmarker och i anslutning till planteringar i tätorter och trädgårdar",
                "Andersson, H. 1999: Rödlistade eller sällsynta evertebrater knutna till ihåliga, murkna eller savande träd samt trädsvampar I Lunds stad. Ent. Tidskr. 120 (4): 169-183",
                "Ljusfälla 12 V 6W  med lysrör som är uppställd under natten och vittjas på morgonen. Fjärilarnas artbestäms och vid behov fotograferas, sedan släpps de levande. Fällan har märket Watkins and Doncaster, E7585  6W Heath trap. Dimensions: 30x30x59cm high.",
                "Arter observerade från Lilla Sjöhagen",
                "Arter observerade under svampplockningar.",
                "Alla observationer i Högalidsparken, Södermalm",
            };

            return values[Random.Next(0, values.Length)];
        }


        private static string GetRandomProjectName()
        {
            string[] values =
            {
                null,
                "Artportalen",
                "RingedBirds",
                "General",
                "Observationsdatabasen",
                "Lund-Botaniska/Zoologiska museet",
                "SMTP_INV",
                "OHN",
                "Stockholm-Naturhistoriska riksmuseet",
                "Limnodata HB",
                "Göteborgs naturhistoriska museum (GNM)",
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomCollectionCode()
        {
            string[] values =
            {
                null,
                "Artportalen",
                "RingedBirds",
                "General",
                "Observationsdatabasen",
                "Lund-Botaniska/Zoologiska museet",
                "SMTP_INV",
                "OHN",
                "Stockholm-Naturhistoriska riksmuseet",
                "Limnodata HB",
                "Göteborgs naturhistoriska museum (GNM)",
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomInstitutionCode()
        {
            string[] values =
            {
                null,
                "ArtDatabanken",
                "NRM",
                "Lunds Botaniska förening",
                "Botaniska sällskapet i Stockholm",
                "LD",
                "Skogsstyrelsen",
                "Östergötlands flora",
                "Sveriges mykologiska förening",
                "Fungus Info",
                "Länsstyrelsen Östergötland",

            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomParish()
        {
            string[] values =
            {
                null,
                "Sydöland",
                "Skanör-Falsterbo",
                "Ösmo-Torö",
                "Hoburg",
                "Husby-Sjutolft",
                "Onsala",
                "Dalarö-Ornö-Utö",
                "Varberg",
                "Sävar-Holmön",
                "Hållnäs-Österlövsta"
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomIndividualCount()
        {
            var rndVal = Random.Next(0, 100);
            if (rndVal > 20)
            {
                return (rndVal - 20).ToString();
            }

            return null;
        }

        private static string GetRandomIdentificationVerificationStatus()
        {
            string[] values =
            {
                null,
                "Approved based on image, sound or video recording",
                "Description required (for the regional records committee)",
                "Report treated regional(for National Rarities Committee)",
                "Unvalidated",
                "Approved based on reporters documentation",
                "Need not be validated",
                "Description required (for the regional records committee)",
                "Approved. Specimen checked by validator.",
                "Approved based on reporters rarity form",
                "Approved based on image, sound or video recording",
                "Validation requested",
                "Description required (for the National Rarities Committee)",
                "Dialogue at reporter"
            };

            return values[Random.Next(0, values.Length)];
        }

        private static DateTime? GetRandomObservationDateEnd(in DateTime observationDateStart)
        {
            if (Random.Next(1, 3) == 1)
            {
                return observationDateStart.AddHours(Random.Next(0, 8)).ToUniversalTime();
            }

            return null;
        }

        private static string GetRandomRightsHolder()
        {
            string[] values =
            {
                "Flora Bohuslän",
                "Botaniska Sektionen I Uppsala",
                "Hallands Flora Floraväktarna",
                "Länsstyrelsen Östergötland",
                "Länsstyrelsen Norrbotten",
                "Nicki Lilltroll",
                "Romeo Olsson",
                "Tord Yvel",
                "Kalle Ninja",
                "Ted Borg",
                "Nils Thynell",
                "Bill Åkesson",
                "Bull Åkesson",
                "Peter Kvist",
                "Pelle Bengtsson",
                "Jan Ekström"
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomRights()
        {
            string[] values =
            {
                null,
                "Free usage",
                "FreeUsage",
                "http://creativecommons.org/publicdomain/zero/1.0/legalcode",
                "Not for public ",
                "Not for public usage",
                "Not for public use"
            };

            return values[Random.Next(0, values.Length)];
        }

        public static string GetRandomAccessRights()
        {
            string[] values =
            {
                null,
                "Free usage",
                "FreeUsage",
                "Not for public ",
                "Not for public use"
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomOwner()
        {
            string[] values =
            {
                null,
                "Länsstyrelsen Värmland",
                "Länsstyrelsen Norrbotten",
                "Lars Broberg",
                "Charlotte Wigermo",
                "Fredrik  Ström",
                "Joakim Ekman",
                "Charles Farrell",
                "Björn Merkell",
                "Östergötlands flora",
                "Hans-Erik Wanntorp",
                "Allan Högberg",
                "Ornitologiska Klubben Eskilstuna",
                "Lars-Göran Lindgren",
                "Gotlands rapportkommitté",
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomHabitat()
        {
            string[] values =
            {
                null,
                "Granskog med ngt bok. Blåbär.",
                "Bergsluttning mot S vid sjön. Asp, tall, gran, bjö",
                "Svackor med al, ask, viden vid sjön. Gräs, utåt va",
                "Skog av gran, tall, björk, asp, ask, hassel i SV-s",
                "Barrskog med björk, ung hassel i svacka på åsen. M",
                "Glest, uthugget bestånd av gran, alm, sälg, nypons",
                "Granskog med asp, björk i ONO-sluttning. Bärris, m",
                "Lundvegetation i blockrik mullsluttning mot V. Ek,",
                "Högvuxen al-björkstrandskog med frodig markvegetat",
                "Hällterräng. I sprickdalar asp, björk, en, nyponsn",
                "Barrskog med asp, björk, lind i stenig terräng med",
                "Lunddäld med al, björk, gran, idegran, olvon. Örte",
                "Grovblockig NV-sluttning med enstaka rönn, al, en.",
                "Ekskog med insl. av asp, rönn, körsbär, gran, i sl",
                "Tall-granskog i blockströdd-stenig terräng. Inslag",
                "Lågt lövträdsbestånd i tämligen låg klippbrant mot",
                "Hedblandskog med gran, tall, björk, rönn, enstaka ",
                "Täml. tät granskog i ngt blockigN-luta. Insl. av a",
                "Mossigt blockras mot NO. Delvis glest stående björ"
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomMunicipality()
        {
            string[] values =
            {
                "Hallstahammar",
                "Ljusdal",
                "Kiruna",
                "Hagfors",
                "Vindeln",
                "Sundsvall",
                "Rättvik",
                "Dals-Ed",
                "Hylte",
                "Nybro",
                "Skövde",
                "Hörby",
                "Tranås",
                "Oskarshamn",
                "Sölvesborg",
                "Ockelbo",
                "Partille",
                "Lysekil",
                "Eda",
                "Vingåker",
                "Sunne",
                "Malung",
                "Åstorp",
                "Svedala",
                "Emmaboda",
                "Lomma",
                "Alvesta",
                "Lidingö",
                "Täby",
                "Askersund",
                "Nykvarn",
                "Borgholm",
                "Norberg",
                "Sollefteå",
                "Halmstad",
                "Sotenäs",
                "Trelleborg",
                "Nacka",
                "Falköping",
                "Kalmar",
                "Markaryd",
                "Ängelholm",
                "Tranemo",
                "Töreboda",
                "Ljusnarsberg",
                "Arvika",
                "Strömsund",
                "Skellefteå",
                "Motala",
                "Mölndal",
                "Kungälv",
                "Tanum",
                "Huddinge",
                "Högsby",
                "Vaggeryd",
                "Håbo",
                "Värnamo",
                "Sävsjö",
                "Lidköping",
                "Nynäshamn",
                "Järfälla",
                "Vallentuna",
                "Sigtuna",
                "Flen",
                "Köping",
                "Bollebygd",
                "Upplands Väsby",
                "Övertorne",
                "Karlskrona",
                "Bollnäs",
                "Söderhamn",
                "Filipstad",
                "Krokom",
                "Kramfors",
                "Södertälje",
                "Säffle",
                "Bengtsfors",
                "Höör",
                "Hässleholm",
                "Kristianstad"
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomCounty()
        {
            string[] values =
            {
                null,
                "Norrbotten",
                "Östergötland",
                "Hallands l",
                "Västra Götalands Län",
                "Jönköpings Län",
                "Lappland",
                "Kronobergs l",
                "Västar Götaland",
                "Hallnad",
                "Västra Götaland",
                "Gävleborg",
                "Dalarna",
                "Dalarnas l",
                "Blekinge",
                "Östergötlands",
                "Södermanlands",
                "Västerbottens",
                "Örebro Län",
                "Dalarnas",
                "Gävleborgs",
                "Jämtlands",
                "Gotland",
                "Bohusl",
                "Torne lappmark",
                "Örebro",
                "Skåne",
                "Västmanlands",
                "Kronoberg",
                "Värmlands Län",
                "Dalarnas Län",
                "Västerbottens Län",
                "Medelpad",
                "Uppsala",
                "Jönköpings",
                "Stockholm",
                "Jönköping",
                "Kronobergs Län",
                "Lycksele lappmark",
                "Kalmar",
                "Skåne Län",
                "Norrbottens Län",
                "Stockholms l",
                "Blekinge län",
                "Värmlands",
                "Västernorrland",
                "Skåne, Sweden",
                "Kalmar l",
                "Kronobergs",
                "Västerbotten",
                "Gävleborgs Län",
                "Norrbottens l",
                "Uppsala l",
                "Gotska Sand",
                "Västra Göraland",
                "Östergötlan",
                "Jämtland",
                "Norrbottens",
                "Västmanland",
                "Hallands Län",
                "Jämtlands Län",
                "Pite lappmark",
                "Västra Götalnad",
                "Gotlands",
                "Hallands",
                "Värmland",
                "Västmanlands Län",
                "Östergötlands Län",
                "Dalsland",
                "Västra Götalands",
                "Södermanland",
                "Stockholms Län",
                "Södermanlands Län",
                "Uppsala Län",
                "Västernorrlands Län",
                "Gotlands Län",
                "Stockholms",
                "Västernorrlands",
                "Halland",
                "Kalmar Län",
                "Uppland",
                "Lule lappmark",
                "Väster Götland",

            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomStateProvince()
        {
            string[] values =
            {
                "Östergötland",
                "Norrbotten",
                "Hälsingland",
                "Skagerrak",
                "Östersjön",
                "Blekinge",
                "Kattegatt",
                "Värmland",
                "Ångermanland",
                "Dalsland",
                "Södermanland",
                "Uppland",
                "Västergötland",
                "Lule lappmark",
                "Halland",
                "Jämtland",
                "Åsele lappmark",
                "Dalarna",
                "Västegötland",
                "Närke",
                "Gästrikland",
                "Västerbotten",
                "Västragötaland",
                "Västmanland",
                "Pite lappmark",
                "Bohuslän",
                "Torne lappmark",
                "Gotland",
                "Skåne",
                "Medelpad",
                "Härjedalen",
                "Södermanland / Uppland",
                "Uppland/Södermanland",
                "Öland",
                "Lycksele lappmark",
                "Småland",
                "Stockholm",
                "Bottenhavet",
                "Boshuslän"
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomLocality()
        {
            string[] values =
            {
                " Gullbäcken",
                " Gårdby österut",
                " Gärdsmark",
                " Hårbäcksravinen",
                " Kullen",
                " Korpilompilo myrgöl",
                " Kappelshamn",
                " Lunnarp 4:3; HA70-64",
                " Marma skjutfält, Rälsmålsbanan",
                " Moskogen",
                " Mörby SV",
                " Sundsby, Källdalen",
                " SV-branten 1, Unna Tuki, Padjelanta NP",
                " Visby, Söderport",
                " Weberöd"
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomOccurrenceRemarks()
        {
            string[] values =
            {
                "",
                "Nedanför sandåsen vid Norrtull",
                "Nära stranden",
                "Vid kyrkan",
                "Stubbe",
                "Igenväxande kärr i naturbetesmark i åssluttning (Onslundaåsen). Även långbensgroda. Mycket önskvärt att återställa vattnet till fungerande lekplats. Mycket fina landhabitat i näromgivningarna.",
                "O sluttningen; Liggande stam",
                "Naturreservat",
                "V om lunden",
                "O om gården",
                "Översvämning i vall strax väster om gården. Torkade ut efterhand under sommaren."
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomOccurrenceStatus()
        {
            string[] values =
            {
                null,
                "Absent",
                "Present",
                "Not rediscovered"
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomBasisOfRecord()
        {
            string[] values =
            {
                "HumanObservation",
                "MachineObservation",
                "PreservedSpecimen",
                "FossilSpecimen",
                "LivingSpecimen",
                "MaterialSample",
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomLifeStage()
        {
            string[] values =
            {
                null,
                "Imago/adult",
                "2K+",
                "Död",
                "Ant. lev:181,död:5",
                "with schistidies",
                "Ant. lev:79,död:0",
                "1K",
                "With capsule"
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomSex()
        {
            string[] values =
            {
                null,
                "Male",
                "In pair",
                "Female",
                "Female coloured",
                "Worker"
            };

            return values[Random.Next(0, values.Length)];
        }

        private static string GetRandomEstablishmentMeans()
        {
            string[] establishmentMeans =
            {
                null,
                "present",
                "doubtful",
                "absent",
                "irregular"
            };

            return establishmentMeans[Random.Next(0, establishmentMeans.Length)];
        }

        public static List<ProcessedDwcObservation> CreateRandomObservations(int numberOfObservations)
        {
            List<ProcessedDwcObservation> speciesObservations = new List<ProcessedDwcObservation>(numberOfObservations);
            for (int i = 0; i < numberOfObservations; i++)
            {
                speciesObservations.Add(CreateRandomObservation());
            }

            return speciesObservations;
        }

        private static string GetRandomBehavior()
        {
            string[] behaviors =
            {
                "mating/mating ceremonies",
                "Visit occupied nest",
                "Egg laying",
                "Migrating N",
                "Incubating",
                "Distraction display",
                "Ill",
                "Permanent territory",
                "Migrating S",
                "Territory outside breeding",
                "Slow movement, long on the surface",
                "Dead, collided with fence",
                "Dead, collided with wind mill",
                "killed by electricity",
                "Running/crawling",
                "Mating/mating ceremonies)",
                "killed by predator",
                "Visiting occupied nest",
                "Brood on eggs",
                "Visit possiblenest?",
                "Leavings of prey/food",
                "Hibernate",
                "Trace of oestrous female",
                "hibernating",
                "Migrating NE",
                "migrating E",
                "Breeding failed",
                "In mating suit",
                "Egg shells",
                "Mine",
                "Approaching boat",
                "display / song outside breeding",
                "Steady course, regular diving",
                "Nest with egg",
                "Individually marked",
                "migration attempt",
                "NULL",
                "Forage",
                "Migrating",
                "Roadkill",
                "Fragment",
                "Flying by",
                "gall",
                "dead, collided with lighthouse",
                "Dead due to disease/starvation",
                "Dead, collided with power line",
                "Display/song",
                "In water/swimming",
                "Migrating fish",
                "Agitated behaviour",
                "dead, collided with aeroplane",
                "Winter habitat",
                "dispute between males",
                "fresh gnaw",
                "Jump",
                "Play",
                "Remains of prey/food",
                "gnaw",
                "Pair in suitable habitat",
                "nest with chick heard",
                "Nest with pulli heard",
                "Tracks from climbing",
                "Found dead",
                "Recently used nest",
                "Feces",
                "Fresh feces",
                "Fresh faeces",
                "Shot/killed",
                "Unsteady course/irregualr diving",
                "Migrating SW",
                "Recently fledged young",
                "Digging",
                "Fresh trace",
                "Shed skin",
                "Sick",
                "Faeces",
                "Breeding ground or offspring",
                "nest building",
                "Remains of fur or hair",
                "Staging",
                "In nesting habitat",
                "Carrying food for young",
                "Flying",
                "Territorial",
                "Carrying faecal sac",
                "Display",
                "Road kill",
                "Old nest",
                "Call",
                "Free-flying",
                "cocoon",
                "Female with offspring",
                "In breeding colours",
                "Dead by disease/starve",
                "Trace",
                "ringed",
                "drowned in fishing net",
                "Foraging",
                "stationary",
                "visit possible nest?",
                "flying overhead",
                "Brood patch",
                "Dead, collided with window",
                "slow movement, long periods on the surface",
                "Fight between males",
                "Slow movement, long periods on the ",
                "Dormant",
                "Migrating W",
                "Migrating NW",
                "Migrating SE",
                "Pregnant female",
                "Trace of adult with offspring"
            };

            return behaviors[Random.Next(0, behaviors.Length)];
        }
    }
}
