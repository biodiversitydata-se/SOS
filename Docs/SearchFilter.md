# Search filter parameters
This page provides information about how to use the search filter parameters.

- [Data provider filter](#data-provider-filter)
- [Date filter](#date-filter)
  * [BetweenStartDateAndEndDate filter type](#betweenstartdateandenddate-filter-type)
  * [OverlappingStartDateAndEndDate filter type](#overlappingstartdateandenddate-filter-type)
  * [OnlyStartDate filter type](#onlystartdate-filter-type)
  * [OnlyEndDate filter type](#onlyenddate-filter-type)
  * [TimeRanges](#timeranges)
- [Geographics filter](#geographics-filter)
  * [Search for observations in area](#search-for-observations-in-area)
  * [Search for observations in multiple areas of same type](#search-for-observations-in-multiple-areas-of-same-type)
  * [Search for observations in multiple areas of different types](#search-for-observations-in-multiple-areas-of-different-types)
  * [Search for observations within a polygon](#search-for-observations-within-a-polygon)
  * [Search for observations within a circle](#search-for-observations-within-a-circle)
  * [Search for observations within a bounding box](#search-for-observations-within-a-bounding-box)
  * [Observation accuracy](#observation-accuracy)
- [Taxon filter](#taxon-filter)
  * [Search for observations of a specific taxon](#search-for-observations-of-a-specific-taxon)
  * [Include underlying taxa in search](#include-underlying-taxa-in-search)
  * [Search for observations with red list categories](#search-for-observations-with-red-list-categories)
  * [Taxon lists](#taxon-lists)
  * [Search for observations with taxon lists - merge](#search-for-observations-with-taxon-lists---merge)
  * [Search for observations with taxon lists - filter](#search-for-observations-with-taxon-lists---filter)
- [Projects filter](#projects-filter)
- [Occurrence status filter](#occurrence-status-filter)
- [Verification status filter](#verification-status-filter)
- [Determination filter](#determination-filter)
- [NotRecovered filter](#notrecovered-filter)
- [BirdNestActivityLimit filter](#birdnestactivitylimit-filter)
- [Reported by me filter](#ReportedByMe-filter)
- [Observed by me filter](#ObservedByMe-filter)
- [Output](#output)
  * [FieldSet](#fieldSet)
  * [Fields](#fields)


## Data provider filter
This filter will return observations only from Artportalen and MVM.
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

### OverlappingStartDateAndEndDate filter type

This filter will return observations where `observation.event.startDate` is less than or equal to `endDate` and `observation.event.endDateDate` is greater than or equal to `startDate`. This is the **default** filter type used when no filter type is specified.
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

### BetweenStartDateAndEndDate filter type
This filter will return observations where `observation.event.startDate` is greater than or equal to `startDate` and `observation.event.endDate` is less than or equal to `endDate`.
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

### OnlyStartDate filter type

This filter will return observations where `observation.event.startDate`  is between `startDate` and `endDate`.
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

This filter will return observations where `observation.event.endDate`  is between `startDate` and `endDate`.
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

This filter will return observations where the hour part of `observation.event.startDate` is between `04:00` and `13:00`.
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


## Geographics filter

Areas used in samples.

| Area | AreaType | FeatureId
|-|-|-|
| Tranås municipality | "Municipality" | "687" |
| Jönköping county | "County" | "6" |
| Kronoberg county | "County" | "7" |

### Search for observations in area
This filter will return observations in Tranås municipality.
```json
{   
    "geographics" : {     
        "areas" : [
            {
                "areaType": "Municipality",
                "featureId": "687"
            }
        ]
    }
}
```

### Search for observations in multiple areas of same type
This filter will return observations in Jönköping & Kronoberg county. OR operator is used when specifying multiple areas of same type.
```json
{
    "geographics" : {
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
}
```


### Search for observations in multiple areas of different types
This filter will return zero observations since Tranås municipality is not part of Kronoberg county. AND operator is used when specifying areas of different types.
```json
{
    "geographics" : {
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
}
```

This filter will return observations in Tranås municipality since it is part of Jönköping county.
```json
{
    "geographics" : {
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
}
```


### Search for observations within a polygon
This filter will return observations within the specified polygon. The coordinates should be specified in the WGS84 coordinate system.
> If `"considerObservationAccuracy": true`, then observations that are outside the polygon but possibly inside when accuracy (coordinateUncertaintyInMeters) of observation is considered, will be included in the result.
```json
{
    "geographics": {
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
This filter will return observations within the specified circle with a radius of 500m. The coordinates should be specified in the WGS84 coordinate system.
```json
{
    "geographics": {        
        "geometries": [
            {"type":"point", "coordinates": [14.99047, 58.01563]}
        ], 
        "maxDistanceFromPoint": 500
    }
}
```

### Search for observations within a bounding box
This filter will return observations within the specified bounding box. The coordinates should be specified in the WGS84 coordinate system.
```json
{
    "geographics": {        
        "boundingBox": {
            "bottomRight": {
                "latitude": 59.17592,
                "longitude": 18.28125
            },
            "topLeft": {
                "latitude": 59.35559,
                "longitude": 17.92968
            }
        }
    }
}
```

### Observation accuracy

| Parameter 	| Description 	|
|-	|-	|
| considerObservationAccuracy 	| If `true`, then observations that are outside the polygon but possibly inside when accuracy (coordinateUncertaintyInMeters) of observation is considered, will be included in the result. 	|
| considerDisturbanceRadius 	| If `true`, then observations that are outside Geometries polygons but close enough when disturbance sensitivity of species are considered, will be included in the result. 	|
| maxAccuracy 	| If set, only observations with less than or equal accuracy (coordinateUncertaintyInMeters) will be included in the result. 	|


This filter will return observations within the specified polygon and observations outside the polygon that is possibly inside when the accuracy is considered. Only observations with accuracy <= 1000m will be included.
```json
{
    "geographics": {
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
        "considerObservationAccuracy": true,
        "considerDisturbanceRadius": true,
        "maxAccuracy": 1000
    }
}
```


## Taxon filter

### Search for observations of a specific taxon
This filter will return observations where taxon is 100777, otter.
```json
{        
    "taxon" : {
        "ids": [100077]            
    }
}
```

### Include underlying taxa in search
This filter will return all bird (aves) observations.
```json
{        
    "taxon" : {
        "ids": [4000104],
        "includeUnderlyingTaxa": true
    }
}
```

### Search for observations with red list categories
This filter will return observation of critically endangered (CR), endangered (EN) and vulnerable (VU) species.
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


### Taxon lists

The following taxon lists exists:

| Id 	| ParentId 	| Title (en) 	| Title (sv) 	|
|-	|-	|-	|-	|
| 1 	|  	| Protected by law species 	| Fridlysta arter 	|
| 2 	|  	| Signal species 	| Signalarter 	|
| 3 	|  	| Invasive species 	| Främmande arter 	|
| 4 	| 3 	| Invasive species in Sweden 	| Främmande arter i Sverige 	|
| 5 	| 3 	| EU regulation 1143/2014 	| EU-förordning 1143/2014 	|
| 6 	| 3 	| Risk assessment 	| Risklista 	|
| 7 	|  	| Redlisted species 	| Rödlistade arter 	|
| 8 	|  	| Habitats directive species 	| Habitatdirektivet 	|
| 9 	| 8 	| Habitats directive Annex 2 	| Habitatdirektivets bilaga 2 	|
| 10 	| 8 	| Habitats directive Annex 2, priority species 	| Habitatdirektivets bilaga 2 (prioriterad art) 	|
| 11 	| 8 	| Habitats directive Annex 4 	| Habitatdirektivets bilaga 4 	|
| 12 	| 8 	| Habitats directive Annex 5 	| Habitatdirektivets bilaga 5 	|
| 13 	|  	| Birds 	| Fåglar 	|
| 14 	| 13 	| Priority birds 	| Prioriterade fåglar 	|
| 15 	| 13 	| Birds directive - Annex 1 	| Fågeldirektivet - bilaga 1 	|
| 16 	| 13 	| Birds directive - Annex 2 	| Fågeldirektivet - bilaga 2 	|
| 17 	|  	| Action plan 	| Åtgärdsprogram 	|
| 18 	|  	| Swedish forest agency nature conservation species 	| Skogsstyrelsens naturvårdsarter 	|

### Search for observations with taxon lists - merge

This filter will return observations for all mammal species (TaxonId=4000107) and all Habitats directive species.
```json
{        
    "taxon" : {
        "ids": [4000107],
        "includeUnderlyingTaxa": true,
        "taxonListIds": [ 8 ],
        "taxonListOperator": "Merge"
    }
}
```

### Search for observations with taxon lists - filter

This filter will return observations for mammal (TaxonId=4000107) species that are part of the Habitats directive species taxon list.
```json
{        
    "taxon" : {
        "ids": [4000107],
        "includeUnderlyingTaxa": true,
        "taxonListIds": [ 8 ],
        "taxonListOperator": "Filter"
    }
}
```

## Projects filter
This filter will return observations in the project ArtArken
```json
{
    "projectIds": [113]
}
```


## Occurrence status filter
This filter will return observations with occurrenceStatus "present"
```json
{
    "occurrenceStatus": "present"
}
```

This filter will return observations with occurrenceStatus "absent"
```json
{
    "occurrenceStatus": "absent"
}
```

## Verification status filter
This filter will return only verified observations.
```json
{
    "verificationStatus": "Verified"
}
```

This filter will return only non verified observations.
```json
{
    "verificationStatus": "NotVerified"
}
```

## Determination filter
This filter will only return observations where `observation.identification.uncertainIdentification=false`.
```json
{
    "determinationFilter": "NotUnsureDetermination"
}
```

This filter will only return observations where `observation.identification.uncertainIdentification=true`.
```json
{
    "determinationFilter": "OnlyUnsureDetermination"
}
```

## NotRecovered filter
This filter will only return observations where `observation.occurrence.isNotRediscoveredObservation=false`.
```json
{
    "notRecoveredFilter": "DontIncludeNotRecovered"
}
```

This filter will only return observations where `observation.occurrence.isNotRediscoveredObservation=true`.
```json
{
    "notRecoveredFilter": "OnlyNotRecovered"
}
```

## BirdNestActivityLimit filter
This filter returns observations where `observation.occurrence.birdNestActivityId` is lower than or equal to the filter value.
```json
{
    "birdNestActivityLimit": 10
}
```

## ReportedByMe filter
This filter returns observations that are reported by the user specified in the authorization access token.
```json
{
    "reportedByMe": true
}
```

## ObservedByMe filter
This filter returns observations that are observed by the user specified in the authorization access token.
```json
{
    "reportedByMe": true
}
```

## Output
With output you can specify which fields that should be included in the result. The default is the `Minimum` field set (the 22 most important fields). Fields with null value are never returned.

### fieldSet
There are four predefined field sets to choose from: [Minimum](FieldSets.md#Minimum), [Extended](FieldSets.md#Extended), [AllWithValues](FieldSets.md#AllWithValues) and [All](FieldSets.md#All). They are described on the [FieldSet documentation page](FieldSets.md)

This filter will return all fields defined in the [Extended](FieldSets.md#Extended) field set.
```json
{
    "output": {
       "fieldSet": "Extended"
    }
}
```

### fields
With fields you can specify which additional fields that should be included in the result.

This filter will return all fields defined in the [Minimum](FieldSets.md#Minimum) field set and also the fields `location.province`, `location.parish`
```json
{
    "output": {
       "fieldSet": "Minimum",
       "fields": ["location.province", "location.parish"]
    }
}
```

If you specify `fields` without specifying `fieldSet`, you will retrieve only the specified fields.

```json
{        
    "output": {
      "fields": [
        "datasetName", 
        "occurrence.occurrenceId", 
        "location.municipality", 
        "taxon.vernacularName", 
        "event.startDate", 
        "event.endDate"
      ]
   }
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
            "endDate": "2014-06-07T00:00:00+02:00",
            "startDate": "2014-06-07T00:00:00+02:00"
        }
    }]
}
```
