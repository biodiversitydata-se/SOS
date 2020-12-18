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
- New field: `Observation.Occurrence.ReproductiveCondition`
- New search field: `Areas`. This relaces the `AreaIds` field.

### Changed
- Remove the `translationCultureCode` field from search filter, and add it as a query parameter.


### Fixed
- Sort order in `/Observations/TaxonAggregation` endpoint. Now the paging will work.

### Removed
- `OutputFields` field from search filter for aggregation endpoints. Affected endpoints:  
- `SortBy` and `SortOrder` query parameters from the `Observations/SearchAggregatedInternal` endpoint.
- `AreaIds` field in search filter. Replaced by `Areas`

## [0.9.1] - 2020-11-30
Initial release
