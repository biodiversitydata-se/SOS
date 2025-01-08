# OGC Web Feature Service (WFS) - Technical overview
All public observations that SOS harvests are available in a direct access OGC Web Feature Service (WFS) described on this page. A [SOS WFS get started guide](WfsServiceGetStarted.md) is available on a separate page.
- [WFS service overview](#wfs-service-overview)
- [Service details](#service-details)
- [HTTP Request examples](#http-request-examples)
- [QGIS query examples](#qgis-query-examples)
- [Fields](#fields)
- [INSPIRE](#inspire)
- [Known problems](#known-problems)
- [Support](#support)

## WFS service overview
| Name  	| Value 	|
|:---	|:---	|
| URL | https://sosgeo.artdata.slu.se/geoserver/SOS/ows |
| Layer | SOS:SpeciesObservations |
| Max returned features in a request | 5 000 |
| Max paging startIndex | 100 000 |

## Service details
- For the moment the only supported WFS version is 1.0.0
- The service doesn't support the `sortBy` parameter in the `GetFeature` request. The default sorting when paging is to sort by `endDate` descending.
- The maximum number of observations returned in a request is 5 000. If you need to retrieve more observations, you need to use a filter or paging.
- The maximum paging startIndex is 100 000. If you need to retrieve more observations, you need to use a filter to split your search into several requests.
- The vocabulary values are always translated into swedish language.
- The service currently only supports public observations that SOS harvests. Observations of species classified sensitive (["nationellt skyddsklassade arter"](https://www.artdatabanken.se/var-verksamhet/fynddata/skyddsklassade-arter/)) can be accessed (given the user has permission) by direct requests to the SOS API (downloading results as shape files to import to a GIS application) or using the [Analysportalen](https://www.analysisportal.se/) (to be replaced by a new application during 2023).

## HTTP Request examples

### Get observations in GeoJSON format
https://sosgeo.artdata.slu.se/geoserver/SOS/ows?service=wfs&version=2.0.0&request=GetFeature&typeName=SOS:SpeciesObservations&outputFormat=application/json&count=10

### Get observations in SWEREF99 TM coordinate system
https://sosgeo.artdata.slu.se/geoserver/SOS/ows?service=wfs&version=2.0.0&request=GetFeature&typeName=SOS:SpeciesObservations&outputFormat=application/json&count=10&srsName=EPSG:3006

### Get observations of a specific organism group by using CQL filter
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
| isPresentObservation | boolean | true | Indicates whether this observation is a present or absent observation (the species you were looking for was either observed or not observed). |
| dataProviderId | integer | 1 | [Data provider id](DataProviders.md). |
| datasetName | string | "Artportalen" | The name identifying the dataset from which the record was derived. |
| startDate | date | "2021-10-30T00:00:00.000" | Start date and time of the event. |
| endDate | date | "2021-10-30T00:00:00.000" | End date and time of the event. |
| modified | date | "2022-01-12T11:38:37.800" | The most recent date-time on which the resource was changed. |
| decimalLatitude | double | 58.32955 | The geographic latitude of the geographic center of a Location (WGS84). |
| decimalLongitude | double | 11.56898 | The geographic longitude of the geographic center of a Location (WGS84). |
| coordinateUncertaintyInMeters | int32 | 100 | The horizontal distance (in meters) from the given CoordinateX and CoordinateY describing the smallest circle containing the whole of the Location. |
| locality | string | "Jordfalls hamn, Boh" | The specific description of the place. |
| municipality | string | "Uddevalla" | The municipality ('kommun' in Swedish) in which the Location occurs. |
| county | string | "Västra Götaland" | The county ('län' in Swedish) in which the Location occurs. |
| province | string | "Bohuslän" | The province ('landskap' in Swedish) in which the Location occurs. |
| institutionCode | string | "Landsorts fågelstation" | The name (or acronym) in use by the institution having custody of the object(s) or information referred to in the record. |
| recordedBy | string | "Marie Stenberg, Chris Olofsson" | A list of names of people, groups, or organizations responsible for recording the original Occurrence. |
| reportedBy | string | "Marie Stenberg" | Name of the person that reported the species observation. |
| habitat | string | "Åker" | A category or description of the habitat in which the Event occurred. |
| occurrenceRemarks | string | "Floraundersökning kalkeffekter åt länsstyrelsen ej kalkat" | Comments or notes about the Occurrence. |
| eventRemarks | string | "Tall herbs and young trees mixed with old oaks." | Comments or notes about the Event. |
| dyntaxaTaxonId | integer | 102835 | Dyntaxa TaxonID. |
| taxonCategory | string | "Art" | Taxon category. Uses values from the [TaxonCategory vocabulary](Vocabularies.md#taxonCategory). |
| organismGroup | string | "Kräftdjur" | Common name of the organism group that observed species belongs to. Classification of species groups is the same as used in latest 'Red List of Swedish Species'. |
| vernacularName | string | "ullig trollhummer" | Vernacular name. |
| scientificName | string | "Galathea nexa" | Scientific name. |
| isRedlisted | boolean | true | True if redlist category assigned to taxon is one of CR, EN, VU, NT. |
| redlistCategory | string | "VU" | Redlist category for redlisted species or subspecies. Possible redlist values are DD (Data Deficient), EX (Extinct), RE (Regionally Extinct), CR (Critically Endangered), EN (Endangered), VU (Vulnerable), NT (Near Threatened). Not redlisted species or subspecies have no value in this property. |
| isProtectedByLaw | boolean | false | Indicates whether the species is protected by the law in Sweden ('fridlyst' in Swedish). |
| isInvasiveInSweden | boolean | false | True if exotic/invasive ('främmande') in Sweden. |
| isInvasiveEu | boolean | false | True if exotic/invasive ('främmande') in Sweden according to EU Regulation 1143/2014. |
| invasiveRiskCategory | string | "SE" | Invasiveness risk assessment category. |
| actionPlan | string | "Fastställt" | Indicates whether the species is the subject of an action plan ('åtgärdsprogram' in Swedish). |
| isHabitatsDirectiveAnnex2 | boolean | false | Indicates whether the taxon is part of Habitats directive Annex 2. https://ec.europa.eu/environment/nature/legislation/habitatsdirective/index_en.htm|
| isHabitatsDirectiveAnnex4 | boolean | false | Indicates whether the taxon is part of Habitats directive Annex 4. https://ec.europa.eu/environment/nature/legislation/habitatsdirective/index_en.htm|
| isHabitatsDirectiveAnnex5 | boolean | false | Indicates whether the taxon is part of Habitats directive Annex 5. https://ec.europa.eu/environment/nature/legislation/habitatsdirective/index_en.htm|
| isVerified | boolean | true | Indicates whether the occurrence is verified by a species expert commitee. A varying subset of rare and/or conservation relevant species occurrences are reviewed by national expert commitees. https://www.artdatabanken.se/var-verksamhet/fynddata/sa-arbetar-artdatabanken-med-validering/|
| isUncertainIdentification | boolean | false | True if determination is uncertain. |
| activity | string | "födosökande" | A description of the activity shown by the subject at the time the occurrence was recorded. Uses values from the [Activity vocabulary](Vocabularies.md#activity) |
| lifeStage | string | "imago/adult" | The age class or life stage of the biological individual(s) at the time the occurrence was recorded. Uses values from the [LifeStage vocabulary](Vocabularies.md#lifeStage) |
| gender | string | "hona" | The gender of the biological individual(s) represented in the Occurrence. Uses values from the [Sex vocabulary](Vocabularies.md#sex) |
| substrate | string | "Hårdbotten" | Substrate. Uses values from the [Substrate vocabulary](Vocabularies.md#substrate) |
| organismQuantity | integer | 1 | The quantity of organisms. |
| organismQuantityUnit | string | "ex." | The type of quantification system used for the quantity of organisms. Uses values from the [Unit vocabulary](Vocabularies.md#unit)|
| isNaturalOccurrence | boolean | true | Indicates if this species occurrence is natural or if it is a result of human activity. |
| isNeverFoundObservation | boolean | false | Indicates if this observation is a never found observation. "Never found" is an observation documenting that the specified species was not found in a location deemed appropriate for the species. |
| isNotRediscoveredObservation | boolean | false | Indicates if this observation is a not rediscovered observation. "Not rediscovered" is an observation documenting that the specified species was not found in a location where it has been observed previously. |
| birdNestActivityId | integer | 4 | Uses id values from the [BirdNestActivity vocabulary](Vocabularies.md#birdNestActivity) Missing value codes: Bird nest activity id. 0 if species is not a bird. Bird occurrences without an activity get the value 1000000.|
| project1Id | integer | 3503 | Project id (for projects in Artportalen dataset). |
| project1Name | string | "Marint faunaväkteri" | Project name. |
| project1Category | string | "Faunaväkteri" | Project category. |
| project1Url | string | "https://www.artportalen.se/Project/View/3503" | Project URL. |
| project1Values | string | "[Typ av substrat=block], [Typ av biotop=klippvägg/bergvägg], [Övrigt=Funnen i spricka i klippväggen]" | Project parameter values. |
| project2Id | integer | 0 | Project 2 id (if the observation belongs to two projects). |
| project2Name | string |  | Project 2 name (if the observation belongs to two projects). |
| project2Category | string |  | Project category (if the observation belongs to two projects). |
| project2Url | string |  | Project URL (if the observation belongs to two projects). |
| project2Values | string |  | Project parameter values (if the observation belongs to two projects). |

## INSPIRE
The INSPIRE layers: SpeciesDistribution and HabitatsDistribution are available using the following URL:

| Name  	| Value 	|
|:---	|:---	|
| URL | https://sosgeo.artdata.slu.se/geoserver/wfs |

## Known problems
The WFS is using [GeoServer](https://geoserver.org/) and a plugin to GeoServer that has a [bug](https://github.com/ngageoint/elasticgeo/issues/122) leading to that requests sometimes stop being processed and no observations are returned until the server is restarted. This problem occurs about once a month. Currently we are restarting GeoServer once a day to try avoid that this problem affects users of the WFS.

### No observations - namespace 'null' problem:
Only V 1.0.0 of WFS is supported currently. For security reasons, we upgraded to GeoServer 2.25.2 during summer 2024, but unfortunately GML3 and WFS 2.0.0 stopped working. GeoServer returns a response, but the namespace of the layer becomes null, which means that most applications cannot interpret the data. We have reported the case to GeoServer, but do not currently know when it is planned to be fixed.
Currently only GML2 and WFS 1.0.0 are working. We therefore recommend changing version in your GIS application as a work-around for the time being. Version can be specified when adding a WFS server connection.

**Want to help?**

If you are a Java developer and want to help us fix the bug, you can find more information about the problem in the following links:
- [Jira ticket](https://osgeo-org.atlassian.net/jira/software/c/projects/GEOS/issues/GEOS-11510?jql=project%20%3D%20%22GEOS%22%20AND%20textfields%20~%20%22gml3%22%20ORDER%20BY%20created%20DESC)
- [GeoTools file - ElasticDataStoreFactory.java](https://github.com/geotools/geotools/blob/main/modules/unsupported/elasticsearch/src/main/java/org/geotools/data/elasticsearch/ElasticDataStoreFactory.java)
*- unlike all other datastores, the ElasticSearchDataStoreFactory lacks a NAMESPACE parameter, so GeoServer cannot inject the workspace-associated namespace URI into it.*

## Support
In case of questions or problems, contact support at SLU Artdatabanken: artdatabanken@slu.se
