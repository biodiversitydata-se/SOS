# Changelog

## [2025.09.11]

### `Added`
- New search filter `TaxonFilterDto.IsInvasiveInSweden`. [Search filter documentation](SearchFilter.md#search-for-invasive-species-in-sweden).
- Support for extended authorization buffer in geographic area searches. New search filter is used by default `GeographicsFilterDto.ConsiderAuthorizationBuffer`.
- New property `ArtportalenInternal.InvasiveSpeciesTreatment` and search filter `ExtendedFilterDto.InvasiveSpeciesTreatmentIds`.
- New enum value for `ExtendedFilterDto.TypeFilter`: `SightingTypeFilterDto.ShowChildrenAndReplacements`.

### `Changed`
- All taxon attributes is now harvested from Dyntaxa custom DwC-A file.


## [2025.05.30]

### `Changed`
- Set `observation.Verified=true` for multiple datasets.
- Switch Elasticsearch driver to use `Elastic.Clients.Elasticsearch`.


## [2025.05.14]

### `Changed`
- `taxon.attributes.isInvasiveInSweden` is now including only taxa with `Severe risk` or `High risk`.

## [2025.04.10]

### `Fixed`
- Prevent duplicate harvest of Artportalen observations.

### `Added`
- Rate limiting by using semaphores in order to limit maximum number of concurrent requests.

### `Changed`
- iNaturalist harvest is now using the iNaturalist API instead of GBIF API.
- Change download limit to 50k observations in order to improve general performance.

## [2025.03.18]

### `Added`
- Signal search - Possibility to receive an indication that region authorization is missing.
- Validation for sortBy fields.

### `Other`
- Improved Observation database harvest and processing.

## [2025.02.12]

### `Added`
- Response compression is now enabled. Use request header `Accept-Encoding: gzip, deflate, br`.

### `Changed`
- Use text/html format for multimedia.csv in Artportalen DwC-A.

### `Other`
- Taxon list id 2 - Signal species, now uses Taxon list service id 282 instead of 33.
- Elasticsearch index optimizations.

## [2024.11.06]

### `Fixed`
- Duplicates could occur when paginating observations where many observations had the same value for the property being sorted on.
- Data provider default coordinateUncertaintyInMeters were not used for providers with DwC-A data source.

### `Other`
- Improved invalid observations error descriptions.

## [2024.10.16]

### `Added`
- AOO/EOO file order endpoint in Analysis API (internal).
- Statistics report endpoints (internal).
- DwC footprintWKT parsing support.

### `Changed`
- Updated Institutions vocabulary.
- InstitutionCode field in DwC-A now uses dataprovider.organization when the value is missing.

### `Fixed`
- Duplicates of Artportalen observations could occur under certain rare circumstances.

### `Other`
- Exports performance improvements.
- Cache improvements.

## [2024.06.19]

### `Added`
- Lund University Biological Museum - Animal Collections dataset.
- Swedish National Forest Inventory: Presence-absence vegetation data (1995-2017, discontinued sample)] dataset.

### `Changed`
- Prepare cache handling and upload files functionality for Kubernetes hosting.

### `Fixed`
- Taxon synonyms with misapplied status.
- Diffusion coordinate uncertainty.

## [2024.05.03]

### `Added`
- Support for Artportalen generalized observations when using `ProtectionFilter.BothPublicAndSensitive`. If you have permission, you will get the correct coordinate, otherwise you will get the generalized coordinate.

### `Changed`
- Artportalen media is moved from `occurrence.media` to `artportalenInternal.media` due to licenses.
- Artportalen DwC-A multimedia extension is not using the identifier property anymore due to licenses.

### `Fixed`
- Prevent duplicate output fields in exports.

### `Other`
- Improved Artportalen incremental harvest stability.


## [2024.02.06]

### `Added`
- Support for export of Artportalen project parameters to Excel.

### `Changed`
- File names in zip exports now include date.

