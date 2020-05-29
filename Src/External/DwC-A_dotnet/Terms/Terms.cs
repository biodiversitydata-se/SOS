using System.Text.RegularExpressions;

namespace DwC_A.Terms
{
    public static class Terms
    {
        public static string acceptedNameUsage = "http://rs.tdwg.org/dwc/terms/acceptedNameUsage";
        public static string acceptedNameUsageID = "http://rs.tdwg.org/dwc/terms/acceptedNameUsageID";
        public static string accessRights = "http://purl.org/dc/terms/accessRights";
        public static string associatedMedia = "http://rs.tdwg.org/dwc/terms/associatedMedia";
        public static string associatedOccurrences = "http://rs.tdwg.org/dwc/terms/associatedOccurrences";
        public static string associatedOrganisms = "http://rs.tdwg.org/dwc/terms/associatedOrganisms";
        public static string associatedReferences = "http://rs.tdwg.org/dwc/terms/associatedReferences";
        public static string associatedSequences = "http://rs.tdwg.org/dwc/terms/associatedSequences";
        public static string associatedTaxa = "http://rs.tdwg.org/dwc/terms/associatedTaxa";
        public static string basisOfRecord = "http://rs.tdwg.org/dwc/terms/basisOfRecord";
        public static string bed = "http://rs.tdwg.org/dwc/terms/bed";
        public static string behavior = "http://rs.tdwg.org/dwc/terms/behavior";
        public static string bibliographicCitation = "http://purl.org/dc/terms/bibliographicCitation";
        public static string catalogNumber = "http://rs.tdwg.org/dwc/terms/catalogNumber";
        public static string @class = "http://rs.tdwg.org/dwc/terms/class";
        public static string collectionCode = "http://rs.tdwg.org/dwc/terms/collectionCode";
        public static string collectionID = "http://rs.tdwg.org/dwc/terms/collectionID";
        public static string continent = "http://rs.tdwg.org/dwc/terms/continent";
        public static string coordinatePrecision = "http://rs.tdwg.org/dwc/terms/coordinatePrecision";

        public static string coordinateUncertaintyInMeters =
            "http://rs.tdwg.org/dwc/terms/coordinateUncertaintyInMeters";

        public static string country = "http://rs.tdwg.org/dwc/terms/country";
        public static string countryCode = "http://rs.tdwg.org/dwc/terms/countryCode";
        public static string county = "http://rs.tdwg.org/dwc/terms/county";
        public static string dataGeneralizations = "http://rs.tdwg.org/dwc/terms/dataGeneralizations";
        public static string datasetID = "http://rs.tdwg.org/dwc/terms/datasetID";
        public static string datasetName = "http://rs.tdwg.org/dwc/terms/datasetName";
        public static string dateIdentified = "http://rs.tdwg.org/dwc/terms/dateIdentified";
        public static string day = "http://rs.tdwg.org/dwc/terms/day";
        public static string decimalLatitude = "http://rs.tdwg.org/dwc/terms/decimalLatitude";
        public static string decimalLongitude = "http://rs.tdwg.org/dwc/terms/decimalLongitude";
        public static string disposition = "http://rs.tdwg.org/dwc/terms/disposition";
        public static string dynamicProperties = "http://rs.tdwg.org/dwc/terms/dynamicProperties";
        public static string earliestAgeOrLowestStage = "http://rs.tdwg.org/dwc/terms/earliestAgeOrLowestStage";
        public static string earliestEonOrLowestEonothem = "http://rs.tdwg.org/dwc/terms/earliestEonOrLowestEonothem";
        public static string earliestEpochOrLowestSeries = "http://rs.tdwg.org/dwc/terms/earliestEpochOrLowestSeries";
        public static string earliestEraOrLowestErathem = "http://rs.tdwg.org/dwc/terms/earliestEraOrLowestErathem";
        public static string earliestGeochronologicalEra = "http://rs.tdwg.org/dwc/iri/earliestGeochronologicalEra";
        public static string earliestPeriodOrLowestSystem = "http://rs.tdwg.org/dwc/terms/earliestPeriodOrLowestSystem";
        public static string endDayOfYear = "http://rs.tdwg.org/dwc/terms/endDayOfYear";
        public static string establishmentMeans = "http://rs.tdwg.org/dwc/terms/establishmentMeans";
        public static string Event = "http://rs.tdwg.org/dwc/terms/Event";
        public static string eventDate = "http://rs.tdwg.org/dwc/terms/eventDate";
        public static string eventID = "http://rs.tdwg.org/dwc/terms/eventID";
        public static string eventRemarks = "http://rs.tdwg.org/dwc/terms/eventRemarks";
        public static string eventTime = "http://rs.tdwg.org/dwc/terms/eventTime";
        public static string family = "http://rs.tdwg.org/dwc/terms/family";
        public static string fieldNotes = "http://rs.tdwg.org/dwc/terms/fieldNotes";
        public static string fieldNumber = "http://rs.tdwg.org/dwc/terms/fieldNumber";
        public static string footprintSpatialFit = "http://rs.tdwg.org/dwc/terms/footprintSpatialFit";
        public static string footprintSRS = "http://rs.tdwg.org/dwc/terms/footprintSRS";
        public static string footprintWKT = "http://rs.tdwg.org/dwc/terms/footprintWKT";
        public static string formation = "http://rs.tdwg.org/dwc/terms/formation";
        public static string FossilSpecimen = "http://rs.tdwg.org/dwc/terms/FossilSpecimen";
        public static string fromLithostratigraphicUnit = "http://rs.tdwg.org/dwc/iri/fromLithostratigraphicUnit";
        public static string genus = "http://rs.tdwg.org/dwc/terms/genus";
        public static string geodeticDatum = "http://rs.tdwg.org/dwc/terms/geodeticDatum";
        public static string GeologicalContext = "http://rs.tdwg.org/dwc/terms/GeologicalContext";
        public static string geologicalContextID = "http://rs.tdwg.org/dwc/terms/geologicalContextID";
        public static string georeferencedBy = "http://rs.tdwg.org/dwc/terms/georeferencedBy";
        public static string georeferencedDate = "http://rs.tdwg.org/dwc/terms/georeferencedDate";
        public static string georeferenceProtocol = "http://rs.tdwg.org/dwc/terms/georeferenceProtocol";
        public static string georeferenceRemarks = "http://rs.tdwg.org/dwc/terms/georeferenceRemarks";
        public static string georeferenceSources = "http://rs.tdwg.org/dwc/terms/georeferenceSources";

