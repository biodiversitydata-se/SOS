using DwC_A;
using DwC_A.Terms;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.Harvesters.Observations.DarwinCore
{
    public class DarwinCoreObservationVerbatimFactory
    {
        public static DarwinCoreObservationVerbatim Create(IRow row)
        {
            var result = new DarwinCoreObservationVerbatim();

            //-----------------------------------------------------------------------------------------------------------
            // Record level
            //-----------------------------------------------------------------------------------------------------------
            result.AccessRights = row.GetValue(Terms.accessRights);
            result.BasisOfRecord = row.GetValue(Terms.basisOfRecord);
            result.BibliographicCitation = row.GetValue(Terms.bibliographicCitation);
            result.CollectionCode = row.GetValue(Terms.collectionCode);
            result.CollectionID = row.GetValue(Terms.collectionID);
            result.DataGeneralizations = row.GetValue(Terms.dataGeneralizations);
            result.DatasetID = row.GetValue(Terms.datasetID);
            result.DatasetName = row.GetValue(Terms.datasetName);
            result.DynamicProperties = row.GetValue(Terms.dynamicProperties);
            result.InformationWithheld = row.GetValue(Terms.informationWithheld);
            result.InstitutionCode = row.GetValue(Terms.institutionCode);
            result.InstitutionID = row.GetValue(Terms.institutionID);
            result.Language = row.GetValue(Terms.language);
            result.License = row.GetValue(Terms.license);
            result.Modified = row.GetValue(Terms.modified);
            result.OwnerInstitutionCode = row.GetValue(Terms.ownerInstitutionCode);
            result.References = row.GetValue(Terms.references);
            result.RightsHolder = row.GetValue(Terms.rightsHolder);
            result.Type = row.GetValue(Terms.type);

            //-----------------------------------------------------------------------------------------------------------
            // Event
            //-----------------------------------------------------------------------------------------------------------
            result.Day = row.GetValue(Terms.day);
            result.EndDayOfYear = row.GetValue(Terms.endDayOfYear);
            result.EventDate = row.GetValue(Terms.eventDate);
            result.EventID = row.GetValue(Terms.eventID);
            result.ParentEventID = row.GetValue(Terms.parentEventID);
            result.EventRemarks = row.GetValue(Terms.eventRemarks);
            result.EventTime = row.GetValue(Terms.eventTime);
            result.FieldNotes = row.GetValue(Terms.fieldNotes);
            result.FieldNumber = row.GetValue(Terms.fieldNumber);
            result.Habitat = row.GetValue(Terms.habitat);
            result.Month = row.GetValue(Terms.month);
            result.SampleSizeUnit = row.GetValue(Terms.sampleSizeUnit);
            result.SampleSizeValue = row.GetValue(Terms.sampleSizeValue);
            result.SamplingEffort = row.GetValue(Terms.samplingEffort);
            result.SamplingProtocol = row.GetValue(Terms.samplingProtocol);
            result.StartDayOfYear = row.GetValue(Terms.startDayOfYear);
            result.VerbatimEventDate = row.GetValue(Terms.verbatimEventDate);
            result.Year = row.GetValue(Terms.year);

            //-----------------------------------------------------------------------------------------------------------
            // GeologicalContext
            //-----------------------------------------------------------------------------------------------------------
            result.Bed = row.GetValue(Terms.bed);
            result.EarliestAgeOrLowestStage = row.GetValue(Terms.earliestAgeOrLowestStage);
            result.EarliestEonOrLowestEonothem = row.GetValue(Terms.earliestEonOrLowestEonothem);
            result.EarliestEpochOrLowestSeries = row.GetValue(Terms.earliestEpochOrLowestSeries);
            result.EarliestEraOrLowestErathem = row.GetValue(Terms.earliestEraOrLowestErathem);
            result.EarliestGeochronologicalEra = row.GetValue(Terms.earliestGeochronologicalEra);
            result.EarliestPeriodOrLowestSystem = row.GetValue(Terms.earliestPeriodOrLowestSystem);
            result.Formation = row.GetValue(Terms.formation);
            result.GeologicalContextID = row.GetValue(Terms.geologicalContextID);
            result.Group = row.GetValue(Terms.group);
            result.HighestBiostratigraphicZone = row.GetValue(Terms.highestBiostratigraphicZone);
            result.LatestAgeOrHighestStage = row.GetValue(Terms.latestAgeOrHighestStage);
            result.LatestEonOrHighestEonothem = row.GetValue(Terms.latestEonOrHighestEonothem);
            result.LatestEpochOrHighestSeries = row.GetValue(Terms.latestEpochOrHighestSeries);
            result.LatestEraOrHighestErathem = row.GetValue(Terms.latestEraOrHighestErathem);
            result.LatestGeochronologicalEra = row.GetValue(Terms.latestGeochronologicalEra);
            result.LatestPeriodOrHighestSystem = row.GetValue(Terms.latestPeriodOrHighestSystem);
            result.LithostratigraphicTerms = row.GetValue(Terms.lithostratigraphicTerms);
            result.LowestBiostratigraphicZone = row.GetValue(Terms.lowestBiostratigraphicZone);
            result.Member = row.GetValue(Terms.member);

            //-----------------------------------------------------------------------------------------------------------
            // Identification
            //-----------------------------------------------------------------------------------------------------------
            result.DateIdentified = row.GetValue(Terms.dateIdentified);
            result.IdentificationID = row.GetValue(Terms.identificationID);
            result.IdentificationQualifier = row.GetValue(Terms.identificationQualifier);
            result.IdentificationReferences = row.GetValue(Terms.identificationReferences);
            result.IdentificationRemarks = row.GetValue(Terms.identificationRemarks);
            result.IdentificationVerificationStatus = row.GetValue(Terms.identificationVerificationStatus);
            result.IdentifiedBy = row.GetValue(Terms.identifiedBy);
            result.TypeStatus = row.GetValue(Terms.typeStatus);

            //-----------------------------------------------------------------------------------------------------------
            // Location
            //-----------------------------------------------------------------------------------------------------------
            result.Continent = row.GetValue(Terms.continent);
            result.CoordinatePrecision = row.GetValue(Terms.coordinatePrecision);
            result.CoordinateUncertaintyInMeters = row.GetValue(Terms.coordinateUncertaintyInMeters);
            result.Country = row.GetValue(Terms.country);
            result.CountryCode = row.GetValue(Terms.countryCode);
            result.County = row.GetValue(Terms.county);
            result.DecimalLatitude = row.GetValue(Terms.decimalLatitude);
            result.DecimalLongitude = row.GetValue(Terms.decimalLongitude);
            result.FootprintSpatialFit = row.GetValue(Terms.footprintSpatialFit);
            result.FootprintSRS = row.GetValue(Terms.footprintSRS);
            result.FootprintWKT = row.GetValue(Terms.footprintWKT);
            result.GeodeticDatum = row.GetValue(Terms.geodeticDatum);
            result.GeoreferencedBy = row.GetValue(Terms.georeferencedBy);
            result.GeoreferencedDate = row.GetValue(Terms.georeferencedDate);
            result.GeoreferenceProtocol = row.GetValue(Terms.georeferenceProtocol);
            result.GeoreferenceRemarks = row.GetValue(Terms.georeferenceRemarks);
            result.GeoreferenceSources = row.GetValue(Terms.georeferenceSources);
            result.GeoreferenceVerificationStatus = row.GetValue(Terms.georeferenceVerificationStatus);
            result.HigherGeography = row.GetValue(Terms.higherGeography);
            result.HigherGeographyID = row.GetValue(Terms.higherGeographyID);
            result.Island = row.GetValue(Terms.island);
            result.IslandGroup = row.GetValue(Terms.islandGroup);
            result.Locality = row.GetValue(Terms.locality);
            result.LocationAccordingTo = row.GetValue(Terms.locationAccordingTo);
            result.LocationId = row.GetValue(Terms.locationID);
            result.LocationRemarks = row.GetValue(Terms.locationRemarks);
            result.MaximumDepthInMeters = row.GetValue(Terms.maximumDepthInMeters);
            result.MaximumDistanceAboveSurfaceInMeters = row.GetValue(Terms.maximumDistanceAboveSurfaceInMeters);
            result.MaximumElevationInMeters = row.GetValue(Terms.maximumElevationInMeters);
            result.MinimumDepthInMeters = row.GetValue(Terms.minimumDepthInMeters);
            result.MinimumDistanceAboveSurfaceInMeters = row.GetValue(Terms.minimumDistanceAboveSurfaceInMeters);
            result.MinimumElevationInMeters = row.GetValue(Terms.minimumElevationInMeters);
            result.Municipality = row.GetValue(Terms.municipality);
            result.PointRadiusSpatialFit = row.GetValue(Terms.pointRadiusSpatialFit);
            result.StateProvince = row.GetValue(Terms.stateProvince);
            result.VerbatimCoordinates = row.GetValue(Terms.verbatimCoordinates);
            result.VerbatimCoordinateSystem = row.GetValue(Terms.verbatimCoordinateSystem);
            result.VerbatimDepth = row.GetValue(Terms.verbatimDepth);
            result.VerbatimElevation = row.GetValue(Terms.verbatimElevation);
            result.VerbatimLatitude = row.GetValue(Terms.verbatimLatitude);
            result.VerbatimLocality = row.GetValue(Terms.verbatimLocality);
            result.VerbatimLongitude = row.GetValue(Terms.verbatimLongitude);
            result.VerbatimSRS = row.GetValue(Terms.verbatimSRS);
            result.WaterBody = row.GetValue(Terms.waterBody);

            //-----------------------------------------------------------------------------------------------------------
            // occurrence
            //-----------------------------------------------------------------------------------------------------------
            result.AssociatedMedia = row.GetValue(Terms.associatedMedia);
            result.AssociatedReferences = row.GetValue(Terms.associatedReferences);
            result.AssociatedSequences = row.GetValue(Terms.associatedSequences);
            result.AssociatedTaxa = row.GetValue(Terms.associatedTaxa);
            result.Behavior = row.GetValue(Terms.behavior);
            result.CatalogNumber = row.GetValue(Terms.catalogNumber);
            result.Disposition = row.GetValue(Terms.disposition);
            result.EstablishmentMeans = row.GetValue(Terms.establishmentMeans);
            result.IndividualCount = row.GetValue(Terms.individualCount);
            result.LifeStage = row.GetValue(Terms.lifeStage);
            result.OccurrenceID = row.GetValue(Terms.occurrenceID);
            result.OccurrenceRemarks = row.GetValue(Terms.occurrenceRemarks);
            result.OccurrenceStatus = row.GetValue(Terms.occurrenceStatus);
            result.OrganismQuantity = row.GetValue(Terms.organismQuantity);
            result.OrganismQuantityType = row.GetValue(Terms.organismQuantityType);
            result.OtherCatalogNumbers = row.GetValue(Terms.otherCatalogNumbers);
            result.Preparations = row.GetValue(Terms.preparations);
            result.PreviousIdentifications = row.GetValue(Terms.previousIdentifications);
            result.RecordedBy = row.GetValue(Terms.recordedBy);
            result.RecordNumber = row.GetValue(Terms.recordNumber);
            result.ReproductiveCondition = row.GetValue(Terms.reproductiveCondition);
            result.Sex = row.GetValue(Terms.sex);

            //-----------------------------------------------------------------------------------------------------------
            // Organism
            //-----------------------------------------------------------------------------------------------------------
            result.OrganismID = row.GetValue(Terms.organismID);
            result.OrganismName = row.GetValue(Terms.organismName);
            result.OrganismScope = row.GetValue(Terms.organismScope);
            result.AssociatedOccurrences = row.GetValue(Terms.associatedOccurrences);
            result.AssociatedOrganisms = row.GetValue(Terms.associatedOrganisms);
            result.PreviousIdentifications = row.GetValue(Terms.previousIdentifications);
            result.OrganismRemarks = row.GetValue(Terms.organismRemarks);

            //-----------------------------------------------------------------------------------------------------------
            // Taxon
            //-----------------------------------------------------------------------------------------------------------
            result.AcceptedNameUsage = row.GetValue(Terms.acceptedNameUsage);
            result.AcceptedNameUsageID = row.GetValue(Terms.acceptedNameUsageID);
            result.Class = row.GetValue(Terms.@class);
            result.Family = row.GetValue(Terms.family);
            result.Genus = row.GetValue(Terms.genus);
            result.HigherClassification = row.GetValue(Terms.higherClassification);
            result.InfraspecificEpithet = row.GetValue(Terms.infraspecificEpithet);
            result.Kingdom = row.GetValue(Terms.kingdom);
            result.NameAccordingTo = row.GetValue(Terms.nameAccordingTo);
            result.NameAccordingToID = row.GetValue(Terms.nameAccordingToID);
            result.NamePublishedIn = row.GetValue(Terms.namePublishedIn);
            result.NamePublishedInID = row.GetValue(Terms.namePublishedInID);
            result.NamePublishedInYear = row.GetValue(Terms.namePublishedInYear);
            result.NomenclaturalCode = row.GetValue(Terms.nomenclaturalCode);
            result.NomenclaturalStatus = row.GetValue(Terms.nomenclaturalStatus);
            result.Order = row.GetValue(Terms.order);
            result.OriginalNameUsage = row.GetValue(Terms.originalNameUsage);
            result.OriginalNameUsageID = row.GetValue(Terms.originalNameUsageID);
            result.ParentNameUsage = row.GetValue(Terms.parentNameUsage);
            result.ParentNameUsageID = row.GetValue(Terms.parentNameUsageID);
            result.Phylum = row.GetValue(Terms.phylum);
            result.ScientificName = row.GetValue(Terms.scientificName);
            result.ScientificNameAuthorship = row.GetValue(Terms.scientificNameAuthorship);
            result.ScientificNameID = row.GetValue(Terms.scientificNameID);
            result.SpecificEpithet = row.GetValue(Terms.specificEpithet);
            result.Subgenus = row.GetValue(Terms.subgenus);
            result.TaxonConceptID = row.GetValue(Terms.taxonConceptID);
            result.TaxonID = row.GetValue(Terms.taxonID);
            result.TaxonomicStatus = row.GetValue(Terms.taxonomicStatus);
            result.TaxonRank = row.GetValue(Terms.taxonRank);
            result.TaxonRemarks = row.GetValue(Terms.taxonRemarks);
            result.VerbatimTaxonRank = row.GetValue(Terms.verbatimTaxonRank);
            result.VernacularName = row.GetValue(Terms.vernacularName);

            //-----------------------------------------------------------------------------------------------------------
            // UseWithIRI
            //-----------------------------------------------------------------------------------------------------------
            result.FromLithostratigraphicUnit = row.GetValue(Terms.fromLithostratigraphicUnit);
            result.InDataset = row.GetValue(Terms.inDataset);
            result.InDescribedPlace = row.GetValue(Terms.inDescribedPlace);
            result.ToTaxon = row.GetValue(Terms.toTaxon);

            //-----------------------------------------------------------------------------------------------------------
            // MaterialSample
            //-----------------------------------------------------------------------------------------------------------
            result.MaterialSampleID = row.GetValue(Terms.materialSampleID);
            
            return result;


            //if (row.TryGetValue(Terms.acceptedNameUsage, out string acceptedNameUsage))
            //{
            //    result.AcceptedNameUsage = acceptedNameUsage;
            //}

            //if (row.TryGetValue(Terms.basisOfRecord, out string basisOfRecord))
            //{
            //    result.BasisOfRecord = basisOfRecord;
            //}

            //if (row.TryGetValue(Terms.occurrenceID, out string occurrenceId))
            //{
            //    result.OccurrenceID = occurrenceId;
            //}

            //if (row.TryGetValue(Terms.catalogNumber, out string catalogNumber))
            //{
            //    result.CatalogNumber = catalogNumber;
            //}
        }

    }

}