### `Fixed`
- Artportalen DwC-A trailing space and quotes.
- Search by `UsePeriodForAllYears` filter bug.

### `Other`
- Improved Artportalen incremental harvest performance.

## [2023.12.20] - Hotfix

### `Fixed`
- Export to `GeoJSON` when using swedish or english property names resulted in duplicate properties and Exception when using all fields in export.
- Taxon List `Filter` operator didn't work properly when resulting taxa set resulted in empty list.
- In certain cases sensitive species observations could not be exported to `Excel`, `GeoJSON` and `CSV`.

## [2023.10.17] - Hotfix

### `Fixed`
- If `OccurrenceStatus` filter is not set, only present observations will be returned.

## [2023.09.20]

### `Added`
- Support for export to Sampling Event DwC-A.

### `Changed`
- Export to CSV, Excel and GeoJSON swedish and english column titles.
- Export to CSV, Excel and GeoJSON column sort orders.
- Daily DwC-A files is now created after the index is validated to be ok. 

### `Fixed`
- Wait for Event index creation did not always finish in expected time.
- Removed decimals in SWEREF99TM coordinate values.
- Stability improvement for recurring jobs (Incremental harvest, File exports).

### `Other`
- SOS Observations API integration tests now uses TestContainers and WebApplicationFactory to improve the quality of the integration tests.


## [2023.05.31] - Hotfix

### `Fixed`
 - GeoJSON exports bug when selecting only specific fields.
 - Histogram days fix.

## [2023.05.30] - Hotfix

### `Changed`
 - Changed the default value for `excludeNullValues` from `true` to `false`.

### `Fixed`
 - `IsRedlisted` field always had the value `false` in certain exports.

## [2023.05.23] - Hotfix

### `Added`
 - Add `Type` property to the `LocationDto` model used in the `/Locations` endpoint.

### `Fixed`
 - `OutputFilter.Fields` could generate exceptions in certain circumstances.

## [2023.04.18]

### `Added`
 - Data Stewardship API.
 - Analysis API.
 - External Health Check.
 - Possibility to exclude observations in search criteria.
 - Added possibility to change column order in exports.

### `Changed`
 - Improved support for harvesting events.
 - InstitutionCode field is now set to "SLU Artdatabanken" in Artportalen DwC-A export.
 - Upgrade to .Net 7.

### `Fixed`
 - Application Insights Observation Count logging bug.
 - Elasticsearch memory bug.


## [2023.01.31] - Hotfix

### `Added`
 - `SearchFilterInternal.SightingTypeSearchGroupIds` filter.

### `Changed`
 - Artportalen harvest SQL query optimization.
 - Set `datasetName` to english data provider name (if missing)

 ### `Fixed`
 - Taxon sum aggregation cache conccurrency issue.
 - Sweden area bug.


## [2023.01.31]

### `Changed`
 - Deprecated properties removed.
 - Added Elasticsearch shard replication

 ### `Fixed`
 - Taxon cache validation issue.
 - ETRS89 was using wrong EPSG code


## [2022.11.17]

### `Changed`
 - Improved user role information by adding information about authorities and areas in the endpoint `/User/Information`.

 ### `Fixed`
 - Some taxa hade wrong `ProtectedByLaw` value.
 - Improved GeoJSON serialization to encode å ä ö properly.

## [2022.10.17]

 ### `Fixed`
 - When searching with `TaxonListIds` underlying taxa will be added if `IncludeUnderlyingTaxa=true`.
 - TimeRange file order bug.
 - Aggregation by day bug.

## [2022.05.24]