        public static string georeferenceVerificationStatus =
            "http://rs.tdwg.org/dwc/terms/georeferenceVerificationStatus";

        public static string group = "http://rs.tdwg.org/dwc/terms/group";
        public static string habitat = "http://rs.tdwg.org/dwc/terms/habitat";
        public static string higherClassification = "http://rs.tdwg.org/dwc/terms/higherClassification";
        public static string higherGeography = "http://rs.tdwg.org/dwc/terms/higherGeography";
        public static string higherGeographyID = "http://rs.tdwg.org/dwc/terms/higherGeographyID";
        public static string highestBiostratigraphicZone = "http://rs.tdwg.org/dwc/terms/highestBiostratigraphicZone";
        public static string HumanObservation = "http://rs.tdwg.org/dwc/terms/HumanObservation";
        public static string Identification = "http://rs.tdwg.org/dwc/terms/Identification";
        public static string identificationID = "http://rs.tdwg.org/dwc/terms/identificationID";
        public static string identificationQualifier = "http://rs.tdwg.org/dwc/terms/identificationQualifier";
        public static string identificationReferences = "http://rs.tdwg.org/dwc/terms/identificationReferences";
        public static string identificationRemarks = "http://rs.tdwg.org/dwc/terms/identificationRemarks";

        public static string identificationVerificationStatus =
            "http://rs.tdwg.org/dwc/terms/identificationVerificationStatus";

