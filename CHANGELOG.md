# Changelog

All notable changes to Observation API will be documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),



##### Types of changes
- `Added` for new features.
- `Changed` for changes in existing functionality.
- `Deprecated` for soon-to-be removed features.
- `Removed` for now removed features.
- `Fixed` for any bug fixes.
- `Security` in case of vulnerabilities.


## [Unreleased]
### Added

### Changed
- `Validated` value is set to true when `IdentificationVerificationStatus` has value `Reported by expert`.

### Fixed


### Removed
- `Occurrence.OrganismQuantityInt` field removed.


## [1.0.0] - 2021-01-21

### Added
- New field: `Observation.Occurrence.ReproductiveCondition`
- New vocabularies: `ReproductiveCondition` and `Behavior`

### Changed
- Remove the `translationCultureCode` field from search filter, and add it as a query parameter.
- Change data type for the `Observation.Occurrence.Behavior` field from string to VocabularyValue.

### Fixed
- Sort order in `/Observations/TaxonAggregation` endpoint. Now the paging will work.

### Removed
- `OutputFields` field from search filter for aggregation endpoints.  
- `SortBy` and `SortOrder` query parameters from the `Observations/SearchAggregatedInternal` endpoint.
- `AreaIds` field in search filter. Replaced by `Areas`

### Other
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
Initial release



