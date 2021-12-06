# Changelog

## [1.4.1]

**Prod**: 

**Test**: 2021-12-06

### `Added`
- Support for response compression. Use header : `Accept-Encoding: br` for Brotli compression or `Accept-Encoding: gzip` for GZIP compression. Brotli compression is usually the faster than GZIP.
- New parameter `gzip=true` or `gzip=false` to the `/Exports/Download` endpoints to enable retrieving Excel, GeoJSON and CSV files without GZIP compression.

## [1.4]

**Prod**: 2021-12-02

**Test**: 2021-12-06

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

## [1.3]

**Prod**: 2021-10-05

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

## [1.2]

### Releases

**Prod**: 2021-06-17

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

## [1.1]

### Releases

**Prod**: 2021-04-08

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


## [1.0.0] - 2021-01-21

### Releases

**Prod**: 2021-01-21

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

## [0.9.1] - 2020-11-30

### Releases

**Test**: 2020-11-30

**Prod**: 2020-11-30


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

