# Extended filter

## NotPresentFilter
Corresponds to IncludeNeverFoundObservations filter in the SOAP web service.

This filter will only return observations where `observation.occurrence.isNeverFoundObservation=false`.
(default)
```json
{
  "extendedFilter": {
     "notPresentFilter": "DontIncludeNotPresent"
  }
}
```

This filter will only return observations where `observation.occurrence.isNeverFoundObservation=true`. About 8 million observations.
```json
{
  "extendedFilter": {
     "notPresentFilter": "OnlyNotPresent"
  }
}
```

Don't apply this filter 
```json
{
  "extendedFilter": {
     "notPresentFilter": "IncludeNotPresent"
  }
}
```

Property used in the Elasticsearch query: 
`occurrence.isNeverFoundObservation` - *Indicates if this observation is a never found observation. "Never found observation" is an observation that says that the specified species was not found in a location deemed appropriate for the species.*
