# Field sets
Several endpoints supports specifying which fields in an observation you want to retrieve using the fields and fieldSet parameter. There are four predefined field sets to choose from.

- [Minimum (default)](#Minimum)
- [Extended](#Extended) 
- [AllWithValues](#All)
- [All](#All)

## Minimum
| Field | Type | Example |
|:---|:---|:---|
| **Record level** 	| 	|  	|  	|
| &nbsp;&nbsp;datasetName | string | "Artportalen" |
| **Occurrence** 	| 	|  	|  	|
| &nbsp;&nbsp;occurrence.occurrenceId | string | "urn:lsid:artportalen.se:Sighting:3921008" |
| &nbsp;&nbsp;occurrence.occurrenceStatus | VocabularyValue[\<occurrenceStatus\>](Vocabularies.md#occurrenceStatus) | \{ "id":0, "value":"present" \} |
| &nbsp;&nbsp;occurrence.recordedBy | string | "Ove Sinatra" |
| &nbsp;&nbsp;occurrence.reportedBy | string | "Dennis Alnér" |
| &nbsp;&nbsp;occurrence.individualCount | string | "1" |
| &nbsp;&nbsp;occurrence.organismQuantity | string | "1" |
| &nbsp;&nbsp;occurrence.organismQuantityInt | int32 | 1 |
| &nbsp;&nbsp;occurrence.organismQuantityUnit | VocabularyValue[\<unit\>](Vocabularies.md#unit) | \{ "id":1, "value":"plantor/tuvor" \} |
| **Event** 	| 	|  	|  	|
| &nbsp;&nbsp;event.startDate | DateTime? | "2008-07-07T09:00:00Z" |
| &nbsp;&nbsp;event.endDate | DateTime? | "2008-07-07T10:30:00Z" |
| **Identification** 	| 	|  	|  	|
| &nbsp;&nbsp;identification.validated | boolean | true |
| &nbsp;&nbsp;identification.uncertainIdentification | boolean | false |
| **Location** 	| 	|  	|  	|
| &nbsp;&nbsp;location.decimalLatitude | double? | 57.4303 |
| &nbsp;&nbsp;location.decimalLongitude | double? | 16.66547 |
| &nbsp;&nbsp;location.coordinateUncertaintyInMeters | int32? | 100 |
| &nbsp;&nbsp;location.county | Area[\<county\>](Areas.md#County-Län) | \{ "featureId":"8", "name":"Kalmar" \} |
| &nbsp;&nbsp;location.municipality | Area[\<municipality\>](Areas.md#Municipality-Kommun) | \{ "featureId":"882", "name":"Oskarshamn" \} |
| **Taxon** 	| 	|  	|  	|
| &nbsp;&nbsp;taxon.id | int32 | 221501 |
| &nbsp;&nbsp;taxon.scientificName | string | "Anacamptis morio" |
| &nbsp;&nbsp;taxon.vernacularName | string | "göknycklar" |
| &nbsp;&nbsp;taxon.attributes.organismGroup | string | "Kärlväxter" |


## Extended
| Field | Type | Example |
|:---|:---|:---|
| **Record level** 	| 	|  	|  	|
| &nbsp;&nbsp;datasetName | string | "Artportalen" |
| &nbsp;&nbsp;collectionCode | string | "Artportalen" |
| &nbsp;&nbsp;dataProviderId | int32 | 1 |
| &nbsp;&nbsp;institutionCode | VocabularyValue[\<institutionCode\>](Vocabularies.md#institutionCode) | \{ id=433, value="Landsorts fågelstation" \} |
| &nbsp;&nbsp;ownerInstitutionCode | string | "SLU Artdatabanken" |
| &nbsp;&nbsp;basisOfRecord | VocabularyValue[\<basisOfRecord\>](Vocabularies.md#basisOfRecord) | \{ "id":0, "value":"HumanObservation" \} |
| &nbsp;&nbsp;rightsHolder 	| string | "Landsorts fågelstation" |
| &nbsp;&nbsp;modified 	| DateTime?	| "2008-07-18T13:19:00Z" |
| &nbsp;&nbsp;measurementOrFacts\[\]<br/>&nbsp;&nbsp;&nbsp;&nbsp;.measurementID<br/>&nbsp;&nbsp;&nbsp;&nbsp;.measurementType<br/>&nbsp;&nbsp;&nbsp;&nbsp;.measurementValue<br/>&nbsp;&nbsp;&nbsp;&nbsp;.measurementUnit | Collection[\<ExtendedMeasurementOrFact\>](https://tools.gbif.org/dwca-validator/extension.do?id=http://rs.iobis.org/obis/terms/ExtendedMeasurementOrFact) | [\{<br/>&nbsp;"measurementID" : "3503-592",<br/>&nbsp;"measurementType" : "Typ av substrat",<br/>&nbsp;"measurementValue" : "silt/lera"<br/>\}] |
| &nbsp;&nbsp;projects\[\]<br/>&nbsp;&nbsp;&nbsp;&nbsp;.id<br/>&nbsp;&nbsp;&nbsp;&nbsp;.name<br/>&nbsp;&nbsp;&nbsp;&nbsp;.owner<br/>&nbsp;&nbsp;&nbsp;&nbsp;.projectParameters\[\] | Collection\<Project\> | [\{<br/>&nbsp;&nbsp;"id" : 774,<br/>&nbsp;&nbsp;"name" : "Inventering av vedsvampar",<br/>&nbsp;&nbsp;"owner" : "Tom Volgers",<br/>&nbsp;&nbsp;"projectParameters" : [\{<br/>&nbsp;&nbsp;&nbsp;&nbsp;"id" : 18,<br/>&nbsp;&nbsp;&nbsp;&nbsp;"name" : "Trädvitalitet",<br/>&nbsp;&nbsp;&nbsp;&nbsp;"value" : "Naturlig låga"<br/>&nbsp;&nbsp;&nbsp;&nbsp;"dataType" : "string"<br/>&nbsp;\}]<br/>\}] |
| **Occurrence** 	| 	|  	|  	|
| &nbsp;&nbsp;occurrence.occurrenceId | string | "urn:lsid:artportalen.se:Sighting:3921008" |
| &nbsp;&nbsp;occurrence.occurrenceStatus | VocabularyValue[\<occurrenceStatus\>](Vocabularies.md#occurrenceStatus) | \{ "id":0, "value":"present" \} |
| &nbsp;&nbsp;occurrence.recordedBy | string | "Ove Sinatra" |
| &nbsp;&nbsp;occurrence.reportedBy | string | "Dennis Alnér" |
| &nbsp;&nbsp;occurrence.individualCount | string | "1" |
| &nbsp;&nbsp;occurrence.organismQuantity | string | "1" |
| &nbsp;&nbsp;occurrence.organismQuantityInt | int32 | 1 |
| &nbsp;&nbsp;occurrence.organismQuantityUnit | VocabularyValue[\<unit\>](Vocabularies.md#unit) | \{ "id":1, "value":"plantor/tuvor" \} |
| &nbsp;&nbsp;occurrence.url | string | "http://www.artportalen.se/sighting/474560" |
| &nbsp;&nbsp;occurrence.associatedMedia | string | http://www.artportalen.se/Image/744511 |
| &nbsp;&nbsp;occurrence.occurrenceRemarks | string | "Floraundersökning   kalkeffekter åt länsstyrelsen ej kalkat" |
| &nbsp;&nbsp;occurrence.activity | VocabularyValue[\<activity\>](Vocabularies.md#activity) | \{ "id":25, "value":"födosökande" \} |
| &nbsp;&nbsp;occurrence.behavior | VocabularyValue[\<behavior\>](Vocabularies.md#behavior) | \{ "id":17, "value":"permanent revir" \} |
| &nbsp;&nbsp;occurrence.biotope | VocabularyValue[\<biotope\>](Vocabularies.md#biotope) | \{ "id":509, "value":"Vattenstrand" \} |
| &nbsp;&nbsp;occurrence.biotopeDescription | string | "havsstrandäng med låga kullar och fuktdrag" |
| &nbsp;&nbsp;occurrence.lifeStage | VocabularyValue[\<lifeStage\>](Vocabularies.md#lifeStage) | \{ "id":24, "value":"larv/nymf" \} |
| &nbsp;&nbsp;occurrence.reproductiveCondition | VocabularyValue[\<reproductiveCondition\>](Vocabularies.md#reproductiveCondition) | \{ "id":13, "value":"ruvfläckar" \} |
| &nbsp;&nbsp;occurrence.sex | VocabularyValue[\<sex\>](Vocabularies.md#sex) | \{ "id":2, "value":"hona" \} |
| &nbsp;&nbsp;occurrence.protectionLevel | int32 | 1 |
| &nbsp;&nbsp;occurrence.isNaturalOccurrence | boolean | true |
| &nbsp;&nbsp;occurrence.isNeverFoundObservation | boolean | false |
| &nbsp;&nbsp;occurrence.isNotRediscoveredObservation | boolean | false |
| &nbsp;&nbsp;occurrence.isPositiveObservation | boolean | true |
| &nbsp;&nbsp;occurrence.substrate.name | VocabularyValue[\<substrate\>](Vocabularies.md#substrate) | \{ "id":84, "value":"Död trädstam" \} |
| &nbsp;&nbsp;occurrence.length	| int32	| 487 |
| &nbsp;&nbsp;occurrence.weight	| int32	| 900 |
| **Event** 	| 	|  	|  	|
| &nbsp;&nbsp;event.startDate | DateTime? | "2008-07-07T09:00:00Z" |
| &nbsp;&nbsp;event.endDate | DateTime? | "2008-07-07T10:30:00Z" |
| &nbsp;&nbsp;event.habitat | string | "Åker" |
| &nbsp;&nbsp;event.eventRemarks | string | "Tall herbs and young trees mixed with old oaks." |
| &nbsp;&nbsp;event.samplingEffort | string | "1 day survey of a 100 m^2 area" |
| &nbsp;&nbsp;event.samplingProtocol | string | "ht<span>tps://ww<span>w.slu<span>.se/globalassets/ris_fin_2008.pdf" |
| &nbsp;&nbsp;event.sampleSizeUnit | string | "m^2" |
| &nbsp;&nbsp;event.sampleSizeValue | string | "100" |
| &nbsp;&nbsp;event.measurementOrFacts\[\]<br/>&nbsp;&nbsp;&nbsp;&nbsp;.measurementID<br/>&nbsp;&nbsp;&nbsp;&nbsp;.measurementType<br/>&nbsp;&nbsp;&nbsp;&nbsp;.measurementValue<br/>&nbsp;&nbsp;&nbsp;&nbsp;.measurementUnit | Collection[\<ExtendedMeasurementOrFact\>](https://tools.gbif.org/dwca-validator/extension.do?id=http://rs.iobis.org/obis/terms/ExtendedMeasurementOrFact) | [\{<br/>&nbsp;"measurementType" : "Vegetationsyteareal",<br/>&nbsp;"measurementValue" : "100",<br/>&nbsp;"measurementUnit" : "m^2"<br/>\}] |
| **Identification** 	| 	|  	|  	|
| &nbsp;&nbsp;identification.validated | boolean | true |
| &nbsp;&nbsp;identification.uncertainIdentification | boolean | false |
| &nbsp;&nbsp;identification.validationStatus | VocabularyValue[\<validationStatus\>](Vocabularies.md#validationStatus) | \{ "id":60, "value":"Godkänd baserat på observatörens uppgifter" \} |
| &nbsp;&nbsp;identification.confirmedBy | string | "Gerhard Boré" |
| &nbsp;&nbsp;identification.identifiedBy | string | "Mårten Ilidasch" |
| &nbsp;&nbsp;identification.verifiedBy | string | "Lennart Lasseman" |
| &nbsp;&nbsp;identification.determinationMethod | VocabularyValue[\<determinationMethod\>](Vocabularies.md#determinationMethod) | \{ "id":3, "value":"stereolupp" \} |
| **Location** 	| 	|  	|  	|
| &nbsp;&nbsp;location.decimalLatitude | double? | 57.4303 |
| &nbsp;&nbsp;location.decimalLongitude | double? | 16.66547 |
| &nbsp;&nbsp;location.coordinateUncertaintyInMeters | int32? | 100 |
| &nbsp;&nbsp;location.county | Area[\<county\>](Areas.md#County-Län) | \{ "featureId":"8", "name":"Kalmar" \} |
| &nbsp;&nbsp;location.municipality | Area[\<municipality\>](Areas.md#Municipality-Kommun) | \{ "featureId":"882", "name":"Oskarshamn" \} |
| &nbsp;&nbsp;location.locality | string | "Mosse 200 m SO om TÅNGEN, Vg" |
| &nbsp;&nbsp;location.province | Area[\<province\>](Areas.md#Province-Landskap) | \{ "featureId":"3", "name":"Småland" \} |
| &nbsp;&nbsp;location.parish | Area[\<parish\>](Areas.md#Parish-Socken) | \{ "featureId":"845", "name":"Misterhult" \} |
| &nbsp;&nbsp;location.geodeticDatum | string | "EPSG:4326" |
| &nbsp;&nbsp;location.locationId | string | "urn:lsid:artportalen.se:site:649345" |
| **Taxon** 	| 	|  	|  	|
| &nbsp;&nbsp;taxon.id | int32 | 221501 |
| &nbsp;&nbsp;taxon.scientificName | string | "Anacamptis morio" |
| &nbsp;&nbsp;taxon.vernacularName | string | "göknycklar" |
| &nbsp;&nbsp;taxon.attributes.organismGroup | string | "Kärlväxter" |
| &nbsp;&nbsp;taxon.genus | string | "Anacamptis" |
| &nbsp;&nbsp;taxon.family | string | "Orchidaceae" |
| &nbsp;&nbsp;taxon.order | string | "Asparagales" |
| &nbsp;&nbsp;taxon.class | string | "Liliopsida" |
| &nbsp;&nbsp;taxon.phylum | string | "Tracheophyta" |
| &nbsp;&nbsp;taxon.kingdom | string | "Plantae" |
| &nbsp;&nbsp;taxon.taxonId | string | "urn:lsid:dyntaxa.se:Taxon:221501" |
| &nbsp;&nbsp;taxon.attributes.protectionLevel | VocabularyValue[\<protectionLevel\>](Vocabularies.md#protectionLevel) | \{ "id":1, "value":"Fullständig åtkomst och fri användning för alla" \} |
| &nbsp;&nbsp;taxon.attributes.redlistCategory | string | "LC" |
| &nbsp;&nbsp;taxon.attributes.protectedByLaw | boolean? | true |

## AllWithValues
All fields that are described on the [Observation documentation page](Observation.md) where there exist at least one value, i.e. all that contain an example value.

## All
All fields that are described on the [Observation documentation page](Observation.md).
