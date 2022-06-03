# OGC Web Feature Service (WFS)
All public observations that SOS harvest are available in a OGC Web Feature Service (WFS) described on this page.
- [WFS service overview](#wfs-service-overview)
- [Service details](#service-details)
- [HTTP Request examples](#http-request-examples)
- [QGIS query examples](#qgis-query-examples)
- [Fields](#fields)

## WFS service overview
| Name  	| Value 	|
|:---	|:---	|
| URL | https://sosgeo.artdata.slu.se/geoserver/SOS/ows |
| Layer | SOS:SpeciesObservations |
| Max returned features in a request | 5 000 |
| Max paging startIndex | 100 000 |

## Service details
- The service doesn't support the `sortBy` parameter ing the `GetFeature` request. The default sorting when paging is to sort by `endDate` descending.
- The maximum number of observations returned in a request is 5 000. If you need to retrieve more observations, you need to use a filter or paging.
- The maximum paging startIndex is 100 000. If you need to retrieve more observations, you need to use a filter.
- The vocabulary values are always translated into swedish language.

## HTTP Request examples

### Get observations in GeoJSON format
https://sosgeo.artdata.slu.se/geoserver/SOS/ows?service=wfs&version=2.0.0&request=GetFeature&typeName=SOS:SpeciesObservations&outputFormat=application/json&count=10

### Get observations in SWEREF99 TM coordinate system
https://sosgeo.artdata.slu.se/geoserver/SOS/ows?service=wfs&version=2.0.0&request=GetFeature&typeName=SOS:SpeciesObservations&outputFormat=application/json&count=10&srsName=EPSG:3006

### Get observations of specific organism group by using CQL filter
[https://sosgeo.artdata.slu.se/geoserver/SOS/ows?service=wfs&version=2.0.0&request=GetFeature&typeName=SOS:SpeciesObservations&outputFormat=application/json&count=10&CQL_Filter=organismGroup='Kärlväxter'](https://sosgeo.artdata.slu.se/geoserver/SOS/ows?service=wfs&version=2.0.0&request=GetFeature&typeName=SOS:SpeciesObservations&outputFormat=application/json&CQL_Filter=organismGroup%20=%20%27K%C3%A4rlv%C3%A4xter%27&count=10)

### Get observations by using paging
https://sosgeo.artdata.slu.se/geoserver/SOS/ows?SERVICE=WFS&Request=GetFeature&Version=2.0.0&Typenames=SOS:SpeciesObservations&StartIndex=20&count=10&outputFormat=application/json

## QGIS query examples

### Get present observations
`isPresentObservation = 1`

### Get invasive observations in a municipality
`(isInvasiveInSweden=1 OR isInvasiveEu=1) AND municipality='Ydre'`

### Get observations between two dates
`endDate >= '2022-05-01' AND endDate <= '2022-05-31'`

### Get observations containing specific phrase in occurrenceRemarks
`occurrenceRemarks LIKE '%björkstam%'`

### Get observations that have project parameter values
`project1Values IS NOT NULL`

## Fields
| Field 	| Type 	| Example value	| Description |
|:---	|:---	|:---	|:---	|
| occurrenceId | string	| "urn:lsid:artportalen.se:sighting:98072725" | A globally unique identifier for the observation. |
| url | string | "https://www.artportalen.se/sighting/98072725" | URL to the observation. |
| isPresentObservation | boolean | true | Indicates whether this observation is a present or absent observation (the species you were looking for was not seen). |
| dataProviderId | integer | 1 | [Data provider id](DataProviders.md). |
| datasetName | string | "Artportalen" | The name identifying the data set from which the record was derived. |
| startDate | date | "2021-10-30T00:00:00.000" | Start date and time of the event. |
| endDate | date | "2021-10-30T00:00:00.000" | End date and time of the event. |
| modified | date | "2022-01-12T11:38:37.800" | The most recent date-time on which the resource was changed. |
| decimalLatitude | double | 58.32955 | The geographic latitude of the geographic center of a Location (WGS84). |
| decimalLongitude | double | 11.56898 | The geographic longitude of the geographic center of a Location (WGS84). |
| coordinateUncertaintyInMeters | int32 | 100 | The horizontal distance (in meters) from the given CoordinateX and CoordinateY describing the smallest circle containing the whole of the Location. |
| locality | string | "Jordfalls hamn, Boh" | The specific description of the place. |
| municipality | string | "Uddevalla" | The municipality ('kommun' in swedish) in which the Location occurs. |
| county | string | "Västra Götaland" | The county ('län' in swedish) in which the Location occurs. |
| province | string | "Bohuslän" | The province ('landskap' in swedish) in which the Location occurs. |
| institutionCode | string | "Landsorts fågelstation" | The name (or acronym) in use by the institution having custody of the object(s) or information referred to in the record. |
| recordedBy | string | "Marie Stenberg, Chris Olofsson" | A list of names of people, groups, or organizations responsible for   recording the original Occurrence. |
| reportedBy | string | "Marie Stenberg" | Name of the person that reported the species observation. |
| habitat | string | "Åker" | A category or description of the habitat in which the Event occurred. |
| occurrenceRemarks | string | "Floraundersökning kalkeffekter åt länsstyrelsen ej kalkat" | Comments or notes about the Occurrence. |
| eventRemarks | string | "Tall herbs and young trees mixed with old oaks." | Comments or notes about the Event. |
| dyntaxaTaxonId | integer | 102835 | Dyntaxa TaxonID. |
| taxonCategory | string | "Art" | Taxon category. Uses values from the [TaxonCategory vocabulary](Vocabularies.md#taxonCategory). |
| organismGroup | string | "Kräftdjur" | Common name of the organism group that observed species belongs to. Classification of species groups is the same as used in latest 'Red List of Swedish Species'. |
| vernacularName | string | "ullig trollhummer" | Vernacular name. |
| scientificName | string | "Galathea nexa" | Scientific name. |
| isRedlisted | boolean | true | True if redlist category is one of CR, EN, VU, NT. |
| redlistCategory | string | "VU" | Redlist category for redlisted species. Possible redlist values are DD (Data Deficient), EX (Extinct), RE (Regionally Extinct), CR (Critically Endangered), EN (Endangered), VU (Vulnerable), NT (Near Threatened). Not redlisted species has no value in this property. |
| isProtectedByLaw | boolean | false | Indicates whether the species is protected by the law in Sweden (fridlyst). |
| isInvasiveInSweden | boolean | false | True if invasive in sweden. |
| isInvasiveEu | boolean | false | True if invasive in sweden according to EU Regulation 1143/2014. |
| invasiveRiskCategory | string | "SE" | Invasive risk assessment category. |
| actionPlan | string | "Fastställt" | Indicates whether the species is the subject of an action plan ('åtgärdsprogram' in swedish). |
| isHabitatsDirectiveAnnex2 | boolean | false | Indicates whether the taxon is part of Habitats directive Annex 2. |
| isHabitatsDirectiveAnnex4 | boolean | false | Indicates whether the taxon is part of Habitats directive Annex 4. |
| isHabitatsDirectiveAnnex5 | boolean | false | Indicates whether the taxon is part of Habitats directive Annex 5. |
| isVerified | boolean | true | Indicates whether the occurrence is verified. |
| isUncertainIdentification | boolean | false | True if determination is uncertain. |
| activity | string | "födosökande" | A description of the activity shown by the subject at the time the Occurrence was recorded. Uses values from the [Activity vocabulary](Vocabularies.md#activity) |
| lifeStage | string | "imago/adult" | The age class or life stage of the biological individual(s) at the time the Occurrence was recorded. Uses values from the [LifeStage vocabulary](Vocabularies.md#lifeStage) |
| gender | string | "hona" | The gender of the biological individual(s) represented in the Occurrence. Uses values from the [Sex vocabulary](Vocabularies.md#sex) |
| substrate | string | "Hårdbotten" | Substrate. Uses values from the [Substrate vocabulary](Vocabularies.md#substrate) |
| organismQuantity | integer | 1 | The quantity of organisms. |
| organismQuantityUnit | string | "ex." | The type of quantification system used for the quantity of organisms. Uses values from the [Unit vocabulary](Vocabularies.md#unit)|
| isNaturalOccurrence | boolean | true | Indicates if this species occurrence is natural or if it is a result of human activity. |
| isNeverFoundObservation | boolean | false | Indicates if this observation is a never found observation. "Never found observation" is an observation that says that the specified species was not found in a location deemed appropriate for the species. |
| isNotRediscoveredObservation | boolean | false | Indicates if this observation is a not rediscovered observation. "Not rediscovered observation" is an observation that says that the specified species was not found in a location where it has previously been observed. |
| birdNestActivityId | integer | 4 | Bird nest activity id. 0 if not a bird. Bird occurrences without an activity gets the value 1000000. Uses id values from the [BirdNestActivity vocabulary](Vocabularies.md#birdNestActivity)|
| project1Id | integer | 3503 | Project id. |
| project1Name | string | "Marint faunaväkteri" | Project name. |
| project1Category | string | "Faunaväkteri" | Project category. |
| project1Url | string | "https://www.artportalen.se/Project/View/3503" | Project URL. |
| project1Values | string | "[Typ av substrat=block], [Typ av biotop=klippvägg/bergvägg], [Övrigt=Funnen i spricka i klippväggen]" | Project parameter values |
| project2Id | integer | 0 | Project 2 id (if the observation belongs to two projects). |
| project2Name | string |  | Project 2 name (if the observation belongs to two projects). |
| project2Category | string |  | Project category (if the observation belongs to two projects). |
| project2Url | string |  | Project URL (if the observation belongs to two projects). |
| project2Values | string |  | Project parameter values (if the observation belongs to two projects). |