        public static string identifiedBy = "http://rs.tdwg.org/dwc/terms/identifiedBy";
        public static string inCollection = "http://rs.tdwg.org/dwc/iri/inCollection";
        public static string inDataset = "http://rs.tdwg.org/dwc/iri/inDataset";
        public static string inDescribedPlace = "http://rs.tdwg.org/dwc/iri/inDescribedPlace";
        public static string individualCount = "http://rs.tdwg.org/dwc/terms/individualCount";
        public static string informationWithheld = "http://rs.tdwg.org/dwc/terms/informationWithheld";
        public static string infraspecificEpithet = "http://rs.tdwg.org/dwc/terms/infraspecificEpithet";
        public static string institutionCode = "http://rs.tdwg.org/dwc/terms/institutionCode";
        public static string institutionID = "http://rs.tdwg.org/dwc/terms/institutionID";
        public static string island = "http://rs.tdwg.org/dwc/terms/island";
        public static string islandGroup = "http://rs.tdwg.org/dwc/terms/islandGroup";
        public static string kingdom = "http://rs.tdwg.org/dwc/terms/kingdom";
        public static string language = "http://purl.org/dc/terms/language";
        public static string latestAgeOrHighestStage = "http://rs.tdwg.org/dwc/terms/latestAgeOrHighestStage";
        public static string latestEonOrHighestEonothem = "http://rs.tdwg.org/dwc/terms/latestEonOrHighestEonothem";
        public static string latestEpochOrHighestSeries = "http://rs.tdwg.org/dwc/terms/latestEpochOrHighestSeries";
        public static string latestEraOrHighestErathem = "http://rs.tdwg.org/dwc/terms/latestEraOrHighestErathem";
        public static string latestGeochronologicalEra = "http://rs.tdwg.org/dwc/iri/latestGeochronologicalEra";
        public static string latestPeriodOrHighestSystem = "http://rs.tdwg.org/dwc/terms/latestPeriodOrHighestSystem";
        public static string license = "http://purl.org/dc/terms/license";
        public static string lifeStage = "http://rs.tdwg.org/dwc/terms/lifeStage";
        public static string lithostratigraphicTerms = "http://rs.tdwg.org/dwc/terms/lithostratigraphicTerms";
        public static string LivingSpecimen = "http://rs.tdwg.org/dwc/terms/LivingSpecimen";
        public static string locality = "http://rs.tdwg.org/dwc/terms/locality";
        public static string Location = "http://purl.org/dc/terms/Location";
        public static string locationAccordingTo = "http://rs.tdwg.org/dwc/terms/locationAccordingTo";
        public static string locationID = "http://rs.tdwg.org/dwc/terms/locationID";
        public static string locationRemarks = "http://rs.tdwg.org/dwc/terms/locationRemarks";
        public static string lowestBiostratigraphicZone = "http://rs.tdwg.org/dwc/terms/lowestBiostratigraphicZone";
        public static string MachineObservation = "http://rs.tdwg.org/dwc/terms/MachineObservation";
        public static string MaterialSample = "http://rs.tdwg.org/dwc/terms/MaterialSample";
        public static string materialSampleID = "http://rs.tdwg.org/dwc/terms/materialSampleID";
        public static string maximumDepthInMeters = "http://rs.tdwg.org/dwc/terms/maximumDepthInMeters";

        public static string maximumDistanceAboveSurfaceInMeters =
            "http://rs.tdwg.org/dwc/terms/maximumDistanceAboveSurfaceInMeters";

        public static string maximumElevationInMeters = "http://rs.tdwg.org/dwc/terms/maximumElevationInMeters";
        public static string measurementAccuracy = "http://rs.tdwg.org/dwc/terms/measurementAccuracy";
        public static string measurementDeterminedBy = "http://rs.tdwg.org/dwc/terms/measurementDeterminedBy";
        public static string measurementDeterminedDate = "http://rs.tdwg.org/dwc/terms/measurementDeterminedDate";
        public static string measurementID = "http://rs.tdwg.org/dwc/terms/measurementID";
        public static string measurementMethod = "http://rs.tdwg.org/dwc/terms/measurementMethod";
        public static string MeasurementOrFact = "http://rs.tdwg.org/dwc/terms/MeasurementOrFact";
        public static string measurementRemarks = "http://rs.tdwg.org/dwc/terms/measurementRemarks";
        public static string measurementType = "http://rs.tdwg.org/dwc/terms/measurementType";
        public static string measurementUnit = "http://rs.tdwg.org/dwc/terms/measurementUnit";
        public static string measurementValue = "http://rs.tdwg.org/dwc/terms/measurementValue";
        public static string member = "http://rs.tdwg.org/dwc/terms/member";
        public static string minimumDepthInMeters = "http://rs.tdwg.org/dwc/terms/minimumDepthInMeters";

        public static string minimumDistanceAboveSurfaceInMeters =
            "http://rs.tdwg.org/dwc/terms/minimumDistanceAboveSurfaceInMeters";

