# Search filters: Nature data - systematic species observations API
This page provides information about how to use the search filter parameters.

**Table of contents**
- [DatasetIds filter](#datasetids-filter)
- [EventIds filter](#eventids-filter)
- [Date filter](#date-filter)
	- [OverlappingStartDateAndEndDate filter type](#overlappingstartdateandenddate-filter-type)
	- [BetweenStartDateAndEndDate filter type](#betweenstartdateandenddate-filter-type)
	- [OnlyStartDate filter type](#onlystartdate-filter-type)
	- [OnlyEndDate filter type](#onlyenddate-filter-type)
- [Taxon filter](#taxon-filter)
- [Geographics filter](#geographics-filter)
	- [County (län) filter](#county-län-filter)
	- [Province (landskap) filter](#province-landskap-filter)
	- [Municipality (kommun) filter](#municipality-kommun-filter)
	- [Parish (socken) filter](#parish-socken-filter)
	- [Polygon geometry filter](#polygon-geometry-filter)
	- [Circle geometry filter](#circle-geometry-filter)

## DatasetIds filter
Filter by a list of dataset identifiers.
- [Overview of the datasets in the API](datasets.md).

**Request sample - Search datasets by datasetIds filter**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/datasets
Content-Type: application/json

{
  "datasetIds": [
    "Artportalen - Fladdermöss"
  ]
}
```

**Response**
```json
{
  "skip": 0,
  "take": 20,
  "count": 1,
  "totalCount": 1,
  "records": [
    {
      "identifier": "Artportalen - Fladdermöss",
      "title": "Fladdermöss",
      "...": "..."
    }
  ]
}
```

**Request sample - Search events by datasetIds filter**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/events?take=1
Content-Type: application/json

{
  "datasetIds": [
    "Artportalen - Fladdermöss"
  ]
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 2724,
  "records": [
    {
      "eventID": "urn:lsid:swedishlifewatch.se:dataprovider:Artportalen:event:10003985154952080918",
      "dataset": {
        "identifier": "Artportalen - Fladdermöss"
      },
      "...": "..."
    }
  ]
}
```

## EventIds filter
Filter by a list of eventIds.

**Request sample - Search events by eventIds filter**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/events?take=1
Content-Type: application/json

{
  "eventIds": [
    "urn:lsid:swedishlifewatch.se:dataprovider:Artportalen:event:10003985154952080918"
  ]
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 1,
  "records": [
    {
      "eventID": "urn:lsid:swedishlifewatch.se:dataprovider:Artportalen:event:10003985154952080918",
      "...": "..."
    }
  ]
}
```

## Date filter

### OverlappingStartDateAndEndDate filter type
This filter will return occurrences/events/datasets where `observation.event.startDate` is less than or equal to `endDate` and `observation.event.endDateDate` is greater than or equal to `startDate`. This is the **default** filter type used when no filter type is specified.

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

**Request sample - Search occurrences by date filter type OverlappingStartDateAndEndDate**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/occurrences?take=1
Content-Type: application/json

{
  "dateFilter": {
    "startDate": "2021-06-16",
    "endDate": "2021-06-17",
    "dateFilterType": "OverlappingStartDateAndEndDate"
  }
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 997,
  "records": [
    {
      "occurrenceID": "urn:lsid:artportalen.se:sighting:98571202",
      "eventStartDate": "2021-06-14T19:40:00Z",
      "eventEndDate": "2021-06-16T02:10:00Z",
      "...": "..."
    }
  ]
}
```

### BetweenStartDateAndEndDate filter type
This filter will return occurrences/events/datasets where `observation.event.startDate` is greater than or equal to `startDate` and `observation.event.endDate` is less than or equal to `endDate`.

| Observation date | Match filter? |
|-|-|
| obs.event.startDate=2021-02-02<br>obs.event.endDate=2021-02-03 | Yes |
| obs.event.startDate=2021-02-03<br>obs.event.endDate=2021-02-04 | Yes |
| obs.event.startDate=2021-02-04<br>obs.event.endDate=2021-02-05 | No |
| obs.event.startDate=2021-02-02<br>obs.event.endDate=2021-02-04 | Yes |
| obs.event.startDate=2021-02-02<br>obs.event.endDate=2021-02-05 | No |
| obs.event.startDate=2021-02-01<br>obs.event.endDate=2021-02-04 | No |

**Request sample - Search occurrences by date filter type BetweenStartDateAndEndDate**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/occurrences?take=1
Content-Type: application/json

{
  "dateFilter": {
    "startDate": "2021-06-16",
    "endDate": "2021-06-17",
    "dateFilterType": "BetweenStartDateAndEndDate"
  }
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 997,
  "records": [
    {
      "occurrenceID": "urn:lsid:artportalen.se:sighting:98571732",
      "eventStartDate": "2021-06-16T20:00:00Z",
      "eventEndDate": "2021-06-16T22:00:00Z",
      "...": "..."
    }
  ]
}
```

### OnlyStartDate filter type

This filter will return occurrences/events/datasets where `observation.event.startDate`  is between `startDate` and `endDate`.

| Observation date | Match filter? |
|-|-|
| obs.event.startDate=2021-02-02 | Yes |
| obs.event.startDate=2021-02-03 | Yes |
| obs.event.startDate=2021-02-04 | No |

**Request sample - Search occurrences by date filter type OnlyStartDate**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/occurrences?take=1
Content-Type: application/json

{
  "dateFilter": {
    "startDate": "2021-06-16",
    "endDate": "2021-06-17",
    "dateFilterType": "OnlyStartDate"
  }
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 259,
  "records": [
    {
      "occurrenceID": "urn:lsid:artportalen.se:sighting:98571198",
      "eventStartDate": "2021-06-16T20:15:00Z",    
      "...": "..."
    }
  ]
}
```


### OnlyEndDate filter type

This filter will return occurrences/events/datasets where `observation.event.endDate`  is between `startDate` and `endDate`.

| Observation date | Match filter? |
|-|-|
| obs.event.endDate=2021-02-02 | Yes |
| obs.event.endDate=2021-02-03 | Yes |
| obs.event.endDate=2021-02-04 | No |

**Request sample - Search occurrences by date filter type OnlyEndDate**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/occurrences?take=1
Content-Type: application/json

{
  "dateFilter": {
    "startDate": "2021-06-16",
    "endDate": "2021-06-17",
    "dateFilterType": "OnlyEndDate"
  }
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 242,
  "records": [
    {
      "occurrenceID": "urn:lsid:artportalen.se:sighting:98571182",
      "eventEndDate": "2021-06-16T02:10:00Z",
      "...": "..."
    }
  ]
}
```

## Taxon filter
This filter will return occurrences/events/datasets where `observation.taxon.taxonID` is matching any of the ids specified in the taxon filter.

**Request sample - Search occurrences by taxonId filter**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/occurrences?take=1
Content-Type: application/json

{
  "taxon": {
    "ids": [100077]
  }
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 11199,
  "records": [
    {
      "occurrenceID": "urn:lsid:artportalen.se:sighting:104037867",
      "taxon": {
        "taxonID": "100077",
        "vernacularName": "utter",
        "scientificName": "Lutra lutra"
      },
      "...": "..."
    }
  ]
}
```

## Geographics filter

### County (län) filter
Filter by [county enum](areas.md#county-lan).

**Request sample - Search events by county filter**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/events?take=1
Content-Type: application/json

{
  "area": {
    "county": "JönköpingsLän"
  }
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 173733,
  "records": [
    {
      "eventID": "urn:lsid:swedishlifewatch.se:dataprovider:Artportalen:event:10000032873271697869",
      "surveyLocation": {
        "county": "JönköpingsLän",
        "province": "Småland",
        "municipality": "Gislaved",
        "parish": "Burseryd"
      },
      "...": "..."
    }
  ]
}
```

### Province (landskap) filter
Filter by [province enum](areas.md#province-landskap).

**Request sample - Search events by province filter**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/events?take=1
Content-Type: application/json

{
  "area": {
    "county": "Östergötland"
  }
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 173733,
  "records": [
    {
      "eventID": "urn:lsid:swedishlifewatch.se:dataprovider:Artportalen:event:10000027069520469625",
      "surveyLocation": {
        "county": "ÖstergötlandsLän",
        "province": "Östergötland",
        "municipality": "Linköping",
        "parish": "Linköping"
      },
      "...": "..."
    }
  ]
}
```

### Municipality (kommun) filter
Filter by [municipality enum](areas.md#municipality-kommun).

**Request sample - Search events by municipality filter**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/events?take=1
Content-Type: application/json

{
  "area": {
    "county": "Tranås"
  }
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 173733,
  "records": [
    {
      "eventID": "urn:lsid:swedishlifewatch.se:dataprovider:Artportalen:event:10026357048434813440",
      "surveyLocation": {
        "county": "JönköpingsLän",
        "province": "Småland",
        "municipality": "Tranås",
        "parish": "Linderås"
      },
      "...": "..."
    }
  ]
}
```

### Parish (socken) filter
Filter by [parish enum](areas.md#parish-socken).

**Request sample - Search events by parish filter**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/events?take=1
Content-Type: application/json

{
  "area": {
    "parish": "Tystberga"
  }
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 173733,
  "records": [
    {
      "eventID": "urn:lsid:swedishlifewatch.se:dataprovider:Artportalen:event:10000373257902201658",
      "surveyLocation": {
        "county": "SödermanlandsLän",
        "province": "Södermanland",
        "municipality": "Nyköping",
        "parish": "Tystberga"
      },
      "...": "..."
    }
  ]
}
```

### Polygon geometry filter
This filter will return observations within the specified polygon. The coordinates should be specified in the WGS84 coordinate system.

**Request sample - Search events by polygon geometry filter**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/events?take=1
Content-Type: application/json

{
  "area": {
    "geometry": {
      "geographicArea": {
        "type": "polygon",
        "coordinates": [
          [
            [16.07063, 56.92573],
            [15.0051, 58.16108],
            [14.58003, 58.10148],
            [14.64143, 57.93294],
            [16.07063, 56.92573]
          ]
        ]
      }
    }
  }
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 22578,
  "records": [
    {
      "eventID": "urn:lsid:swedishlifewatch.se:dataprovider:Artportalen:event:10000146541482139962",
      "surveyLocation": {
        "emplacement": {
          "coordinates": [
            15.480811021677173,
            57.4938575247702
          ],
          "type": "point"
        },
        "county": "JönköpingsLän",
        "province": "Småland",
        "municipality": "Vetlanda",
        "parish": "Karlstorp"
      },
      "...": "..."
    }
  ]
}
```


### Circle geometry filter
This filter will return observations within the specified circle with a radius of *maxDistanceFromGeometries*. The coordinates should be specified in the WGS84 coordinate system.

**Request sample - Search events by circle filter**
```http
POST https://api.artdatabanken.se/data-stewardship-api/v1/events?take=1
Content-Type: application/json

{
  "area": {
    "geometry": {
      "geographicArea": {
        "type": "point",
        "coordinates": [14.99047, 58.01563]
      },
      "maxDistanceFromGeometries": 500
    }
  }
}
```

**Response**
```json
{
  "skip": 0,
  "take": 1,
  "count": 1,
  "totalCount": 10,
  "records": [
    {
      "eventID": "urn:lsid:swedishlifewatch.se:dataprovider:Artportalen:event:1036309339374928699",      
      "surveyLocation": {        
        "emplacement": {
          "coordinates": [
            14.986962194232467,
            58.018534200433609
          ],
          "type": "point"
        },
        "county": "JönköpingsLän",
        "province": "Småland",
        "municipality": "Tranås",
        "parish": "Tranås_F",        
      },
      "...": "..."
    }
  ]
}
```