using DwC_A.Terms;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.Harvesters.Observations
{
    public static class DwcMapper
    {
        public static void MapValueByTerm(
            DarwinCoreObservationVerbatim observation,
            string term, 
            string val)
        {
            switch (term)
            {
                // todo - should we handle "id"?
                //case "id":
                //    observation.Id = val;
                //    break;
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
    }
}