### `Added`
- Artportalen Checklist support. New endpoints: `GET Checklists/{id}`, `POST Checklists/CalculateTrend`
- `Observations/MetricGridAggregation` endpoint that creates grid aggregations using SWEREF99 TM coordinate system.
- `SOS.Elasticsarch.Proxy` service that is used by WFS service (GeoServer).
- New observation properties: `Location.Sweref99TmX`, `Location.Sweref99TmY`, `Location.Attributes.ExternalId`, `Location.Attributes.ProjectId`, `Taxon.Attributes.TaxonCategory`, `Taxon.Attributes.IsRedlisted`, `Taxon.Attributes.IsInvasiveInSweden`, `Taxon.Attributes.IsInvasiveAccordingToEuRegulation`, `Taxon.Attributes.InvasiveRiskAssessmentCategory`
- Application insights harvest health check


### `Changed`
 - The following fields are replaced
    - `protected` replaced by `sensitive`
    - `occurrence.protectectionLevel` replaced by `occurrence.sensitivityCategory`
    - `taxon.attributes.protectionLevel` replaced by `taxon.attributes.sensitivityCategory`
    - `identification.validated` replaced by `identification.verified`
    - `identification.validationStatus` replaced by `identification.verificationStatus`

### `Fixed`
- DwC-A file text encoding label typo
- Duplicates when observation has changed during harvest



## [2022.03.08]

### `Added`
- Support for response compression. Use header : `Accept-Encoding: br` for Brotli compression or `Accept-Encoding: gzip` for GZIP compression. Brotli compression is usually the faster than GZIP.
- New parameter `gzip=true` or `gzip=false` to the `/Exports/Download` endpoints to enable retrieving Excel, GeoJSON and CSV files without GZIP compression.
- `ProvinceCount` property to `CachedCount` endpoint.

## [2021.12.02]

### `Added`
- New date and time properties : `Event.PlainStartTime`, `Event.PlainEndTime`, `Event.PlainStartDate`, `Event.PlainEndDate`. 
- Export to CSV.
- New endpoint for observation count that uses caching for improved performance.
- Support for Elasticsearch clusters.

### `Changed`
- Changed time format to use W. Europe Standard Time with time zone included. Example: "2016-01-17T11:00:00+01:00".
- Change property names (the replaced names will still be available until next version, 1.5, of the API)
  - `Sensitive` (replaces `Protected`)
  - `Occurrence.SensitiviyCategory` (replaces `Occurrence.ProtectionLevel`)
  - `Taxon.Attributes.SensitivityCategory` (replaces `Taxon.Attributes.ProtectionLevel`)
  - `Identification.Verified` (replaces `Identification.Validated`)
  - `Identification.VerificationStatus` (replaces `Identification.ValidationStatus`)
  - `ArtportalenInternal.HasAnyTriggeredVerificationRuleWithWarning` (replaces `ArtportalenInternal.HasAnyTriggeredValidationRuleWithWarning`)
  - `ArtportalenInternal.HasTriggeredVerificationRules` (replaces `ArtportalenInternal.HasTriggeredValidationRules`)
- Hangfire jobs now uses two queues. One with high priority (observation processing) and one with normal priority (file orders).
- Limit the number of simultaneous file orders per user to 5.

### `Fixed`
- Added a new date format in order to parse the TUVA dataset.
- Changed SHARK OccurrenceId value in order to get unique keys.
- 

## [2021.10.05]

### `Added`
- New search parameter : `ReportedByMe`. Requires authorization token.
- New search parameter : `ObservedByMe`. Requires authorization token.
- New `/Location` endpoint used to retrieve information about Artportalen sites.
- General support for protected species. Ie. also support for other data sources in addition to Artportalen and the Observation Database.
- Export to Excel.
- Export to GeoJSON.
- Multimedia extension is added to the Artportalen DwC-A export.
- Support for storing imported DwC-A file.
- Replaced OutputFields in search filter with output: { fields : string[], fieldSet: enum  (Minimum, Extended, AllWithKnownValues, All)}

### `Fixed`
- DwC-A time parsing bug in the harvest step.

## [2021.06.17]

### Releases

### `Added`
- New search filter: `BirdNestActivityLimit`
- New vocabulary: `BirdNestActivity`
- Possibility to get result as GeoJSON in the following endpoint: `/Observations/Internal/Search`
- Possibility to export areas as JSON, GeoJSON or WKT in the `/Areas/{areaType}/{fetureId}` endpoint.

