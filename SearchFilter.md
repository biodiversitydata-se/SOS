# Search filter parameters
This page provides information about how to use the search filter parameters.

- [Data provider filter](#data-provider-filter)
- [Date filter](#date-filter)
  * [BetweenStartDateAndEndDate filter type](#betweenstartdateandenddate-filter-type)
  * [OverlappingStartDateAndEndDate filter type](#overlappingstartdateandenddate-filter-type)
  * [OnlyStartDate filter type](#onlystartdate-filter-type)
  * [OnlyEndDate filter type](#onlyenddate-filter-type)
  * [TimeRanges](#timeranges)
- [Area filter](#area-filter)
  * [Search for observations in area](#search-for-observations-in-area)
  * [Search for observations in multiple areas of same type](#search-for-observations-in-multiple-areas-of-same-type)
  * [Search for observations in multiple areas of different types](#search-for-observations-in-multiple-areas-of-different-types)
- [Geometry filter](#geometry-filter)
  * [Search for observations within a polygon](#search-for-observations-within-a-polygon)
  * [Search for observations within a circle](#search-for-observations-within-a-circle)
- [Taxon filter](#taxon-filter)
  * [Search for observations of a specific taxon](#search-for-observations-of-a-specific-taxon)
  * [Include underlying taxa in search](#include-underlying-taxa-in-search)
  * [Search for observations with red list categories](#search-for-observations-with-red-list-categories)
- [Projects filter](#projects-filter)
- [Occurrence status filter](#occurrence-status-filter)
- [OutputFields](#outputfields)

## Data provider filter
This request will return observations only from Artportalen and MVM.
```json
{    
    "dataProvider": {
        "ids": [1, 4]
  }
}
```

| Data provider | Id |
|-|-|
| Artportalen | 1 |
| Clam Gateway | 2 |
| KUL | 3 |
| MVM | 4 |
| ... | ... |


## Date filter

### BetweenStartDateAndEndDate filter type
This request will return observations where `observation.event.startDate` is greater than or equal to `startDate` and `observation.event.endDateDate` is less than or equal to `endDate`.
```json
{        
  "date": {
    "startDate": "2021-02-02",
    "endDate": "2021-02-04",
    "dateFilterType": "BetweenStartDateAndEndDate"    
  }
}
```

| Observation date | Match filter? |
|-|-|
| obs.event.startDate=2021-02-02<br>obs.event.endDate=2021-02-03 | Yes |
| obs.event.startDate=2021-02-03<br>obs.event.endDate=2021-02-04 | Yes |
| obs.event.startDate=2021-02-04<br>obs.event.endDate=2021-02-05 | No |
| obs.event.startDate=2021-02-02<br>obs.event.endDate=2021-02-04 | Yes |
| obs.event.startDate=2021-02-02<br>obs.event.endDate=2021-02-05 | No |
| obs.event.startDate=2021-02-01<br>obs.event.endDate=2021-02-04 | No |

### OverlappingStartDateAndEndDate filter type

This request will return observations where `observation.event.startDate` is less than or equal to `endDate` and `observation.event.endDateDate` is greater than or equal to `startDate`.
```json
{        
  "date": {
    "startDate": "2021-02-02",
    "endDate": "2021-02-04",
    "dateFilterType": "OverlappingStartDateAndEndDate"    
  }
}
```

| Observation date | Match filter? |
|-|-|
| obs.event.startDate=2021-02-02<br>obs.event.endDate=2021-02-03 | Yes |
| obs.event.startDate=2021-02-03<br>obs.event.endDate=2021-02-04 | Yes |
| obs.event.startDate=2021-02-04<br>obs.event.endDate=2021-02-05 | Yes |
| obs.event.startDate=2021-02-02<br>obs.event.endDate=2021-02-04 | Yes |
| obs.event.startDate=2021-02-02<br>obs.event.endDate=2021-02-05 | Yes |
| obs.event.startDate=2021-02-01<br>obs.event.endDate=2021-02-04 | Yes |
| obs.event.startDate=2021-02-01<br>obs.event.endDate=2021-02-01 | No |
| obs.event.startDate=2021-02-05<br>obs.event.endDate=2021-02-05 | No |

### OnlyStartDate filter type

This request will return observations where `observation.event.startDate`  is between `startDate` and `endDate`.
```json
{        
  "date": {
    "startDate": "2021-02-02",
    "endDate": "2021-02-03",
    "dateFilterType": "OnlyStartDate"    
  }
}
```

| Observation date | Match filter? |
|-|-|
| obs.event.startDate=2021-02-02 | Yes |
| obs.event.startDate=2021-02-03 | Yes |
| obs.event.startDate=2021-02-04 | No |

### OnlyEndDate filter type

This request will return observations where `observation.event.endDate`  is between `startDate` and `endDate`.
```json
{        
  "date": {
    "startDate": "2021-02-02",
    "endDate": "2021-02-03",
    "dateFilterType": "OnlyEndDate"    
  }
}
```

| Observation date | Match filter? |
|-|-|
| obs.event.endDate=2021-02-02 | Yes |
| obs.event.endDate=2021-02-03 | Yes |
| obs.event.endDate=2021-02-04 | No |


### TimeRanges

This request will return observations where the hour part of `observation.event.startDate` is between `04:00` and `13:00`.
```json
{        
  "date": {
    "timeRanges": ["Morning", "Forenoon"]
  }
}
```

| Time range | Hours |
|-|-|
| Morning | 04:00-09:00 |
| Forenoon | 09:00-13:00 |
| Afternoon | 18:00-23:00 |
| Evening | 23:00-04:00 |



## Area filter

Areas used in samples.

| Area | AreaType | FeatureId
|-|-|-|
| Tranås municipality | "Municipality" | "687" |
| Jönköping county | "County" | "6" |
| Kronoberg county | "County" | "7" |

### Search for observations in area
This request will return observations in Tranås municipality.
```json
{        
    "areas" : [
        {
            "areaType": "Municipality",
            "featureId": "687"
        }
    ]
}
```

### Search for observations in multiple areas of same type
This request will return observations in Jönköping & Kronoberg county. OR operator is used when specifying multiple areas of same type.
```json
{        
    "areas" : [
        {
            "areaType": "County",
            "featureId": "6"
        },
        {
            "areaType": "County",
            "featureId": "7"
        }
    ]
}
```


### Search for observations in multiple areas of different types
This request will return zero observations since Tranås municipality is not part of Kronoberg county. AND operator is used when specifying areas of different types.
```json
{        
    "areas" : [
        {
            "areaType": "County",
            "featureId": "7"
        },
        {
            "areaType": "Municipality",
            "featureId": "687"
        }
    ]
}
```

This request will return observations in Tranås municipality since it is part of Jönköping county.
```json
{        
    "areas" : [
        {
            "areaType": "County",
            "featureId": "6"
        },
        {
            "areaType": "Municipality",
            "featureId": "687"
        }
    ]
}
```


## Geometry filter

### Search for observations within a polygon
This request will return observations within the specified polygon.
> If `"considerObservationAccuracy": true`, then observations that are outside the polygon but possibly inside when accuracy (coordinateUncertaintyInMeters) of observation is considered, will be included in the result.
```json
{
    "geometry": {
        "geometries": [
            {
                "type": "polygon",
                "coordinates": [
                    [
                        [15.07063, 57.92573],
                        [15.0051, 58.16108],
                        [14.58003, 58.10148],
                        [14.64143, 57.93294],
                        [15.07063, 57.92573]
                    ]
                ]
            }
        ],        
        "considerObservationAccuracy": false
    }
}
```

### Search for observations within a circle
This request will return observations within the specified circle with a radius of 500m.
```json
{
    "geometry": {        
        "geometries": [
            {"type":"point", "coordinates": [14.99047, 58.01563]}
        ], 
        "maxDistanceFromPoint": 500
    }
}
```


## Taxon filter

### Search for observations of a specific taxon
This request will return observations where taxon is 100777, otter.
```json
{        
    "taxon" : {
        "ids": [100077]            
    }
}
```

### Include underlying taxa in search
This request will return all bird (aves) observations.
```json
{        
    "taxon" : {
        "ids": [4000104],
        "includeUnderlyingTaxa": true
    }
}
```

### Search for observations with red list categories
This request will return observation of critically endangered (CR), endangered (EN) and vulnerable (VU) species.
```json
{        
    "taxon" : {
        "redListCategories": [ "CR", "EN", "VU" ]
    }
}
```

| Red list category | Title |
|-|-|
| RE | Regionally extinct |
| CR | Critically endangered |
| EN | Endangered |
| VU | Vulnerable |
| NT | Near threatened |
| DD | Data deficient |
| EX | Extinct |
| LC | Least concern |
| NA | Not applicable |
| NE | Not evaluated |


## Projects filter
This request will return observations in the project ArtArken
```json
{
    "projectIds": [113]
}
```


## Occurrence status filter
This request will return observations with occurrenceStatus "present"
```json
{
    "occurrenceStatus": "present"
}
```

This request will return observations with occurrenceStatus "absent"
```json
{
    "occurrenceStatus": "absent"
}
```

## OutputFields
With outputFields you can specify what fields should be included in the result.
```json
{        
    "outputFields": [
        "datasetName", 
        "occurrence.occurrenceId", 
        "location.municipality", 
        "taxon.vernacularName", 
        "event.startDate", 
        "event.endDate"
    ]
}
=>
{
"records": [
    {
        "datasetName": "Artportalen",
        "location": {
            "municipality": {
                "name": "Hultsfred",
                "featureId": "860"
            }
        },
        "taxon": {
            "vernacularName": "utter"
        },
        "occurrence": {
            "occurrenceId": "urn:lsid:artportalen.se:Sighting:15959565"
        },
        "event": {
            "endDate": "2014-06-06T22:00:00Z",
            "startDate": "2014-06-06T22:00:00Z"
        }
    },
    {
        "datasetName": "Artportalen",
        "location": {
            "municipality": {
                "name": "Boden",
                "featureId": "2582"
            }
        },
        "taxon": {
            "vernacularName": "utter"
        },
        "occurrence": {
            "occurrenceId": "urn:lsid:artportalen.se:Sighting:4517"
        },
        "event": {
            "endDate": "2008-12-05T23:00:00Z",
            "startDate": "2008-12-05T23:00:00Z"
        }
    },
    ...
}
```
