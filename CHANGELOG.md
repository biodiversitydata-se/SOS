# Changelog

## [1.0.1]

### Releases

**Test**: 2021-03-09

**Prod**: Unreleased. Preliminary release april 2021


### `Changed`

#### API

##### Observations 
All API-endpoints with the suffix Internal have been moved to `/Internal/previous name`.

Ex. `/SearchInternal` -> `/Internal/Search`


##### Dataprovider

 `/Observations/Provider/{providerId}/LastModified` has been moved to `/DataProvider/{providerId}/LastModified`

##### Areas

- `/Areas/{areaType}/{feature}/Export` endpoint changed to `/Areas/{areaType}/{featureId}/Export`.

#### Sökfilter
- ArtportalenFilter.GenderIds => ExtendedFilter.SexIds
- ArtportalenFilter.ProjectIds => ProjectIds
- ArtportalenFilter => ExtendedFilter
- dataProviderIds => dataProvider.ids

#### Observation

- Om `Identification.UncertainIdentification` (förr `UncertainDetermination`) = true så sätts `Identification.IdentificationRemarks` till värdet `Uncertain determination`
- ReportedBy => Occurrence.ReportedBy
- ReportedDate => Occurrence.ReportedDate
- ProtectionLevel => Occurrence.ProtectionLevel
- Media => Occurrence.Media
- `Validated` value is set to true when `IdentificationVerificationStatus` has value `Reported by expert`.

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
- DeterminedBy tas bort och värdet mappas istället till Identification.IdentifiedBy
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

**Test**: 2021-01-21

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

