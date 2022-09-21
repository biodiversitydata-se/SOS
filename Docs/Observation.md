# Observation object
All fields that are part of an observation are listed on this page.

## Fields

| Field 	| Type 	| Example 	| Description | Darwin Core 	|
|:---	|:---	|:---	|:---	|:---	|  
| **Record level** 	| 	|  	|  	|  	|
| dataProviderId 	| int32 	| 1 	| Data provider id. 	|  	|
| basisOfRecord 	| VocabularyValue[\<basisOfRecord\>](Vocabularies.md#basisOfRecord) | \{ "id":0, "value":"HumanObservation" \} 	| The specific nature of the data record. 	| https://dwc.tdwg.org/terms/#dwc:basisOfRecord 	|
| collectionCode 	| string 	| "Artportalen" 	| The name, acronym, coden, or initialism identifying the collection or data set from which the record was derived. 	| https://dwc.tdwg.org/terms/#dwc:collectionCode 	|
| datasetId 	| string 	| "urn:lsid:swedishlifewatch.se:dataprovider:Artportalen" 	| An identifier for the set of data. May be a global unique identifier or an identifier specific to a collection or institution. 	| https://dwc.tdwg.org/terms/#dwc:datasetID 	|
| datasetName 	| string 	| "Artportalen" 	| The name identifying the data set from which the record was derived. 	| https://dwc.tdwg.org/terms/#dwc:datasetName 	|
| institutionId 	| string 	| "urn:lsid:artdata.slu.se:organization:433" 	| An identifier for the institution having custody of the object(s) or information referred to in the record. 	| https://dwc.tdwg.org/terms/#dwc:institutionID 	|
| institutionCode 	| VocabularyValue[\<institutionCode\>](Vocabularies.md#institutionCode)	| \{ id=433, value="Landsorts fågelstation" \} 	| The name (or acronym) in use by the institution having custody of the object(s) or information referred to in the record. 	| https://dwc.tdwg.org/terms/#dwc:institutionCode 	|
| ownerInstitutionCode 	| string 	| "SLU Artdatabanken" 	| The name (or acronym) in use by the institution having ownership of the object(s) or information referred to in the record. 	| https://dwc.tdwg.org/terms/#dwc:ownerInstitutionCode 	|
| language 	| string 	| "sv" 	| A language of the resource. Recommended best practice is to use a controlled vocabulary such as RFC 4646 [RFC4646]. 	| https://dwc.tdwg.org/terms/#dwc:language 	|
| accessRights 	| VocabularyValue[\<accessRights\>](Vocabularies.md#accessRights)	| \{ "id": 0, "value":"Free usage" \} 	| Information about who can access the resource or an indication of its security status. 	| https://dwc.tdwg.org/terms/#dcterms:accessRights 	|
| license 	| string 	| "CC BY-NC 4.0" 	| A legal document giving official permission to do something with the resource. 	| https://dwc.tdwg.org/terms/#dwc:license 	|
| rightsHolder 	| string 	| "Landsorts fågelstation" 	| A person or organization owning or managing rights over the resource. 	| https://dwc.tdwg.org/terms/#dwc:rightsHolder 	|
| modified 	| DateTime? 	| "2008-07-18T13:19:00Z" 	| The most recent date-time on which the resource was changed (UTC). 	| https://dwc.tdwg.org/terms/#dwc:modified 	|
| sensitive 	| boolean 	| false 	| Indicates whether the observation is sensitive and therefore protected. 	|  	|
| publicCollection 	| string 	| "Uppsala-Evolutionsmuseet" 	| Public collection name. 	|  	|
| privateCollection 	| string 	| "Lennart Lasseman" 	| Private collection name. 	|  	|
| speciesCollectionLabel 	| string 	| "FA5361-3" 	| Species collection label. 	|  	|
| dynamicProperties 	| string 	| "{ "SiteSetByUser" : true }" 	| A list of additional measurements, facts, characteristics, or assertions about the record. 	| https://dwc.tdwg.org/terms/#dwc:dynamicProperties 	|
| measurementOrFacts 	| Collection[\<ExtendedMeasurementOrFact\>](https://tools.gbif.org/dwca-validator/extension.do?id=http://rs.iobis.org/obis/terms/ExtendedMeasurementOrFact)	| [\{<br/>&nbsp;"measurementID" : "3503-592",<br/>&nbsp;"measurementType" : "Typ av substrat",<br/>&nbsp;"measurementValue" : "silt/lera",<br/>&nbsp;"measurementDeterminedDate" : "2017-04-30T22:00:00Z",<br/>&nbsp;"measurementMethod" : "Dykning",<br/>&nbsp;"measurementRemarks" : "miljöbeskrivning. Artportalen project=\"Marint faunaväkteri\""<br/>\}] 	| Measurement or facts linked to the observation. 	| https://tools.gbif.org/dwca-validator/extension.do?id=http://rs.iobis.org/obis/terms/ExtendedMeasurementOrFact 	|
| projects 	| Collection\<Project\>	| [\{<br/>&nbsp;&nbsp;"id" : 774,<br/>&nbsp;&nbsp;"name" : "Inventering av vedsvampar",<br/>&nbsp;&nbsp;"isPublic" : false,<br/>&nbsp;&nbsp;"owner" : "Tom Volgers",<br/>&nbsp;&nbsp;"projectURL" : "https://www.artportalen.se/Project/View/774"<br/>&nbsp;&nbsp;"category" : "Fungi inventories",<br/>&nbsp;&nbsp;"categorySwedish" : "Storsvampinventering",<br/>&nbsp;&nbsp;"startDate" : "2013-08-31T22:00:00Z"<br/>&nbsp;&nbsp;"endDate" : "2013-10-30T23:00:00Z",<br/>&nbsp;&nbsp;"description" : "Project description..."<br/>&nbsp;&nbsp;"projectParameters" : [\{<br/>&nbsp;&nbsp;&nbsp;&nbsp;"id" : 18,<br/>&nbsp;&nbsp;&nbsp;&nbsp;"name" : "Trädvitalitet",<br/>&nbsp;&nbsp;&nbsp;&nbsp;"value" : "Naturlig låga"<br/>&nbsp;&nbsp;&nbsp;&nbsp;"dataType" : "string"<br/>&nbsp;\}]<br/>\}] | Projects from Artportalen associated with the observation. | |
| bibliographicCitation 	| string 	|  	| A bibliographic reference for the resource as a statement indicating how this record should be cited (attributed) when used. 	| https://dwc.tdwg.org/terms/#dcterms:bibliographicCitation 	|
| collectionId 	| string 	|  	| An identifier for the collection or dataset from which the record was derived. 	| https://dwc.tdwg.org/terms/#dwc:collectionID 	|
| dataGeneralizations 	| string 	|  	| Actions taken to make the shared data less specific or complete than in its original form. Suggests that alternative data of higher quality may be available on request. 	| https://dwc.tdwg.org/terms/#dwc:dataGeneralizations 	|
| informationWithheld 	| string 	|  	| Additional information that exists, but that has not been shared in the given record. 	| https://dwc.tdwg.org/terms/#dwc:informationWithheld 	|
| references 	| string 	|  	| A related resource that is referenced, cited, or otherwise pointed to by the described resource. 	| https://dwc.tdwg.org/terms/#dwc:references 	|
| type 	| VocabularyValue[\<type\>](Vocabularies.md#type)	|  	| The nature or genre of the resource. 	| https://dwc.tdwg.org/terms/#dwc:type 	|
| &nbsp;  	|  	|  	|  	|  	|
| **Event** 	| 	|  	|  	|  	|  
| event.startDate 	| DateTime? 	| "2008-07-07T11:00:00+02:00" 	| Start date/time of the event in ISO8601 Date format including time zone.    	|  	|
| event.endDate 	| DateTime? 	| "2008-07-07T12:30:00+02:00" 	| End date/time of the event in ISO8601 Date format including time zone.    	|  	|
| event.plainStartDate 	| string 	| "2008-07-07" 	| Start date of the event in the format yyyy-MM-dd.    	|  	|
| event.plainEndDate 	| string 	| "2008-07-07" 	| End date of the event in the format yyyy-MM-dd.    	|  	|
| event.plainStartTime 	| string 	| "11:00" 	| Start time of the event in W. Europe Standard Time formatted as hh:mm.    	|  	|
| event.plainEndTime 	| string 	| "12:30" 	| End time of the event in W. Europe Standard Time formatted as hh:mm.    	|  	|
| event.eventId 	| string 	| "1A474222-F49B-46C4-A7AB-BCD798A04A36" 	| An identifier   for the set of information associated with an Event (something that occurs at   a place and time).  	| https://dwc.tdwg.org/terms/#dwc:eventID 	|
| event.eventRemarks 	| string 	| "Tall herbs and young trees mixed with old oaks." 	| Comments or notes about the Event.    	| https://dwc.tdwg.org/terms/#dwc:eventRemarks 	|
| event.discoveryMethod | VocabularyValue[\<discoveryMethod\>](Vocabularies.md#discoveryMethod)	| \{ "id": 3, "value":"binoculars" \} 	| DiscoveryMethod from Artportalen 	|  	|
| event.habitat 	| string 	| "Åker" 	| A category or description of the habitat in which the Event occurred.    	| https://dwc.tdwg.org/terms/#dwc:habitat 	|
| event.samplingEffort 	| string 	| "1 day survey of a 100 m^2 area" 	| The amount of effort expended during an Event.    	| https://dwc.tdwg.org/terms/#dwc:samplingEffort 	|
| event.samplingProtocol 	| string 	| "ht<span>tps://ww<span>w.slu<span>.se/globalassets/ris_fin_2008.pdf"	| The name of, reference to, or description of the method or protocol used   during an Event.    	| https://dwc.tdwg.org/terms/#dwc:samplingProtocol 	|
| event.sampleSizeUnit 	| string 	| "m^2" 	| The unit of measurement of the size (time duration, length, area, or   volume) of a sample in a sampling event.    	| https://dwc.tdwg.org/terms/#dwc:sampleSizeUnit 	|
| event.sampleSizeValue 	| string 	| "100" 	| A numeric value for a measurement of the size (time duration, length,   area, or volume) of a sample in a sampling event.    	| https://dwc.tdwg.org/terms/#dwc:sampleSizeValue 	|
| event.verbatimEventDate 	| string 	| "1997-06-21" 	| The verbatim original representation of the date and time information for   an Event.    	| https://dwc.tdwg.org/terms/#dwc:verbatimEventDate 	|
| event.measurementOrFacts 	| Collection[\<ExtendedMeasurementOrFact\>](https://tools.gbif.org/dwca-validator/extension.do?id=http://rs.iobis.org/obis/terms/ExtendedMeasurementOrFact)	|  [\{<br/>&nbsp;"measurementType" : "Vegetationsyteareal",<br/>&nbsp;"measurementValue" : "100",<br/>&nbsp;"measurementUnit" : "m^2"<br/>\}] 	| Measurement or facts associated with the event. 	| https://tools.gbif.org/dwca-validator/extension.do?id=http://rs.iobis.org/obis/terms/ExtendedMeasurementOrFact 	|
| event.fieldNotes 	| string 	|  	| The text of notes taken in the field about the Event.    	| https://dwc.tdwg.org/terms/#dwc:fieldNotes 	|
| event.fieldNumber 	| string 	|  	| An identifier given to the event in the field. Often serves as a link   between field notes and the Event.    	| https://dwc.tdwg.org/terms/#dwc:fieldNumber 	|
| event.media 	|Collection[\<Multimedia\>](https://tools.gbif.org/dwca-validator/extension.do?id=gbif:Multimedia) 	|  	| Media associated with the event. 	| https://tools.gbif.org/dwca-validator/extension.do?id=gbif:Multimedia 	|
| event.parentEventId 	| string 	|  	| An identifier for the broader Event that groups this and potentially   other Events.    	| https://dwc.tdwg.org/terms/#dwc:parentEventID 	|  
| &nbsp;  	|  	|  	|  	|  	|
| **Occurrence** 	| 	|  	|  	|  	|  
| occurrence.occurrenceId 	| string 	| "urn:lsid:artportalen.se:Sighting:3921008" 	| A globally unique identifier for the Occurrence.    	| https://dwc.tdwg.org/terms/#dwc:occurrenceID 	|
| occurrence.catalogNumber 	| string 	| "3921008" 	| An identifier for the record within the data set or collection.    	| https://dwc.tdwg.org/terms/#dwc:occurrence.catalogNumber 	|
| occurrence.catalogId 	| int32    	| 3921008 	| An int32 identifier for the record within the data set or collection.    	|  	|
| occurrence.occurrenceRemarks 	| string 	|  "Floraundersökning   kalkeffekter åt länsstyrelsen ej kalkat" 	| Comments or notes about the Occurrence.    	| https://dwc.tdwg.org/terms/#dwc:occurrenceRemarks 	|
| occurrence.recordedBy 	| string 	| "Ove Sinatra" 	| A list of names of people, groups, or organizations responsible for   recording the original Occurrence.    	| https://dwc.tdwg.org/terms/#dwc:recordedBy 	|
| occurrence.reportedBy 	| string 	| "Dennis Alnér" 	| Name of the person that reported the species observation.    	|  	|
| occurrence.reportedDate 	| string 	| "2011-10-28T06:51:00Z" 	| Date and time when the species observation was reported (UTC).    	|  	|
| occurrence.occurrenceStatus 	| VocabularyValue[\<occurrenceStatus\>](Vocabularies.md#occurrenceStatus)	| \{ "id":0, "value":"present" \}	| A statement about the presence or absence of a Taxon at a Location. 	| https://dwc.tdwg.org/terms/#dwc:occurrenceStatus 	|
| occurrence.activity 	| VocabularyValue[\<activity\>](Vocabularies.md#activity)	| \{ "id":25, "value":"födosökande" \} | A description of the activity shown by the subject at the time the   Occurrence was recorded. 	|  	|
| occurrence.behavior 	| VocabularyValue[\<behavior\>](Vocabularies.md#behavior)	| \{ "id":17, "value":"permanent revir" \} | A description of the behavior shown by the subject at the time the   Occurrence was recorded. 	| https://dwc.tdwg.org/terms/#dwc:behavior 	|
| occurrence.biotope 	| VocabularyValue[\<biotope\>](Vocabularies.md#biotope)	| \{ "id":509, "value":"Vattenstrand" \} | Biotope. 	|  	|
| occurrence.lifeStage 	| VocabularyValue[\<lifeStage\>](Vocabularies.md#lifeStage)	| \{ "id":24, "value":"larv/nymf" \}	| The age class or life stage of the biological individual(s) at the time   the Occurrence was recorded. Recommended best practice is to use a controlled   vocabulary. 	| https://dwc.tdwg.org/terms/#dwc:occurrence.lifeStage 	|
| occurrence.reproductiveCondition 	| VocabularyValue[\<reproductiveCondition\>](Vocabularies.md#reproductiveCondition)	| \{ "id":13, "value":"ruvfläckar" \}	| The reproductive condition of the biological individual(s) represented in   the Occurrence. 	| https://dwc.tdwg.org/terms/#dwc:reproductiveCondition 	|
| occurrence.sex 	| VocabularyValue[\<sex\>](Vocabularies.md#sex)	| \{ "id":2, "value":"hona" \}	| The sex of the biological individual(s) represented in the Occurrence. 	| https://dwc.tdwg.org/terms/#dwc:sex 	|
| occurrence.biotopeDescription 	| string 	| "havsstrandäng med låga kullar och fuktdrag" 	| Description of biotope.    	|  	|
| occurrence.associatedMedia 	| string 	| http://www.artportalen.se/Image/744511 	| A list of identifiers of media associated with the Occurrence.    	| https://dwc.tdwg.org/terms/#dwc:associatedMedia 	|
| occurrence.associatedReferences 	| string 	| "urn:lsid:artportalen.se:Sighting:PlantAndMushroom.4518773" 	| A list of identifiers of literature associated with the Occurrence.    	| https://dwc.tdwg.org/terms/#dwc:associatedReferences 	|
| occurrence.birdNestActivityId 	| int32    	| 4 	| Bird nest activity id. 0 if not a   bird. Bird occurrences without an activity gets the value 1000000. Use   birdNestActivityLimit filter when searching.    	|  	|
| occurrence.individualCount 	| string 	| "1" 	| The number of individuals represented present at the time of the   Occurrence.    	| https://dwc.tdwg.org/terms/#dwc:occurrence.individualCount 	|
| occurrence.organismQuantity 	| string 	| "1" 	| A number or enumeration value for the quantity of organisms.    	| https://dwc.tdwg.org/terms/#dwc:organismQuantity 	|
| occurrence.organismQuantityInt 	| int32 	| 1 	| The quantity of organisms as integer.    	|  	|
| occurrence.organismQuantityUnit 	| VocabularyValue[\<unit\>](Vocabularies.md#unit) 	| \{ "id":1, "value":"plantor/tuvor" \}	| The type of quantification system used for the quantity of organisms. 	| https://dwc.tdwg.org/terms/#dwc:organismQuantityUnit 	|
| occurrence.isNaturalOccurrence 	| boolean 	| true 	| Indicates if this species occurrence is natural or if it is a result of   human activity.    	|  	|
| occurrence.isNeverFoundObservation 	| boolean 	| false 	| Indicates if this observation is a   never found observation. "Never found observation" is an   observation that says that the specified species was not found in a location   deemed appropriate for the species.    	|  	|
| occurrence.isNotRediscoveredObservation 	| boolean 	| false 	| Indicates if this observation is a   not rediscovered observation. "Not rediscovered observation" is an   observation that says that the specified species was not found in a location   where it has previously been observed.    	|  	|
| occurrence.isPositiveObservation 	| boolean 	| true 	| Indicates if this observation is a   positive observation. "Positive observation" is a normal   observation indicating that a species has been seen at a specified location.    	|  	|
| occurrence.media 	| Collection[\<Multimedia\>](https://tools.gbif.org/dwca-validator/extension.do?id=gbif:Multimedia)	| [\{<br/>&nbsp;"type":"StillImage",<br/>&nbsp;"format":"image/jpeg",<br/>&nbsp;"identifier":"https://www.artportalen.se/MediaLibrary/2020/11/a268a815-67cc-4a97-8636-c18c6826f397_image.jpg",<br/>&nbsp;"references":"https://www.artportalen.se/Image/3139311",<br/>&nbsp;"created":"2020-11-03 16:15",<br/>&nbsp;"license":"© all rights reserved",<br/>&nbsp;"rightsHolder":"Tom Volgers"<br/>\}]	| Media associated with the observation. 	| https://tools.gbif.org/dwca-validator/extension.do?id=gbif:Multimedia 	|
| occurrence.preparations 	| string 	| "alcohol" 	| A list of preparations and preservation methods for a specimen.    	| https://dwc.tdwg.org/terms/#dwc:preparations 	|
| occurrence.sensitivityCategory 	| int32    	| 1 	| Occurrence protection level. This is   a value between 1 to 5. 1 indicates public access and 5 is the highest used   security level.    	|  	|
| occurrence.recordNumber 	| string 	| "200240" 	| An identifier given to the Occurrence at the time it was recorded. Often   serves as a link between field notes and an Occurrence record, such as a   specimen collector's number.    	| https://dwc.tdwg.org/terms/#dwc:recordNumber 	|
| occurrence.substrate.description 	| string 	| "2 substratenheter # Dead tree stem # Grov granlåga # Picea   abies" 	| Description of substrate. 	|  	|
| occurrence.substrate.id 	| int32? 	| 84 	| Substrate id. 	|  	|
| occurrence.substrate.name 	| VocabularyValue[\<substrate\>](Vocabularies.md#substrate)	| \{ "id":84, "value":"Död trädstam" \}	| Substrate. 	|  	|
| occurrence.substrate.quantity 	| int32? 	| 2 	| Quantity of substrate. 	|  	|
| occurrence.substrate.speciesDescription 	| string 	|  	| Description of substrate species. 	|  	|
| occurrence.substrate.speciesId 	| int32? 	| 220850 	| Substrate taxon id. 	|  	|
| occurrence.substrate.speciesScientificName 	| string 	| "Picea abies" 	| Scientific name of substrate species. 	|  	|
| occurrence.substrate.speciesVernacularName 	| string 	| "gran" 	| Vernacular name of substrate species. 	|  	|
| occurrence.url 	| string 	| "http://www.artportalen.se/sighting/474560" 	| URL to occurrence.    	|  	|
| occurrence.length 	| int32 	| 487 	| The reported length in mm.    	|  	|
| occurrence.weight 	| int32 	| 900 	| The reported weight in gram.    	|  	|
| occurrence.associatedOccurrences 	| string 	|  	| A list of identifiers of other Occurrence records and their associations   to this Occurrence.    	| https://dwc.tdwg.org/terms/#dwc:associatedOccurrences 	|
| occurrence.associatedSequences 	| string 	|  	| A list of identifiers of genetic sequence information associated with the   Occurrence. 	| https://dwc.tdwg.org/terms/#dwc:associatedSequences 	|
| occurrence.associatedTaxa 	| string 	|  	| A list of identifiers or names of taxa and their associations with the   Occurrence.    	| https://dwc.tdwg.org/terms/#dwc:associatedTaxa 	|
| occurrence.disposition 	| string 	|  	| The current state of a specimen with respect to the collection identified   in collectionCode or collectionID.    	| https://dwc.tdwg.org/terms/#dwc:occurrence.disposition 	|
| occurrence.establishmentMeans 	| VocabularyValue[\<establishmentMeans\>](Vocabularies.md#establishmentMeans)	|  	| Statement   about whether an organism or organisms have been introduced to a given place   and time through the direct or indirect activity of modern humans. 	| https://dwc.tdwg.org/terms/#dwc:occurrence.establishmentMeans 	|
| occurrence.individualID 	| string 	|  	| An identifier for an individual or   named group of individual organisms represented in the Occurrence.    	|  	|
| occurrence.otherCatalogNumbers 	| string 	|  	| A list of previous or alternate fully qualified catalog numbers or other   human-used identifiers for the same Occurrence.    	| https://dwc.tdwg.org/terms/#dwc:otherCatalogNumbers 	|
| &nbsp;  	|  	|  	|  	|  	|
| **Identification** 	| 	|  	|  	|  	|  
| identification.confirmedBy 	| string 	| "Gerhard Boré" 	| Confirmed by.    	|  	|
| identification.confirmedDate 	| string 	| "2017" 	| Date of confirmation.    	|  	|
| identification.identifiedBy 	| string 	| "Mårten Ilidasch" 	| A list of names of people, groups, or organizations who assigned the   Taxon to the subject.    	| https://dwc.tdwg.org/terms/#dwc:identifiedBy 	|
| identification.dateIdentified 	| string 	| "2016" 	| The date on which the subject was identified as representing the Taxon.    	| https://dwc.tdwg.org/terms/#dwc:dateIdentified 	|
| identification.verifiedBy 	| string 	| "Lennart Lasseman" 	| A list of names of people, who verified the observation.    	|  	|
| identification.verified 	| boolean    	| true 	| Indicates whether the occurrence is verified.    	|  	|
| identification.verificationStatus 	| VocabularyValue[\<validationStatus\>](Vocabularies.md#validationStatus) 	| \{ "id":60, "value":"Godkänd baserat på observatörens uppgifter" \}	| Verification status. 	|  	|
| identification.determinationMethod 	| VocabularyValue[\<determinationMethod\>](Vocabularies.md#determinationMethod) 	| \{ "id":3, "value":"stereolupp" \} | Method used in species determination. 	|  	|
| identification.uncertainIdentification 	| boolean    	| false 	| True if determination is uncertain.    	|  	|
| identification.identificationQualifier 	| string 	| "Andersson, 1976" 	| A brief phrase or a standard term ("cf.", "aff.") to   express the determiner's doubts about the Identification.    	| https://dwc.tdwg.org/terms/#dwc:identificationQualifier 	|
| identification.typeStatus 	| string 	| "paratype" 	| A list of nomenclatural types applied to the subject.    	| https://dwc.tdwg.org/terms/#dwc:typeStatus 	|
| identification.identificationId 	| string 	|  	| An identifier for the Identification.    	| https://dwc.tdwg.org/terms/#dwc:identificationID 	|
| identification.identificationReferences 	| string 	|  	| A list of references used in the Identification.    	| https://dwc.tdwg.org/terms/#dwc:identificationReferences 	|
| identification.identificationRemarks 	| string 	|  	| Comments or notes about the Identification.    	| https://dwc.tdwg.org/terms/#dwc:identificationRemarks 	|  
| &nbsp;  	|  	|  	|  	|  	|
| **Location** 	| 	|  	|  	|  	|  
| location.decimalLatitude 	| double? 	| 57.4303 	| The geographic latitude of the geographic center of a Location (WGS84).    	| https://dwc.tdwg.org/terms/#dwc:decimalLatitude 	|
| location.decimalLongitude 	| double? 	| 16.66547 	| The geographic longitude of the geographic center of a Location (WGS84).    	| https://dwc.tdwg.org/terms/#dwc:decimalLongitude 	|
| location.geodeticDatum 	| string 	| "EPSG:4326" 	| The ellipsoid, geodetic datum, or spatial reference system (SRS) upon   which the geographic coordinates given in decimalLatitude and decimalLongitude as based.    	| https://dwc.tdwg.org/terms/#dwc:geodeticDatum 	|
| location.coordinateUncertaintyInMeters 	| int32? 	| 100 	| The horizontal distance (in meters) from the given CoordinateX and   CoordinateY describing the smallest circle containing the whole of the Location.    	| https://dwc.tdwg.org/terms/#dwc:coordinateUncertaintyInMeters 	|
| location.Sweref99TmX 	| double? 	| 661035 	| X coordinate in SWEREF99 TM. |  	|
| location.Sweref99TmY 	| double? 	| 6569227 	| Y coordinate in SWEREF99 TM. |  	|
| location.locality 	| string 	| "Mosse 200 m SO om TÅNGEN, Vg" 	| The specific description of the place.    	| https://dwc.tdwg.org/terms/#dwc:locality 	|
| location.continent 	| VocabularyValue[\<continent\>](Vocabularies.md#continent)	| \{ "id":4, "value":"Europe" \} | The name of the continent in which the Location occurs. 	| https://dwc.tdwg.org/terms/#dwc:continent 	|
| location.country 	| VocabularyValue[\<country\>](Vocabularies.md#country) | \{ "id":0, "value":"Sweden" \} | The name of the country in which the Location occurs. 	| https://dwc.tdwg.org/terms/#dwc:country 	|
| location.countryCode 	| string 	| "SE" 	| The standard code for the country in which the Location occurs.    	| https://dwc.tdwg.org/terms/#dwc:countryCode 	|
| location.county 	| Area[\<county\>](Areas.md#County-Län)	| \{ "featureId":"8", "name":"Kalmar" \}	| The county ('län' in swedish) in which the Location occurs. 	| https://dwc.tdwg.org/terms/#dwc:county 	|
| location.municipality 	| Area[\<municipality\>](Areas.md#Municipality-Kommun) 	| \{ "featureId":"882", "name":"Oskarshamn" \}	| The municipality ('kommun' in swedish) in which the Location occurs. 	| https://dwc.tdwg.org/terms/#dwc:municipality 	|
| location.parish 	| Area[\<parish\>](Areas.md#Parish-Socken) 	| \{ "featureId":"845", "name":"Misterhult" \}	| The parish ('socken' in swedish) in which the Location occurs. 	|  	|
| location.province 	| Area[\<province\>](Areas.md#Province-Landskap) 	| \{ "featureId":"3", "name":"Småland" \} | The province ('landskap' in swedish) in which the Location occurs. 	| https://dwc.tdwg.org/terms/#dwc:stateProvince 	|
| location.georeferencedBy 	| string 	| "Göte Svensson" 	| A list of names of people, groups, or organizations who determined the   georeference the Location.    	| https://dwc.tdwg.org/terms/#dwc:georeferencedBy 	|
| location.georeferencedDate 	| string 	| 2017-06-29  00:00:00 	| The date on which the Location was georeferenced.    	| https://dwc.tdwg.org/terms/#dwc:georeferencedDate 	|
| location.georeferenceRemarks 	| string 	| "The location is obfuscated (200-800m)" 	| Notes or comments about the spatial description determination.    	| https://dwc.tdwg.org/terms/#dwc:georeferenceRemarks 	|
| location.higherGeography 	| string 	| "Lule Lappmark, Sweden" 	| A list of geographic names less specific than the information captured in   the locality term.    	| https://dwc.tdwg.org/terms/#dwc:higherGeography 	|
| location.island 	| string 	| "Holmön" 	| The name of the island on or near which the Location occurs.    	| https://dwc.tdwg.org/terms/#dwc:island 	|
| location.locationId 	| string 	| "urn:lsid:artportalen.se:site:649345" 	| An identifier for the set of location information.    	| https://dwc.tdwg.org/terms/#dwc:locationId 	|
| location.locationRemarks 	| string 	| "Old-growth blueberry spruce forest." 	| Comments or notes about the Location.    	| https://dwc.tdwg.org/terms/#dwc:locationRemarks 	|
| location.maximumDepthInMeters 	| double? 	| 2.7 	| The greater depth of a range of depth below the local surface, in meters.    	| https://dwc.tdwg.org/terms/#dwc:maximumDepthInMeters 	|
| location.maximumElevationInMeters 	| double? 	| 5 	| The upper limit of the range of elevation, in meters.    	| https://dwc.tdwg.org/terms/#dwc:maximumElevationInMeters 	|
| location.minimumDepthInMeters 	| double? 	| 2.7 	| The lesser depth of a range of depth below the local surface, in meters.    	| https://dwc.tdwg.org/terms/#dwc:minimumDepthInMeters 	|
| location.minimumElevationInMeters 	| double? 	| 5 	| The lower limit of the range of elevation, in meters.    	| https://dwc.tdwg.org/terms/#dwc:minimumElevationInMeters 	|
| location.verbatimCoordinateSystem 	| string 	| "EPSG:4326" 	| The spatial coordinate system for the verbatimLatitude and   verbatimLongitude or the verbatimCoordinates of the Location.    	| https://dwc.tdwg.org/terms/#dwc:verbatimCoordinateSystem 	|
| location.verbatimLatitude 	| string 	| "57.4303" 	| The verbatim original latitude of the Location.    	| https://dwc.tdwg.org/terms/#dwc:verbatimLatitude 	|
| location.verbatimLongitude 	| string 	| "16.66547" 	| The verbatim original longitude of the Location.    	| https://dwc.tdwg.org/terms/#dwc:verbatimLongitude 	|
| location.verbatimSRS 	| string 	| "EPSG:4326" 	| The ellipsoid, geodetic datum, or spatial reference system (SRS) upon   which coordinates given in verbatimLatitude and verbatimLongitude, or   verbatimCoordinates are based.    	| https://dwc.tdwg.org/terms/#dwc:verbatimSRS 	|
| location.waterBody 	| string 	| "Alsterån" 	| The name of the water body in which the Location occurs.    	| https://dwc.tdwg.org/terms/#dwc:waterBody 	|
| location.attributes.countyPartIdByCoordinate 	| string 	| 14 	| County part id.    	|  	|
| location.attributes.provincePartIdByCoordinate 	| string 	| 9 	| Province part id.    	|  	|
| location.attributes.verbatimProvince 	| string 	| "Småland" 	| The original StateProvince value from data provider.    	|  	|
| location.attributes.verbatimMunicipality 	| string 	|  	| The original municipality value from data provider.    	|  	|
| location.attributes.externalId 	| string 	|  	| External Id of an Artportalen site.    	|  	|
| location.attributes.projectId 	| string 	|  	| Artportalen project id.    	|  	|
| location.maximumDistanceAboveSurfaceInMeters 	| double? 	|  	| The greater distance in a range of distance from a reference surface in   the vertical direction, in meters.    	| https://dwc.tdwg.org/terms/#dwc:maximumDistanceAboveSurfaceInMeters 	|
| location.coordinatePrecision 	| double? 	|  	| A decimal representation of the precision of the coordinates given in the   DecimalLatitude and DecimalLongitude.    	| https://dwc.tdwg.org/terms/#dwc:coordinatePrecision 	|
| location.footprintSpatialFit 	| string 	|  	| The ratio of the area of the footprint (footprintWKT) to the area of the   true (original, or most specific) spatial representation of the Location.    	| https://dwc.tdwg.org/terms/#dwc:footprintSpatialFit 	|
| location.footprintSRS 	| string 	|  	| A Well-Known Text (WKT) representation of the Spatial Reference System   (SRS) for the footprintWKT of the Location.    	| https://dwc.tdwg.org/terms/#dwc:footprintSRS 	|
| location.footprintWKT 	| string 	|  	| A Well-Known Text (WKT) representation of the shape (footprint, geometry)   that defines the Location.    	| https://dwc.tdwg.org/terms/#dwc:footprintWKT 	|
| location.georeferenceProtocol 	| string 	|  	| A description or reference to the methods used to determine the spatial   footprint, coordinates, and uncertainties.    	| https://dwc.tdwg.org/terms/#dwc:georeferenceProtocol 	|
| location.georeferenceSources 	| string 	|  	| A list of maps, gazetteers, or other resources used to georeference the   Location.    	| https://dwc.tdwg.org/terms/#dwc:georeferenceSources 	|
| location.georeferenceVerificationStatus 	| string 	|  	| A categorical description of the extent to which the georeference has   been verified to represent the best possible spatial description.    	| https://dwc.tdwg.org/terms/#dwc:georeferenceVerificationStatus 	|
| location.higherGeographyID 	| string 	|  	| An identifier for the geographic region within which the Location   occurred.    	| https://dwc.tdwg.org/terms/#dwc:higherGeographyID 	|
| location.islandGroup 	| string 	|  	| The name of the island group in which the Location occurs.    	| https://dwc.tdwg.org/terms/#dwc:islandGroup 	|
| location.locationAccordingTo 	| string 	|  	| Information about the source of this Location information.    	| https://dwc.tdwg.org/terms/#dwc:locationAccordingTo 	|
| location.minimumDistanceAboveSurfaceInMeters 	| double? 	|  	| The lesser distance in a range of distance from a reference surface in   the vertical direction, in meters.    	| https://dwc.tdwg.org/terms/#dwc:minimumDistanceAboveSurfaceInMeters 	|
| location.pointRadiusSpatialFit 	| string 	|  	| The ratio of the area of the point-radius to the area of the true spatial   representation of the Location.    	| https://dwc.tdwg.org/terms/#dwc:pointRadiusSpatialFit 	|
| location.verbatimCoordinates 	| string 	|  	| The verbatim original spatial coordinates of the Location.    	| https://dwc.tdwg.org/terms/#dwc:verbatimCoordinates 	|
| location.verbatimDepth 	| string 	|  	| The original description of the depth below the local surface.    	| https://dwc.tdwg.org/terms/#dwc:verbatimDepth 	|
| location.verbatimElevation 	| string 	|  	| The original description of the elevation of the Location.    	| https://dwc.tdwg.org/terms/#dwc:verbatimElevation 	|
| location.verbatimLocality 	| string 	|  	| The original textual description of the place.    	| https://dwc.tdwg.org/terms/#dwc:verbatimLocality 	|    
| &nbsp;  	|  	|  	|  	|  	|
| **Taxon** 	| 	|  	|  	|  	| 
| taxon.id 	| int32    	| 221501 	| Dyntaxa taxon id.    	|  	|
| taxon.taxonId 	| string 	| "urn:lsid:dyntaxa.se:Taxon:221501" 	| An identifier for the set of taxon information.     	| https://dwc.tdwg.org/terms/#dwc:taxonId 	|
| taxon.scientificName 	| string 	| "Anacamptis morio" 	| The full scientific name, with authorship and date information if known.    	| https://dwc.tdwg.org/terms/#dwc:scientificName 	|
| taxon.scientificNameAuthorship 	| string 	| "(L.) R. M. Bateman et al." 	| The authorship information for the scientificName formatted according to   the conventions of the applicable nomenclaturalCode.    	| https://dwc.tdwg.org/terms/#dwc:scientificNameAuthorship 	|
| taxon.acceptedNameUsageID 	| string 	| "urn:lsid:dyntaxa.se:Taxon:221501" 	| An identifier for the name usage of the currently valid or accepted   taxon.    	| https://dwc.tdwg.org/terms/#dwc:acceptedNameUsageID 	|
| taxon.vernacularName 	| string 	| "göknycklar" 	| Vernacular name. 	| https://dwc.tdwg.org/terms/#dwc:vernacularName 	|
| taxon.taxonRank 	| string 	| "species" 	| The taxonomic rank of the most specific name in the scientificName.    	| https://dwc.tdwg.org/terms/#dwc:taxonRank 	|
| taxon.birdDirective 	| boolean 	| false 	| Indicates whether the taxon is part of bird directive.    	|  	|
| taxon.genus 	| string 	| "Anacamptis" 	| The full scientific name of the genus in which the taxon is classified.    	| https://dwc.tdwg.org/terms/#dwc:genus 	|
| taxon.family 	| string 	| "Orchidaceae" 	| The full scientific name of the family in which the taxon is classified.    	| https://dwc.tdwg.org/terms/#dwc:family 	|
| taxon.order 	| string 	| "Asparagales" 	| The full scientific name of the order in which the taxon is classified.    	| https://dwc.tdwg.org/terms/#dwc:order 	|
| taxon.class 	| string 	| "Liliopsida" 	| The full scientific name of the class in which the taxon is classified.    	| https://dwc.tdwg.org/terms/#dwc:class 	|
| taxon.phylum 	| string 	| "Tracheophyta" 	| The full scientific name of the phylum or division in which the taxon is   classified.    	| https://dwc.tdwg.org/terms/#dwc:phylum 	|
| taxon.kingdom 	| string 	| "Plantae" 	| The full scientific name of the kingdom in which the taxon is classified.    	| https://dwc.tdwg.org/terms/#dwc:kingdom 	|
| taxon.higherClassification 	| string 	| "Biota \| Plantae \| Viridiplantae \| Streptophyta \| Embryophyta \|   Tracheophyta \| Euphyllophytina \| Spermatophytae \| Angiospermae \| Liliopsida \|   Asparagales \| Orchidaceae \| Anacamptis" 	| A list of taxa names terminating at the rank immediately superior to the   taxon referenced in the taxon record.    	| https://dwc.tdwg.org/terms/#dwc:higherClassification 	|
| taxon.nomenclaturalStatus 	| string 	| "valid" 	| The status related to the original publication of the name and its   conformance to the relevant rules of nomenclature.    	| https://dwc.tdwg.org/terms/#dwc:nomenclaturalStatus 	|
| taxon.originalNameUsageId 	| string 	| "urn:lsid:dyntaxa.se:Taxon:1005661" 	| An identifier for the name usage in which the terminal element of the   scientificName was originally established under the rules of the associated   nomenclaturalCode.    	| https://dwc.tdwg.org/terms/#dwc:originalNameUsageID 	|
| taxon.parentNameUsageId 	| string 	| "urn:lsid:dyntaxa.se:Taxon:1005870" 	| An identifier for the name usage of the direct, most proximate   higher-rank parent taxon of the most specific element of the scientificName.    	| https://dwc.tdwg.org/terms/#dwc:parentNameUsageID 	|
| taxon.secondaryParentDyntaxaTaxonIds 	| Collection\<int32\> 	| [6003870] 	| Secondary parents dyntaxa taxon ids. 	|  	|
| taxon.taxonomicStatus 	| string 	| "accepted" 	| The status of the use of the scientificName as a label for a taxon.    	| https://dwc.tdwg.org/terms/#dwc:taxonomicStatus 	|
| taxon.taxonRemarks 	| string 	| "auktorn bör vara M. Schultze för att skilja honom från andra   Schultze" 	| Comments or notes about the taxon or name.    	| https://dwc.tdwg.org/terms/#dwc:taxonRemarks 	|
| taxon.attributes.actionPlan 	| string 	| "Fastställt" 	| Indicates whether the species is the subject of an action plan   ('åtgärdsprogram' in swedish).    	|  	|
| taxon.attributes.disturbanceRadius 	| int32? 	| 500 	| Taxon disturbance radius.    	|  	|
| taxon.attributes.dyntaxaTaxonId 	| int32    	| 221501 	| Taxon id value in Dyntaxa.    	|  	|
| taxon.attributes.parentDyntaxaTaxonId 	| int32? 	| 1005661 	| Parent Dyntaxa TaxonId.    	|  	|
| taxon.attributes.natura2000HabitatsDirectiveArticle2 	| boolean 	| false 	| Indicates whether the taxon is part of Habitats Directive Annex 2. https://eunis.eea.europa.eu/references/2325/species   	|  	|
| taxon.attributes.natura2000HabitatsDirectiveArticle4 	| boolean 	| false 	| Indicates whether the taxon is part of Habitats Directive Annex 4. https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/564569/annex-IVa-animals.pdf   	|  	|
| taxon.attributes.natura2000HabitatsDirectiveArticle5 	| boolean 	| false 	| Indicates whether the taxon is part of Habitats Directive Annex 5. https://registry.nbnatlas.org/public/show/dr2407   	|  	|
| taxon.attributes.organismGroup 	| string 	| "Kärlväxter" 	| Common name of the organism group   that observed species belongs to. Classification of species groups is the   same as used in latest 'Red List of Swedish Species'.    	|  	|
| taxon.attributes.protectedByLaw 	| boolean 	| true 	| Indicates whether the species is protected by the law in Sweden. In Swedish "Fridlyst enligt Artskyddsförodningen (SFS 2007:845)".   	|  	|
| taxon.attributes.sensitivityCategory 	| VocabularyValue[\<protectionLevel\>](Vocabularies.md#protectionLevel)	| \{ "id":1, "value":"Fullständig åtkomst och fri användning för alla" \}	| Information about how protected (sensitivity level) a species is in Sweden. This is a value between 1 to 5. 1 indicates public access and 5 is the highest used sensitivity level. More information (in Swedish): https://www.artdatabanken.se/var-verksamhet/fynddata/skyddsklassade-arter/	|  	|
| taxon.attributes.redlistCategory 	| string 	| "LC" 	| Redlist category for redlisted   species. Possible redlist values are DD (Data Deficient), EX (Extinct), RE   (Regionally Extinct), CR (Critically Endangered), EN (Endangered), VU   (Vulnerable), NT (Near Threatened). Not redlisted species have no value in   this property.    	|  	|
| taxon.attributes.isRedlisted 	| boolean 	| true | True if redlist category is one of CR, EN, VU, NT. 	|  	|
| taxon.attributes.isInvasiveInSweden 	| boolean 	| true | True if invasive in sweden. 	|  	|
| taxon.attributes.isInvasiveAccordingToEuRegulation 	| boolean 	| true | True if invasive in Sweden according to EU Regulation 1143/2014. 	|  	|
| taxon.attributes.invasiveRiskAssessmentCategory 	| string 	| SE | Invasive risk assessment category. More information (in Swedish): https://www.artdatabanken.se/arter-och-natur/biologisk-mangfald/frammande-arter/slu-artdatabankens-arbete-med-frammande-arter/	|  	|
| taxon.attributes.sortOrder 	| int32    	| 91415 	| Systematic sort order.    	|  	|
| taxon.attributes.swedishHistory 	| string 	| "Spontan" 	| This property contains information about the species immigration history.    	|  	|
| taxon.attributes.swedishOccurrence 	| string 	| "Bofast och reproducerande" 	| Information about the species   occurrence in Sweden. For example information about if the species reproduce   in sweden.    	|  	|
| taxon.attributes.synonyms 	| Collection\<TaxonSynonymName\> 	| [\{<br/>&nbsp;"name":"Orchis morio",<br/>&nbsp;"author":"L.",<br/>&nbsp;"taxonomicStatus":"synonym",<br/>&nbsp;"nomenclaturalStatus":"valid"<br/>\}] 	| Scientific synonym names. 	|  	|
| taxon.attributes.taxonCategory 	| VocabularyValue[\<taxonCategory\>](Vocabularies.md#taxonCategory)	| \{ "id":17, "value":"Art" \}	| Taxon category. 	|  	|
| taxon.attributes.vernacularNames 	| Collection\<TaxonVernacularName\> 	| [\{<br/>&nbsp;"name":"göknycklar",<br/>&nbsp;"language":"sv",<br/>&nbsp;"countryCode":"SE",<br/>&nbsp;"isPreferredName":"true"<br/>\}] | Vernacular names. 	|  	|
| taxon.acceptedNameUsage 	| string 	|  	| The full name, with authorship and date information if known, of the   currently valid or accepted taxon.    	| https://dwc.tdwg.org/terms/#dwc:acceptedNameUsage 	|
| taxon.infraspecificEpithet 	| string 	|  	| The name of the lowest or terminal infraspecific epithet of the   scientificName, excluding any rank designation.    	| https://dwc.tdwg.org/terms/#dwc:infraspecificEpithet 	|
| taxon.nameAccordingTo 	| string 	|  	| The reference to the source in which the specific taxon concept   circumscription is defined or implied.    	| https://dwc.tdwg.org/terms/#dwc:nameAccordingTo 	|
| taxon.nameAccordingToID 	| string 	|  	| An identifier for the source in which the specific taxon concept   circumscription is defined or implied.    	| https://dwc.tdwg.org/terms/#dwc:nameAccordingToID 	|
| taxon.namePublishedIn 	| string 	|  	| A reference for the publication in which the scientificName was   originally established under the rules of the associated nomenclaturalCode.    	| https://dwc.tdwg.org/terms/#dwc:namePublishedIn 	|
| taxon.namePublishedInId 	| string 	|  	| An identifier for the publication in which the scientificName was   originally established under the rules of the associated nomenclaturalCode.    	| https://dwc.tdwg.org/terms/#dwc:namePublishedInId 	|
| taxon.namePublishedInYear 	| string 	|  	| The four-digit year in which the scientificName was published.    	| https://dwc.tdwg.org/terms/#dwc:namePublishedInYear 	|
| taxon.nomenclaturalCode 	| string 	|  	| The nomenclatural code under which the scientificName is constructed.    	| https://dwc.tdwg.org/terms/#dwc:nomenclaturalCode 	|
| taxon.originalNameUsage 	| string 	|  	| The taxon name, with authorship and date information if known, as it   originally appeared when first established under the rules of the associated   nomenclaturalCode.    	| https://dwc.tdwg.org/terms/#dwc:originalNameUsage 	|
| taxon.parentNameUsage 	| string 	|  	| The full name, with authorship and date information if known, of the   direct, most proximate higher-rank parent taxon of the most specific element   of the scientificName.    	| https://dwc.tdwg.org/terms/#dwc:parentNameUsage 	|
| taxon.scientificNameId 	| string 	|  	| An identifier for the nomenclatural details of a scientific name.    	| https://dwc.tdwg.org/terms/#dwc:scientificNameID 	|
| taxon.specificEpithet 	| string 	|  	| The name of the first or species epithet of the scientificName.    	| https://dwc.tdwg.org/terms/#dwc:specificEpithet 	|
| taxon.subgenus 	| string 	|  	| The full scientific name of the subgenus in which the taxon is   classified.    	| https://dwc.tdwg.org/terms/#dwc:subgenus 	|
| taxon.taxonConceptId 	| string 	|  	| An identifier for the taxonomic concept to which the record refers.    	| https://dwc.tdwg.org/terms/#dwc:taxonConceptID 	|
| taxon.verbatimTaxonRank 	| string 	|  	| The taxonomic rank of the most specific name in the scientificName as it   appears in the original record.    	| https://dwc.tdwg.org/terms/#dwc:verbatimTaxonRank 	|    
| &nbsp;  	|  	|  	|  	|  	|
| **Organism** 	| 	|  	|  	|  	| 
| organism.organismId 	| string 	|  	| An identifier for the Organism instance.    	| https://dwc.tdwg.org/terms/#dwc:organismID 	|
| organism.organismName 	| string 	|  	| A textual name or label assigned to an Organism instance.    	| https://dwc.tdwg.org/terms/#dwc:organismName 	|
| organism.organismScope 	| string 	|  	| A description of the kind of Organism instance.    	| https://dwc.tdwg.org/terms/#dwc:organismScope 	|
| organism.associatedOccurrences 	| string 	|  	| A list of identifiers of other Occurrence records and their associations   to this Occurrence.    	| https://dwc.tdwg.org/terms/#dwc:associatedOccurrences 	|
| organism.associatedOrganisms 	| string 	|  	| A list of identifiers of other Organisms and their associations to this   Organism.    	| https://dwc.tdwg.org/terms/#dwc:associatedOrganisms 	|
| organism.previousIdentifications 	| string 	|  	| A list of previous assignments of names to the Organism.    	| https://dwc.tdwg.org/terms/#dwc:previousIdentifications 	|
| organism.organismRemarks 	| string 	|  	| Comments or notes about the Organism instance.    	| https://dwc.tdwg.org/terms/#dwc:organismRemarks 	|    
| &nbsp;  	|  	|  	|  	|  	|
| **GeologicalContext** 	| 	|  	|  	|  	|   
| geologicalContext.bed 	| string 	|  	| The full name of the lithostratigraphic bed from which the cataloged item   was collected.    	| https://dwc.tdwg.org/terms/#dwc:bed 	|
| geologicalContext.earliestAgeOrLowestStage 	| string 	|  	| The full name of the earliest possible geochronologic age or lowest   chronostratigraphic stage attributable to the stratigraphic horizon from   which the cataloged item was collected.    	| https://dwc.tdwg.org/terms/#dwc:earliestAgeOrLowestStage 	|
| geologicalContext.earliestEonOrLowestEonothem 	| string 	|  	| The full name of the earliest possible geochronologic eon or lowest   chrono-stratigraphic eonothem or the informal name ("Precambrian")   attributable to the stratigraphic horizon from which the cataloged item was   collected.    	| https://dwc.tdwg.org/terms/#dwc:earliestEonOrLowestEonothem 	|
| geologicalContext.earliestEpochOrLowestSeries 	| string 	|  	| The full name of the earliest possible geochronologic epoch or lowest   chronostratigraphic series attributable to the stratigraphic horizon from   which the cataloged item was collected.    	| https://dwc.tdwg.org/terms/#dwc:earliestEpochOrLowestSeries 	|
| geologicalContext.earliestEraOrLowestErathem 	| string 	|  	| The full name of the earliest possible geochronologic era or lowest   chronostratigraphic erathem attributable to the stratigraphic horizon from   which the cataloged item was collected.    	| https://dwc.tdwg.org/terms/#dwc:earliestEraOrLowestErathem 	|
| geologicalContext.earliestGeochronologicalEra 	| string 	|  	| Use to link a dwc:GeologicalContext instance to chronostratigraphic time   periods at the lowest possible level in a standardized hierarchy.    	| https://dwc.tdwg.org/terms/#dwc:earliestGeochronologicalEra 	|
| geologicalContext.earliestPeriodOrLowestSystem 	| string 	|  	| The full name of the earliest possible geochronologic period or lowest   chronostratigraphic system attributable to the stratigraphic horizon from   which the cataloged item was collected.    	| https://dwc.tdwg.org/terms/#dwc:earliestPeriodOrLowestSystem 	|
| geologicalContext.formation 	| string 	|  	| The full name of the lithostratigraphic formation from which the   cataloged item was collected.    	| https://dwc.tdwg.org/terms/#dwc:formation 	|
| geologicalContext.geologicalContextId 	| string 	|  	| An identifier for the set of information associated with a   GeologicalContext (the location within a geological context, such as   stratigraphy).    	| https://dwc.tdwg.org/terms/#dwc:geologicalContextId 	|
| geologicalContext.group 	| string 	|  	| The full name of the lithostratigraphic group from which the cataloged   item was collected.    	| https://dwc.tdwg.org/terms/#dwc:group 	|
| geologicalContext.highestBiostratigraphicZone 	| string 	|  	| The full name of the highest possible geological biostratigraphic zone of   the stratigraphic horizon from which the cataloged item was collected.    	| https://dwc.tdwg.org/terms/#dwc:highestBiostratigraphicZone 	|
| geologicalContext.latestAgeOrHighestStage 	| string 	|  	| The full name of the latest possible geochronologic age or highest   chronostratigraphic stage attributable to the stratigraphic horizon from   which the cataloged item was collected.    	| https://dwc.tdwg.org/terms/#dwc:latestAgeOrHighestStage 	|
| geologicalContext.latestEonOrHighestEonothem 	| string 	|  	| The full name of the latest possible geochronologic eon or highest   chrono-stratigraphic eonothem or the informal name ("Precambrian")   attributable to the stratigraphic horizon from which the cataloged item was   collected.    	| https://dwc.tdwg.org/terms/#dwc:latestEonOrHighestEonothem 	|
| geologicalContext.latestEpochOrHighestSeries 	| string 	|  	| The full name of the latest possible geochronologic epoch or highest   chronostratigraphic series attributable to the stratigraphic horizon from   which the cataloged item was collected.    	| https://dwc.tdwg.org/terms/#dwc:latestEpochOrHighestSeries 	|
| geologicalContext.latestEraOrHighestErathem 	| string 	|  	| The full name of the latest possible geochronologic era or highest   chronostratigraphic erathem attributable to the stratigraphic horizon from   which the cataloged item was collected.    	| https://dwc.tdwg.org/terms/#dwc:latestEraOrHighestErathem 	|
| geologicalContext.latestGeochronologicalEra 	| string 	|  	| Use to link a dwc:GeologicalContext instance to chronostratigraphic time   periods at the lowest possible level in a standardized hierarchy.    	| https://dwc.tdwg.org/terms/#dwc:latestGeochronologicalEra 	|
| geologicalContext.latestPeriodOrHighestSystem 	| string 	|  	| The full name of the latest possible geochronologic period or highest   chronostratigraphic system attributable to the stratigraphic horizon from   which the cataloged item was collected.    	| https://dwc.tdwg.org/terms/#dwc:latestPeriodOrHighestSystem 	|
| geologicalContext.lithostratigraphicTerms 	| string 	|  	| The combination of all litho-stratigraphic names for the rock from which   the cataloged item was collected.    	| https://dwc.tdwg.org/terms/#dwc:lithostratigraphicTerms 	|
| geologicalContext.lowestBiostratigraphicZone 	| string 	|  	| The full name of the lowest possible geological biostratigraphic zone of   the stratigraphic horizon from which the cataloged item was collected.    	| https://dwc.tdwg.org/terms/#dwc:lowestBiostratigraphicZone 	|
| geologicalContext.member 	| string 	|  	| The full name of the lithostratigraphic member from which the cataloged   item was collected.    	| https://dwc.tdwg.org/terms/#dwc:member 	|  
| &nbsp;  	|  	|  	|  	|  	|
| **MaterialSample** 	| 	|  	|  	|  	|   
| materialSample.materialSampleId |	string |  |	An identifier for the MaterialSample (as opposed to a particular digital record of the material sample). |	https://dwc.tdwg.org/terms/#dwc:materialSampleID  