        public static string minimumElevationInMeters = "http://rs.tdwg.org/dwc/terms/minimumElevationInMeters";
        public static string modified = "http://purl.org/dc/terms/modified";
        public static string month = "http://rs.tdwg.org/dwc/terms/month";
        public static string municipality = "http://rs.tdwg.org/dwc/terms/municipality";
        public static string nameAccordingTo = "http://rs.tdwg.org/dwc/terms/nameAccordingTo";
        public static string nameAccordingToID = "http://rs.tdwg.org/dwc/terms/nameAccordingToID";
        public static string namePublishedIn = "http://rs.tdwg.org/dwc/terms/namePublishedIn";
        public static string namePublishedInID = "http://rs.tdwg.org/dwc/terms/namePublishedInID";
        public static string namePublishedInYear = "http://rs.tdwg.org/dwc/terms/namePublishedInYear";
        public static string nomenclaturalCode = "http://rs.tdwg.org/dwc/terms/nomenclaturalCode";
        public static string nomenclaturalStatus = "http://rs.tdwg.org/dwc/terms/nomenclaturalStatus";
        public static string Occurrence = "http://rs.tdwg.org/dwc/terms/Occurrence";
        public static string occurrenceID = "http://rs.tdwg.org/dwc/terms/occurrenceID";
        public static string occurrenceRemarks = "http://rs.tdwg.org/dwc/terms/occurrenceRemarks";
        public static string occurrenceStatus = "http://rs.tdwg.org/dwc/terms/occurrenceStatus";
        public static string order = "http://rs.tdwg.org/dwc/terms/order";
        public static string Organism = "http://rs.tdwg.org/dwc/terms/Organism";
        public static string organismID = "http://rs.tdwg.org/dwc/terms/organismID";
        public static string organismName = "http://rs.tdwg.org/dwc/terms/organismName";
        public static string organismQuantity = "http://rs.tdwg.org/dwc/terms/organismQuantity";
        public static string organismQuantityType = "http://rs.tdwg.org/dwc/terms/organismQuantityType";
        public static string organismRemarks = "http://rs.tdwg.org/dwc/terms/organismRemarks";
        public static string organismScope = "http://rs.tdwg.org/dwc/terms/organismScope";
        public static string originalNameUsage = "http://rs.tdwg.org/dwc/terms/originalNameUsage";
        public static string originalNameUsageID = "http://rs.tdwg.org/dwc/terms/originalNameUsageID";
        public static string otherCatalogNumbers = "http://rs.tdwg.org/dwc/terms/otherCatalogNumbers";
        public static string ownerInstitutionCode = "http://rs.tdwg.org/dwc/terms/ownerInstitutionCode";
        public static string parentEventID = "http://rs.tdwg.org/dwc/terms/parentEventID";
        public static string parentNameUsage = "http://rs.tdwg.org/dwc/terms/parentNameUsage";
        public static string parentNameUsageID = "http://rs.tdwg.org/dwc/terms/parentNameUsageID";
        public static string phylum = "http://rs.tdwg.org/dwc/terms/phylum";
        public static string pointRadiusSpatialFit = "http://rs.tdwg.org/dwc/terms/pointRadiusSpatialFit";
        public static string preparations = "http://rs.tdwg.org/dwc/terms/preparations";
        public static string PreservedSpecimen = "http://rs.tdwg.org/dwc/terms/PreservedSpecimen";
        public static string previousIdentifications = "http://rs.tdwg.org/dwc/terms/previousIdentifications";
        public static string recordedBy = "http://rs.tdwg.org/dwc/terms/recordedBy";
        public static string recordNumber = "http://rs.tdwg.org/dwc/terms/recordNumber";
        public static string references = "http://purl.org/dc/terms/references";
        public static string relatedResourceID = "http://rs.tdwg.org/dwc/terms/relatedResourceID";
        public static string relationshipAccordingTo = "http://rs.tdwg.org/dwc/terms/relationshipAccordingTo";
        public static string relationshipEstablishedDate = "http://rs.tdwg.org/dwc/terms/relationshipEstablishedDate";
        public static string relationshipOfResource = "http://rs.tdwg.org/dwc/terms/relationshipOfResource";
        public static string relationshipRemarks = "http://rs.tdwg.org/dwc/terms/relationshipRemarks";
        public static string reproductiveCondition = "http://rs.tdwg.org/dwc/terms/reproductiveCondition";
        public static string resourceID = "http://rs.tdwg.org/dwc/terms/resourceID";
        public static string ResourceRelationship = "http://rs.tdwg.org/dwc/terms/ResourceRelationship";
        public static string resourceRelationshipID = "http://rs.tdwg.org/dwc/terms/resourceRelationshipID";
        public static string rightsHolder = "http://purl.org/dc/terms/rightsHolder";
        public static string sampleSizeUnit = "http://rs.tdwg.org/dwc/terms/sampleSizeUnit";
        public static string sampleSizeValue = "http://rs.tdwg.org/dwc/terms/sampleSizeValue";
        public static string samplingEffort = "http://rs.tdwg.org/dwc/terms/samplingEffort";
        public static string samplingProtocol = "http://rs.tdwg.org/dwc/terms/samplingProtocol";
        public static string scientificName = "http://rs.tdwg.org/dwc/terms/scientificName";
        public static string scientificNameAuthorship = "http://rs.tdwg.org/dwc/terms/scientificNameAuthorship";
        public static string scientificNameID = "http://rs.tdwg.org/dwc/terms/scientificNameID";
        public static string sex = "http://rs.tdwg.org/dwc/terms/sex";
        public static string specificEpithet = "http://rs.tdwg.org/dwc/terms/specificEpithet";
        public static string startDayOfYear = "http://rs.tdwg.org/dwc/terms/startDayOfYear";
        public static string stateProvince = "http://rs.tdwg.org/dwc/terms/stateProvince";
        public static string subgenus = "http://rs.tdwg.org/dwc/terms/subgenus";
        public static string Taxon = "http://rs.tdwg.org/dwc/terms/Taxon";
        public static string taxonConceptID = "http://rs.tdwg.org/dwc/terms/taxonConceptID";
        public static string taxonID = "http://rs.tdwg.org/dwc/terms/taxonID";
        public static string taxonomicStatus = "http://rs.tdwg.org/dwc/terms/taxonomicStatus";
        public static string taxonRank = "http://rs.tdwg.org/dwc/terms/taxonRank";
        public static string taxonRemarks = "http://rs.tdwg.org/dwc/terms/taxonRemarks";
        public static string toTaxon = "http://rs.tdwg.org/dwc/iri/toTaxon";
        public static string type = "http://purl.org/dc/terms/type";
        public static string typeStatus = "http://rs.tdwg.org/dwc/terms/typeStatus";
        public static string UseWithIRI = "http://rs.tdwg.org/dwc/terms/attributes/UseWithIRI";
        public static string verbatimCoordinates = "http://rs.tdwg.org/dwc/terms/verbatimCoordinates";
        public static string verbatimCoordinateSystem = "http://rs.tdwg.org/dwc/terms/verbatimCoordinateSystem";
        public static string verbatimDepth = "http://rs.tdwg.org/dwc/terms/verbatimDepth";
        public static string verbatimElevation = "http://rs.tdwg.org/dwc/terms/verbatimElevation";
        public static string verbatimEventDate = "http://rs.tdwg.org/dwc/terms/verbatimEventDate";
        public static string verbatimLatitude = "http://rs.tdwg.org/dwc/terms/verbatimLatitude";
        public static string verbatimLocality = "http://rs.tdwg.org/dwc/terms/verbatimLocality";
        public static string verbatimLongitude = "http://rs.tdwg.org/dwc/terms/verbatimLongitude";
        public static string verbatimSRS = "http://rs.tdwg.org/dwc/terms/verbatimSRS";
        public static string verbatimTaxonRank = "http://rs.tdwg.org/dwc/terms/verbatimTaxonRank";
        public static string vernacularName = "http://rs.tdwg.org/dwc/terms/vernacularName";
        public static string waterBody = "http://rs.tdwg.org/dwc/terms/waterBody";
        public static string year = "http://rs.tdwg.org/dwc/terms/year";

        // ExtendedMeasurementOrFact
        public static string measurementTypeID = "http://rs.iobis.org/obis/terms/measurementTypeID";
        public static string measurementValueID = "http://rs.iobis.org/obis/terms/measurementValueID";
        public static string measurementUnitID = "http://rs.iobis.org/obis/terms/measurementUnitID";

        // Simple Multimedia
        public static string format = "http://purl.org/dc/terms/format";
        public static string identifier = "http://purl.org/dc/terms/identifier";
        public static string title = "http://purl.org/dc/terms/title";
        public static string description = "http://purl.org/dc/terms/description";
        public static string created = "http://purl.org/dc/terms/created";
        public static string creator = "http://purl.org/dc/terms/creator";
        public static string contributor = "http://purl.org/dc/terms/contributor";
        public static string publisher = "http://purl.org/dc/terms/publisher";
        public static string audience = "http://purl.org/dc/terms/audience";
        public static string source = "	http://purl.org/dc/terms/source";


        public static string ShortName(string term)
        {
            if (term == null)
            {
                return null;
            }

            var regex = new Regex("[^/]+$");
            var match = regex.Match(term);
            return string.IsNullOrEmpty(match.Value) ? term : match.Value;
        }
    }
}