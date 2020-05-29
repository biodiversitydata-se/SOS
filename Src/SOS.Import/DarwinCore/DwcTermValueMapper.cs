using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore
{
    public static class DwcTermValueMapper
    {
        public static void MapValueByTerm(
            DwcObservationVerbatim observation,
            string term,
            string val)
        {
            switch (term)
            {
                case "id":
                    observation.RecordId = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/acceptedNameUsage":
                    observation.AcceptedNameUsage = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/acceptedNameUsageID":
                    observation.AcceptedNameUsageID = val;
                    break;
                case "http://purl.org/dc/terms/accessRights":
                    observation.AccessRights = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/associatedMedia":
                    observation.AssociatedMedia = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/associatedOccurrences":
                    observation.AssociatedOccurrences = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/associatedOrganisms":
                    observation.AssociatedOrganisms = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/associatedReferences":
                    observation.AssociatedReferences = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/associatedSequences":
                    observation.AssociatedSequences = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/associatedTaxa":
                    observation.AssociatedTaxa = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/basisOfRecord":
                    observation.BasisOfRecord = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/bed":
                    observation.Bed = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/behavior":
                    observation.Behavior = val;
                    break;
                case "http://purl.org/dc/terms/bibliographicCitation":
                    observation.BibliographicCitation = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/catalogNumber":
                    observation.CatalogNumber = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/class":
                    observation.Class = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/collectionCode":
                    observation.CollectionCode = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/collectionID":
                    observation.CollectionID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/continent":
                    observation.Continent = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/coordinatePrecision":
                    observation.CoordinatePrecision = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/coordinateUncertaintyInMeters":
                    observation.CoordinateUncertaintyInMeters = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/country":
                    observation.Country = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/countryCode":
                    observation.CountryCode = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/county":
                    observation.County = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/dataGeneralizations":
                    observation.DataGeneralizations = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/datasetID":
                    observation.DatasetID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/datasetName":
                    observation.DatasetName = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/dateIdentified":
                    observation.DateIdentified = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/day":
                    observation.Day = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/decimalLatitude":
                    observation.DecimalLatitude = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/decimalLongitude":
                    observation.DecimalLongitude = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/disposition":
                    observation.Disposition = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/dynamicProperties":
                    observation.DynamicProperties = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/earliestAgeOrLowestStage":
                    observation.EarliestAgeOrLowestStage = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/earliestEonOrLowestEonothem":
                    observation.EarliestEonOrLowestEonothem = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/earliestEpochOrLowestSeries":
                    observation.EarliestEpochOrLowestSeries = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/earliestEraOrLowestErathem":
                    observation.EarliestEraOrLowestErathem = val;
                    break;
                case "http://rs.tdwg.org/dwc/iri/earliestGeochronologicalEra":
                    observation.EarliestGeochronologicalEra = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/earliestPeriodOrLowestSystem":
                    observation.EarliestPeriodOrLowestSystem = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/endDayOfYear":
                    observation.EndDayOfYear = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/establishmentMeans":
                    observation.EstablishmentMeans = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/eventDate":
                    observation.EventDate = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/eventID":
                    observation.EventID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/eventRemarks":
                    observation.EventRemarks = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/eventTime":
                    observation.EventTime = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/family":
                    observation.Family = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/fieldNotes":
                    observation.FieldNotes = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/fieldNumber":
                    observation.FieldNumber = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/footprintSpatialFit":
                    observation.FootprintSpatialFit = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/footprintSRS":
                    observation.FootprintSRS = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/footprintWKT":
                    observation.FootprintWKT = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/formation":
                    observation.Formation = val;
                    break;
                case "http://rs.tdwg.org/dwc/iri/fromLithostratigraphicUnit":
                    observation.FromLithostratigraphicUnit = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/genus":
                    observation.Genus = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/geodeticDatum":
                    observation.GeodeticDatum = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/geologicalContextID":
                    observation.GeologicalContextID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/georeferencedBy":
                    observation.GeoreferencedBy = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/georeferencedDate":
                    observation.GeoreferencedDate = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/georeferenceProtocol":
                    observation.GeoreferenceProtocol = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/georeferenceRemarks":
                    observation.GeoreferenceRemarks = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/georeferenceSources":
                    observation.GeoreferenceSources = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/georeferenceVerificationStatus":
                    observation.GeoreferenceVerificationStatus = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/group":
                    observation.Group = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/habitat":
                    observation.Habitat = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/higherClassification":
                    observation.HigherClassification = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/higherGeography":
                    observation.HigherGeography = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/higherGeographyID":
                    observation.HigherGeographyID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/highestBiostratigraphicZone":
                    observation.HighestBiostratigraphicZone = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/identificationID":
                    observation.IdentificationID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/identificationQualifier":
                    observation.IdentificationQualifier = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/identificationReferences":
                    observation.IdentificationReferences = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/identificationRemarks":
                    observation.IdentificationRemarks = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/identificationVerificationStatus":
                    observation.IdentificationVerificationStatus = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/identifiedBy":
                    observation.IdentifiedBy = val;
                    break;
                case "http://rs.tdwg.org/dwc/iri/inCollection":
                    observation.InCollection = val;
                    break;
                case "http://rs.tdwg.org/dwc/iri/inDataset":
                    observation.InDataset = val;
                    break;
                case "http://rs.tdwg.org/dwc/iri/inDescribedPlace":
                    observation.InDescribedPlace = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/individualCount":
                    observation.IndividualCount = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/informationWithheld":
                    observation.InformationWithheld = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/infraspecificEpithet":
                    observation.InfraspecificEpithet = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/institutionCode":
                    observation.InstitutionCode = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/institutionID":
                    observation.InstitutionID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/island":
                    observation.Island = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/islandGroup":
                    observation.IslandGroup = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/kingdom":
                    observation.Kingdom = val;
                    break;
                case "http://purl.org/dc/terms/language":
                    observation.Language = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/latestAgeOrHighestStage":
                    observation.LatestAgeOrHighestStage = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/latestEonOrHighestEonothem":
                    observation.LatestEonOrHighestEonothem = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/latestEpochOrHighestSeries":
                    observation.LatestEpochOrHighestSeries = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/latestEraOrHighestErathem":
                    observation.LatestEraOrHighestErathem = val;
                    break;
                case "http://rs.tdwg.org/dwc/iri/latestGeochronologicalEra":
                    observation.LatestGeochronologicalEra = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/latestPeriodOrHighestSystem":
                    observation.LatestPeriodOrHighestSystem = val;
                    break;
                case "http://purl.org/dc/terms/license":
                    observation.License = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/lifeStage":
                    observation.LifeStage = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/lithostratigraphicTerms":
                    observation.LithostratigraphicTerms = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/locality":
                    observation.Locality = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/locationAccordingTo":
                    observation.LocationAccordingTo = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/locationID":
                    observation.LocationID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/locationRemarks":
                    observation.LocationRemarks = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/lowestBiostratigraphicZone":
                    observation.LowestBiostratigraphicZone = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/materialSampleID":
                    observation.MaterialSampleID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/maximumDepthInMeters":
                    observation.MaximumDepthInMeters = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/maximumDistanceAboveSurfaceInMeters":
                    observation.MaximumDistanceAboveSurfaceInMeters = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/maximumElevationInMeters":
                    observation.MaximumElevationInMeters = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/member":
                    observation.Member = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/minimumDepthInMeters":
                    observation.MinimumDepthInMeters = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/minimumDistanceAboveSurfaceInMeters":
                    observation.MinimumDistanceAboveSurfaceInMeters = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/minimumElevationInMeters":
                    observation.MinimumElevationInMeters = val;
                    break;
                case "http://purl.org/dc/terms/modified":
                    observation.Modified = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/month":
                    observation.Month = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/municipality":
                    observation.Municipality = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/nameAccordingTo":
                    observation.NameAccordingTo = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/nameAccordingToID":
                    observation.NameAccordingToID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/namePublishedIn":
                    observation.NamePublishedIn = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/namePublishedInID":
                    observation.NamePublishedInID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/namePublishedInYear":
                    observation.NamePublishedInYear = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/nomenclaturalCode":
                    observation.NomenclaturalCode = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/nomenclaturalStatus":
                    observation.NomenclaturalStatus = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/occurrenceID":
                    observation.OccurrenceID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/occurrenceRemarks":
                    observation.OccurrenceRemarks = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/occurrenceStatus":
                    observation.OccurrenceStatus = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/order":
                    observation.Order = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/organismID":
                    observation.OrganismID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/organismName":
                    observation.OrganismName = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/organismQuantity":
                    observation.OrganismQuantity = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/organismQuantityType":
                    observation.OrganismQuantityType = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/organismRemarks":
                    observation.OrganismRemarks = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/organismScope":
                    observation.OrganismScope = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/originalNameUsage":
                    observation.OriginalNameUsage = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/originalNameUsageID":
                    observation.OriginalNameUsageID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/otherCatalogNumbers":
                    observation.OtherCatalogNumbers = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/ownerInstitutionCode":
                    observation.OwnerInstitutionCode = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/parentEventID":
                    observation.ParentEventID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/parentNameUsage":
                    observation.ParentNameUsage = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/parentNameUsageID":
                    observation.ParentNameUsageID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/phylum":
                    observation.Phylum = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/pointRadiusSpatialFit":
                    observation.PointRadiusSpatialFit = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/preparations":
                    observation.Preparations = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/previousIdentifications":
                    observation.PreviousIdentifications = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/recordedBy":
                    observation.RecordedBy = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/recordNumber":
                    observation.RecordNumber = val;
                    break;
                case "http://purl.org/dc/terms/references":
                    observation.References = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/reproductiveCondition":
                    observation.ReproductiveCondition = val;
                    break;
                case "http://purl.org/dc/terms/rightsHolder":
                    observation.RightsHolder = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/sampleSizeUnit":
                    observation.SampleSizeUnit = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/sampleSizeValue":
                    observation.SampleSizeValue = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/samplingEffort":
                    observation.SamplingEffort = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/samplingProtocol":
                    observation.SamplingProtocol = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/scientificName":
                    observation.ScientificName = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/scientificNameAuthorship":
                    observation.ScientificNameAuthorship = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/scientificNameID":
                    observation.ScientificNameID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/sex":
                    observation.Sex = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/specificEpithet":
                    observation.SpecificEpithet = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/startDayOfYear":
                    observation.StartDayOfYear = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/stateProvince":
                    observation.StateProvince = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/subgenus":
                    observation.Subgenus = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/taxonConceptID":
                    observation.TaxonConceptID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/taxonID":
                    observation.TaxonID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/taxonomicStatus":
                    observation.TaxonomicStatus = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/taxonRank":
                    observation.TaxonRank = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/taxonRemarks":
                    observation.TaxonRemarks = val;
                    break;
                case "http://rs.tdwg.org/dwc/iri/toTaxon":
                    observation.ToTaxon = val;
                    break;
                case "http://purl.org/dc/terms/type":
                    observation.Type = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/typeStatus":
                    observation.TypeStatus = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimCoordinates":
                    observation.VerbatimCoordinates = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimCoordinateSystem":
                    observation.VerbatimCoordinateSystem = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimDepth":
                    observation.VerbatimDepth = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimElevation":
                    observation.VerbatimElevation = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimEventDate":
                    observation.VerbatimEventDate = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimLatitude":
                    observation.VerbatimLatitude = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimLocality":
                    observation.VerbatimLocality = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimLongitude":
                    observation.VerbatimLongitude = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimSRS":
                    observation.VerbatimSRS = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimTaxonRank":
                    observation.VerbatimTaxonRank = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/vernacularName":
                    observation.VernacularName = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/waterBody":
                    observation.WaterBody = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/year":
                    observation.Year = val;
                    break;

                // todo - more mappings?
                //case "http://rs.tdwg.org/dwc/terms/measurementAccuracy":
                //    observation.MeasurementAccuracy = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/measurementDeterminedBy":
                //    observation.MeasurementDeterminedBy = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/measurementDeterminedDate":
                //    observation.MeasurementDeterminedDate = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/measurementID":
                //    observation.MeasurementID = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/measurementMethod":
                //    observation.MeasurementMethod = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/MeasurementOrFact":
                //    observation.MeasurementOrFact = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/measurementRemarks":
                //    observation.MeasurementRemarks = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/measurementType":
                //    observation.MeasurementType = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/measurementUnit":
                //    observation.MeasurementUnit = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/measurementValue":
                //    observation.MeasurementValue = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/relatedResourceID":
                //    observation.RelatedResourceID = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/relationshipAccordingTo":
                //    observation.RelationshipAccordingTo = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/relationshipEstablishedDate":
                //    observation.RelationshipEstablishedDate = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/relationshipOfResource":
                //    observation.RelationshipOfResource = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/relationshipRemarks":
                //    observation.RelationshipRemarks = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/resourceID":
                //    observation.ResourceID = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/ResourceRelationship":
                //    observation.ResourceRelationship = val;
                //    break;
                //case "http://rs.tdwg.org/dwc/terms/resourceRelationshipID":
                //    observation.ResourceRelationshipID = val;
                //    break;
            }
        }

        public static void MapValueByTerm(
            DwcExtendedMeasurementOrFact emofItem,
            string term,
            string val)
        {
            switch (term)
            {
                // todo - should we handle "id"?
                //case "id":
                //    emofItem.Id = val;
                //    break;
                case "http://rs.tdwg.org/dwc/terms/measurementID":
                    emofItem.MeasurementID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/occurrenceID":
                    emofItem.OccurrenceID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementType":
                    emofItem.MeasurementType = val;
                    break;
                case "http://rs.iobis.org/obis/terms/measurementTypeID":
                    emofItem.MeasurementTypeID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementValue":
                    emofItem.MeasurementValue = val;
                    break;
                case "http://rs.iobis.org/obis/terms/measurementValueID":
                    emofItem.MeasurementValueID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementAccuracy":
                    emofItem.MeasurementAccuracy = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementUnit":
                    emofItem.MeasurementUnit = val;
                    break;
                case "http://rs.iobis.org/obis/terms/measurementUnitID":
                    emofItem.MeasurementUnitID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementDeterminedDate":
                    emofItem.MeasurementDeterminedDate = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementDeterminedBy":
                    emofItem.MeasurementDeterminedBy = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementMethod":
                    emofItem.MeasurementMethod = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementRemarks":
                    emofItem.MeasurementRemarks = val;
                    break;
            }
        }

        public static void MapValueByTerm(DwcEvent dwcEvent, string term, string val)
        {
            switch (term)
            {
                case "id":
                    dwcEvent.RecordId = val;
                    break;
                case "http://purl.org/dc/terms/accessRights":
                    dwcEvent.AccessRights = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/basisOfRecord":
                    dwcEvent.BasisOfRecord = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/bed":
                    dwcEvent.Bed = val;
                    break;
                case "http://purl.org/dc/terms/bibliographicCitation":
                    dwcEvent.BibliographicCitation = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/collectionCode":
                    dwcEvent.CollectionCode = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/collectionID":
                    dwcEvent.CollectionID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/continent":
                    dwcEvent.Continent = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/coordinatePrecision":
                    dwcEvent.CoordinatePrecision = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/coordinateUncertaintyInMeters":
                    dwcEvent.CoordinateUncertaintyInMeters = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/country":
                    dwcEvent.Country = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/countryCode":
                    dwcEvent.CountryCode = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/county":
                    dwcEvent.County = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/dataGeneralizations":
                    dwcEvent.DataGeneralizations = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/datasetID":
                    dwcEvent.DatasetID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/datasetName":
                    dwcEvent.DatasetName = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/day":
                    dwcEvent.Day = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/decimalLatitude":
                    dwcEvent.DecimalLatitude = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/decimalLongitude":
                    dwcEvent.DecimalLongitude = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/dynamicProperties":
                    dwcEvent.DynamicProperties = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/earliestAgeOrLowestStage":
                    dwcEvent.EarliestAgeOrLowestStage = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/earliestEonOrLowestEonothem":
                    dwcEvent.EarliestEonOrLowestEonothem = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/earliestEpochOrLowestSeries":
                    dwcEvent.EarliestEpochOrLowestSeries = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/earliestEraOrLowestErathem":
                    dwcEvent.EarliestEraOrLowestErathem = val;
                    break;
                case "http://rs.tdwg.org/dwc/iri/earliestGeochronologicalEra":
                    dwcEvent.EarliestGeochronologicalEra = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/earliestPeriodOrLowestSystem":
                    dwcEvent.EarliestPeriodOrLowestSystem = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/endDayOfYear":
                    dwcEvent.EndDayOfYear = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/eventDate":
                    dwcEvent.EventDate = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/eventID":
                    dwcEvent.EventID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/eventRemarks":
                    dwcEvent.EventRemarks = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/eventTime":
                    dwcEvent.EventTime = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/fieldNotes":
                    dwcEvent.FieldNotes = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/fieldNumber":
                    dwcEvent.FieldNumber = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/footprintSpatialFit":
                    dwcEvent.FootprintSpatialFit = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/footprintSRS":
                    dwcEvent.FootprintSRS = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/footprintWKT":
                    dwcEvent.FootprintWKT = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/formation":
                    dwcEvent.Formation = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/geodeticDatum":
                    dwcEvent.GeodeticDatum = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/geologicalContextID":
                    dwcEvent.GeologicalContextID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/georeferencedBy":
                    dwcEvent.GeoreferencedBy = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/georeferencedDate":
                    dwcEvent.GeoreferencedDate = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/georeferenceProtocol":
                    dwcEvent.GeoreferenceProtocol = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/georeferenceRemarks":
                    dwcEvent.GeoreferenceRemarks = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/georeferenceSources":
                    dwcEvent.GeoreferenceSources = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/georeferenceVerificationStatus":
                    dwcEvent.GeoreferenceVerificationStatus = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/group":
                    dwcEvent.Group = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/habitat":
                    dwcEvent.Habitat = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/higherGeography":
                    dwcEvent.HigherGeography = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/higherGeographyID":
                    dwcEvent.HigherGeographyID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/highestBiostratigraphicZone":
                    dwcEvent.HighestBiostratigraphicZone = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/informationWithheld":
                    dwcEvent.InformationWithheld = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/institutionCode":
                    dwcEvent.InstitutionCode = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/institutionID":
                    dwcEvent.InstitutionID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/island":
                    dwcEvent.Island = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/islandGroup":
                    dwcEvent.IslandGroup = val;
                    break;
                case "http://purl.org/dc/terms/language":
                    dwcEvent.Language = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/latestAgeOrHighestStage":
                    dwcEvent.LatestAgeOrHighestStage = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/latestEonOrHighestEonothem":
                    dwcEvent.LatestEonOrHighestEonothem = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/latestEpochOrHighestSeries":
                    dwcEvent.LatestEpochOrHighestSeries = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/latestEraOrHighestErathem":
                    dwcEvent.LatestEraOrHighestErathem = val;
                    break;
                case "http://rs.tdwg.org/dwc/iri/latestGeochronologicalEra":
                    dwcEvent.LatestGeochronologicalEra = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/latestPeriodOrHighestSystem":
                    dwcEvent.LatestPeriodOrHighestSystem = val;
                    break;
                case "http://purl.org/dc/terms/license":
                    dwcEvent.License = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/lithostratigraphicTerms":
                    dwcEvent.LithostratigraphicTerms = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/locality":
                    dwcEvent.Locality = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/locationAccordingTo":
                    dwcEvent.LocationAccordingTo = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/locationID":
                    dwcEvent.LocationID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/locationRemarks":
                    dwcEvent.LocationRemarks = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/lowestBiostratigraphicZone":
                    dwcEvent.LowestBiostratigraphicZone = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/maximumDepthInMeters":
                    dwcEvent.MaximumDepthInMeters = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/maximumDistanceAboveSurfaceInMeters":
                    dwcEvent.MaximumDistanceAboveSurfaceInMeters = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/maximumElevationInMeters":
                    dwcEvent.MaximumElevationInMeters = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/member":
                    dwcEvent.Member = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/minimumDepthInMeters":
                    dwcEvent.MinimumDepthInMeters = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/minimumDistanceAboveSurfaceInMeters":
                    dwcEvent.MinimumDistanceAboveSurfaceInMeters = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/minimumElevationInMeters":
                    dwcEvent.MinimumElevationInMeters = val;
                    break;
                case "http://purl.org/dc/terms/modified":
                    dwcEvent.Modified = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/month":
                    dwcEvent.Month = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/municipality":
                    dwcEvent.Municipality = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/ownerInstitutionCode":
                    dwcEvent.OwnerInstitutionCode = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/parentEventID":
                    dwcEvent.ParentEventID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/pointRadiusSpatialFit":
                    dwcEvent.PointRadiusSpatialFit = val;
                    break;
                case "http://purl.org/dc/terms/references":
                    dwcEvent.References = val;
                    break;
                case "http://purl.org/dc/terms/rightsHolder":
                    dwcEvent.RightsHolder = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/sampleSizeUnit":
                    dwcEvent.SampleSizeUnit = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/sampleSizeValue":
                    dwcEvent.SampleSizeValue = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/samplingEffort":
                    dwcEvent.SamplingEffort = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/samplingProtocol":
                    dwcEvent.SamplingProtocol = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/startDayOfYear":
                    dwcEvent.StartDayOfYear = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/stateProvince":
                    dwcEvent.StateProvince = val;
                    break;
                case "http://purl.org/dc/terms/type":
                    dwcEvent.Type = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimCoordinates":
                    dwcEvent.VerbatimCoordinates = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimCoordinateSystem":
                    dwcEvent.VerbatimCoordinateSystem = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimDepth":
                    dwcEvent.VerbatimDepth = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimElevation":
                    dwcEvent.VerbatimElevation = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimEventDate":
                    dwcEvent.VerbatimEventDate = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimLatitude":
                    dwcEvent.VerbatimLatitude = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimLocality":
                    dwcEvent.VerbatimLocality = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimLongitude":
                    dwcEvent.VerbatimLongitude = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/verbatimSRS":
                    dwcEvent.VerbatimSRS = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/waterBody":
                    dwcEvent.WaterBody = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/year":
                    dwcEvent.Year = val;
                    break;
            }
        }

        public static void MapValueByTerm(DwcMeasurementOrFact mofItem, string term, string val)
        {
            switch (term)
            {
                // todo - should we handle "id"?
                //case "id":
                //    mofItem.Id = val;
                //    break;
                case "http://rs.tdwg.org/dwc/terms/measurementID":
                    mofItem.MeasurementID = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementType":
                    mofItem.MeasurementType = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementValue":
                    mofItem.MeasurementValue = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementAccuracy":
                    mofItem.MeasurementAccuracy = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementUnit":
                    mofItem.MeasurementUnit = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementDeterminedDate":
                    mofItem.MeasurementDeterminedDate = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementDeterminedBy":
                    mofItem.MeasurementDeterminedBy = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementMethod":
                    mofItem.MeasurementMethod = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/measurementRemarks":
                    mofItem.MeasurementRemarks = val;
                    break;
            }
        }

        public static void MapValueByTerm(DwcMultimedia item, string term, string val)
        {
            switch (term)
            {
                // todo - should we handle "id"?
                //case "id":
                //    mofItem.Id = val;
                //    break;
                case "http://purl.org/dc/terms/type":
                    item.Type = val;
                    break;
                case "http://purl.org/dc/terms/format":
                    item.Format = val;
                    break;
                case "http://purl.org/dc/terms/identifier":
                    item.Identifier = val;
                    break;
                case "http://purl.org/dc/terms/references":
                    item.References = val;
                    break;
                case "http://purl.org/dc/terms/title":
                    item.Title = val;
                    break;
                case "http://purl.org/dc/terms/description":
                    item.Description = val;
                    break;
                case "http://purl.org/dc/terms/created":
                    item.Created = val;
                    break;
                case "http://purl.org/dc/terms/creator":
                    item.Creator = val;
                    break;
                case "http://purl.org/dc/terms/contributor":
                    item.Contributor = val;
                    break;
                case "http://purl.org/dc/terms/publisher":
                    item.Publisher = val;
                    break;
                case "http://purl.org/dc/terms/audience":
                    item.Audience = val;
                    break;
                case "http://purl.org/dc/terms/source":
                    item.Source = val;
                    break;
                case "http://purl.org/dc/terms/license":
                    item.License = val;
                    break;
                case "http://purl.org/dc/terms/rightsHolder":
                    item.RightsHolder = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/datasetID":
                    item.DatasetID = val;
                    break;
            }
        }

        public static void MapValueByTerm(DwcAudubonMedia item, string term, string val)
        {
            switch (term)
            {
                case "http://purl.org/dc/terms/identifier":
                    item.Identifier = val;
                    break;
                case "http://purl.org/dc/elements/1.1/type":
                    item.TypeAc = val;
                    break;
                case "http://purl.org/dc/terms/type":
                    item.Type = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/subtypeLiteral":
                    item.SubtypeLiteral = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/subtype":
                    item.Subtype = val;
                    break;
                case "http://purl.org/dc/terms/title":
                    item.Title = val;
                    break;
                case "http://purl.org/dc/terms/modified":
                    item.Modified = val;
                    break;
                case "http://ns.adobe.com/xap/1.0/MetadataDate":
                    item.MetadataDate = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/metadataLanguageLiteral":
                    item.MetadataLanguageLiteral = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/metadataLanguage":
                    item.MetadataLanguage = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/providerManagedID":
                    item.ProviderManagedID = val;
                    break;
                case "http://ns.adobe.com/xap/1.0/Rating":
                    item.Rating = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/commenterLiteral":
                    item.CommenterLiteral = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/commenter":
                    item.Commenter = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/comments":
                    item.Comments = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/reviewerLiteral":
                    item.ReviewerLiteral = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/reviewer":
                    item.Reviewer = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/reviewerComments":
                    item.ReviewerComments = val;
                    break;
                case "http://purl.org/dc/terms/available":
                    item.Available = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/hasServiceAccessPoint":
                    item.HasServiceAccessPoint = val;
                    break;
                case "http://purl.org/dc/elements/1.1/rights":
                    item.RightsAc = val;
                    break;
                case "http://purl.org/dc/terms/rights":
                    item.Rights = val;
                    break;
                case "http://ns.adobe.com/xap/1.0/rights/Owner":
                    item.Owner = val;
                    break;
                case "http://ns.adobe.com/xap/1.0/rights/UsageTerms":
                    item.UsageTerms = val;
                    break;
                case "http://ns.adobe.com/xap/1.0/rights/WebStatement":
                    item.WebStatement = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/licenseLogoURL":
                    item.LicenseLogoURL = val;
                    break;
                case "http://ns.adobe.com/photoshop/1.0/Credit":
                    item.Credit = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/attributionLogoURL":
                    item.AttributionLogoURL = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/attributionLinkURL":
                    item.AttributionLinkURL = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/fundingAttribution":
                    item.FundingAttribution = val;
                    break;
                case "http://purl.org/dc/elements/1.1/source":
                    item.SourceAc = val;
                    break;
                case "http://purl.org/dc/terms/source":
                    item.Source = val;
                    break;
                case "http://purl.org/dc/elements/1.1/creator":
                    item.CreatorAc = val;
                    break;
                case "http://purl.org/dc/terms/creator":
                    item.Creator = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/providerLiteral":
                    item.ProviderLiteral = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/provider":
                    item.Provider = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/metadataCreatorLiteral":
                    item.MetadataCreatorLiteral = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/metadataCreator":
                    item.MetadataCreator = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/metadataProviderLiteral":
                    item.MetadataProviderLiteral = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/metadataProvider":
                    item.MetadataProvider = val;
                    break;
                case "http://purl.org/dc/terms/description":
                    item.Description = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/caption":
                    item.Caption = val;
                    break;
                case "http://purl.org/dc/elements/1.1/language":
                    item.LanguageAc = val;
                    break;
                case "http://purl.org/dc/terms/language":
                    item.Language = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/physicalSetting":
                    item.PhysicalSetting = val;
                    break;
                case "http://iptc.org/std/Iptc4xmpExt/2008-02-29/CVterm":
                    item.CVterm = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/subjectCategoryVocabulary":
                    item.SubjectCategoryVocabulary = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/tag":
                    item.Tag = val;
                    break;
                case "http://iptc.org/std/Iptc4xmpExt/2008-02-29/LocationShown":
                    item.LocationShown = val;
                    break;
                case "http://iptc.org/std/Iptc4xmpExt/2008-02-29/WorldRegion":
                    item.WorldRegion = val;
                    break;
                case "http://iptc.org/std/Iptc4xmpExt/2008-02-29/CountryCode":
                    item.CountryCode = val;
                    break;
                case "http://iptc.org/std/Iptc4xmpExt/2008-02-29/CountryName":
                    item.CountryName = val;
                    break;
                case "http://iptc.org/std/Iptc4xmpExt/2008-02-29/ProvinceState":
                    item.ProvinceState = val;
                    break;
                case "http://iptc.org/std/Iptc4xmpExt/2008-02-29/City":
                    item.City = val;
                    break;
                case "http://iptc.org/std/Iptc4xmpExt/2008-02-29/Sublocation":
                    item.Sublocation = val;
                    break;
                case "http://purl.org/dc/terms/temporal":
                    item.Temporal = val;
                    break;
                case "http://ns.adobe.com/xap/1.0/CreateDate":
                    item.CreateDate = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/timeOfDay":
                    item.TimeOfDay = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/taxonCoverage":
                    item.TaxonCoverage = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/scientificName":
                    item.ScientificName = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/identificationQualifier":
                    item.IdentificationQualifier = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/vernacularName":
                    item.VernacularName = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/nameAccordingTo":
                    item.NameAccordingTo = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/scientificNameID":
                    item.ScientificNameID = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/otherScientificName":
                    item.OtherScientificName = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/identifiedBy":
                    item.IdentifiedBy = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/dateIdentified":
                    item.DateIdentified = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/taxonCount":
                    item.TaxonCount = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/subjectPart":
                    item.SubjectPart = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/sex":
                    item.Sex = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/lifeStage":
                    item.LifeStage = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/subjectOrientation":
                    item.SubjectOrientation = val;
                    break;
                case "http://rs.tdwg.org/dwc/terms/preparations":
                    item.Preparations = val;
                    break;
                case "http://iptc.org/std/Iptc4xmpExt/2008-02-29/LocationCreated":
                    item.LocationCreated = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/digitizationDate":
                    item.DigitizationDate = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/captureDevice":
                    item.CaptureDevice = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/resourceCreationTechnique":
                    item.ResourceCreationTechnique = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/IDofContainingCollection":
                    item.IDofContainingCollection = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/relatedResourceID":
                    item.RelatedResourceID = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/providerID":
                    item.ProviderID = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/derivedFrom":
                    item.DerivedFrom = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/associatedSpecimenReference":
                    item.AssociatedSpecimenReference = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/associatedObservationReference":
                    item.AssociatedObservationReference = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/accessURI":
                    item.AccessURI = val;
                    break;
                case "http://purl.org/dc/elements/1.1/format":
                    item.FormatAc = val;
                    break;
                case "http://purl.org/dc/terms/format":
                    item.Format = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/variantLiteral":
                    item.VariantLiteral = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/variant":
                    item.Variant = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/variantDescription":
                    item.VariantDescription = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/furtherInformationURL":
                    item.FurtherInformationURL = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/licensingException":
                    item.LicensingException = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/serviceExpectation":
                    item.ServiceExpectation = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/hashFunction":
                    item.HashFunction = val;
                    break;
                case "http://rs.tdwg.org/ac/terms/hashValue":
                    item.HashValue = val;
                    break;
                case "http://ns.adobe.com/exif/1.0/PixelXDimension":
                    item.PixelXDimension = val;
                    break;
                case "http://ns.adobe.com/exif/1.0/PixelYDimension":
                    item.PixelYDimension = val;
                    break;
            }
        }
    }
}