### `Changed`
- Geting a single observation by occurrence id (both external and internal) return one observation and not an array with one observation
#### API

#### Sök
- Bbox parametrar som skickats i "query-string" har ersatts av Geographics.BoundingBox

#### Sökfilter
- ExtendedFilter.MaxAccuracy => Geographics.MaxAccuracy
- ExtendedFilter.DeterminationFilter => DeterminationFilter
- ExtendedFilter.BoundingBox => Geographics.BoundingBox
- ExtendedFilter.NotRecoveredFilter => NotRecoveredFilter
- Geometry => Geographics
- Areas => Geographics.Areas
- OnlyValidated => ValidationStatus (enum)

## [2021.04.08]

### Releases

### `Changed`

#### API

##### Observations 
All API-endpoints with the suffix Internal have been moved to `/Internal/previous name`.

Ex. `/SearchInternal` -> `/Internal/Search`

`/Observations/TaxonAggregation` has changed from approximate aggregation to always correct aggregation. To get all records you can set take and limit to null. You can also use paging, but only for the first 1000 records.

##### Dataprovider

 `/Observations/Provider/{providerId}/LastModified` has been moved to `/DataProvider/{providerId}/LastModified`

##### Areas

- `/Areas/{areaType}/{feature}/Export` endpoint changed to `/Areas/{areaType}/{featureId}/Export`.

#### Sökfilter
- ArtportalenFilter.GenderIds => ExtendedFilter.SexIds
- ArtportalenFilter.ProjectIds => ProjectIds
- ArtportalenFilter => ExtendedFilter
- dataProviderIds => dataProvider.ids
- Taxon.TaxonIds => taxon.Ids
- geometry.usePointAccuracy => geometry.considerObservationAccuracy

#### Observation

- Om `Identification.UncertainIdentification` (förr `UncertainDetermination`) = true så sätts `Identification.IdentificationRemarks` till värdet `Uncertain determination`
- ReportedBy => Occurrence.ReportedBy
- ReportedDate => Occurrence.ReportedDate
- ProtectionLevel => Occurrence.ProtectionLevel
- Media => Occurrence.Media
- `Validated` value is set to true when `IdentificationVerificationStatus` has value `Reported by expert`.
- New field `SpeciesCollectionLabel`

*ArtportalenInternal*
- PrivateCollection => Observation.PrivateCollection
- Projects => Observation.Projects

*Event*
- QuantityOfSubstrate => Occurrence.Substrate.Quantity
- Substrate => Occurrence.Substrate.Name
- SubstrateDescription => Occurrence.Substrate.Description
- SubstrateSpeciesDescription => Occurrence.Substrate.SpeciesDescription
- SubstrateSpeciesVernacularName => Occurrence.Substrate.SpeciesVernacularName
- SubstrateSpeciesScientificName => Occurrence.Substrate.SpeciesScientificName
- SubstrateSpeciesId => Occurrence.Substrate.SpeciesId

*Identification*
- Identification.UncertainDetermination => Identification.UncertainIdentification

*Occurrence*
- DiscoveryMethod => Event.DiscoveryMethod
- Gender => Occurrence.Sex
- DeterminedBy => Identification.IdentifiedBy
- DeterminationYear => ArtportalenInternal.DeterminationYear
- ConfirmationYear => ArtportalenInternal.ConfirmationYear
- ConfirmedBy => Identification.ConfirmedBy
- PublicCollection => Observation.PublicCollection

*Taxon*
- Natura2000HabitatsDirectiveArticle2 => Taxon.Attributes.Natura2000HabitatsDirectiveArticle2
- Natura2000HabitatsDirectiveArticle4 => Taxon.Attributes.Natura2000HabitatsDirectiveArticle4
- Natura2000HabitatsDirectiveArticle5 => Taxon.Attributes.Natura2000HabitatsDirectiveArticle5
- RedlistCategory => Taxon.Attributes.RedlistCategory
- DyntaxaTaxonId => Taxon.Attributes.DyntaxaTaxonId
- ParentDyntaxaTaxonId => Taxon.Attributes.ParentDyntaxaTaxonId
- OrganismGroup => Taxon.Attributes.OrganismGroup
- ProtectionLevel => Taxon.Attributes.ProtectionLevel
- ActionPlan => Taxon.Attributes.ActionPlan
- DisturbanceLevel => Taxon.Attributes.DisturbanceLevel
- ProtectedByLaw => Taxon.Attributes.ProtectedByLaw
- SortOrder => Taxon.Attributes.SortOrder
- SwedishOccurrence => Taxon.Attributes.SwedishOccurrence
- SwedishHistory => Taxon.Attributes.SwedishHistory
- VernacularNames => Taxon.Attributes.VernacularNames
- Synonyms => Taxon.Attributes.Synonyms
- IndividualId => ArtportalenInternal.SightingBarcodeURL

*Location*
- ParentLocationId => ArtportalenInternal.ParentLocationId
- ParentLocality => ArtportalenInternal.ParentLocality
- CountyPartIdByCoordinate => Location.Attributes.CountyPartIdByCoordinate
- ProvincePartIdByCoordinate => Location.Attributes.ProvincePartIdByCoordinate
- VerbatimMunicipality => Location.Attributes.VerbatimMunicipality
- VerbatimProvince => Location.Attributes.VerbatimProvince

#### DataProviders
- Name, swedishName => Name, depending on input parameter
- Description, swedishDescription => Description, depending on input parameter
- Organization, swedishOrganization => Organization, depending on input parameter
- Path: [string], depending on input parameter


### `Removed`
- `Occurrence.OrganismQuantityInt` field removed.


## [2021.01.21]

### Releases

### `Added`
- New field: `Observation.Occurrence.ReproductiveCondition`
- New vocabularies: `ReproductiveCondition` and `Behavior`

### `Changed`
- Remove the `translationCultureCode` field from search filter, and add it as a query parameter.
- Change data type for the `Observation.Occurrence.Behavior` field from string to VocabularyValue.

### `Fixed`
- Sort order in `/Observations/TaxonAggregation` endpoint. Now the paging will work.

### `Removed`
- `OutputFields` field from search filter for aggregation endpoints.  
- `SortBy` and `SortOrder` query parameters from the `Observations/SearchAggregatedInternal` endpoint.
- `AreaIds` field in search filter. Replaced by `Areas`

### `Other`
Areas
Id i areaobjektet är borttagen. En area identifieras av sin areaType och featureid.
Resursen Areas/{areaId}/Export är borttagen
Area-objektet är ändrat.
Från:
{
  "areaType": "Municipality",
  "feature": "1440",
  "id": 53,
  "name": "Ale"
},

Till:
{
  "areaType": "Municipality",
  "featureId": "1440",
  "name": "Ale"
}

Observations
Generellt
TranslationCultureCode är flyttad ut från sökfiltret och skickas numera in som en query-parameter
AreaId är borttaget från sökfiltret. Areas anges enligt:
"areas": [
  {
    "type": "AreaType",
    "featureId": "string"
  }
]
Aggregerade sökningar
OutputFields är borttagen från aggregerade sökningar då den inte fyllde någon funktion
SearchAggregatedInternal
SortBy, sortOrder borttagna då den inte fyllde någon funktion

## [2020.11.30] - 2020-11-30

### Releases

Initial release


## Changelog documentation


All notable changes to Observation API will be documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),



##### Types of changes
- `Added` for new features.
- `Changed` for changes in existing functionality.
- `Deprecated` for soon-to-be removed features.
- `Removed` for now removed features.
- `Fixed` for any bug fixes.
- `Security` in case of vulnerabilities.

