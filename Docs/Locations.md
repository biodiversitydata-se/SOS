# Locations
You can get site details by using the `Locations_GetLocationsByIds` endpoint. The `pointWithBuffer` property will get you the polygon for the site.

**Example payload**
```json
[
  "urn:lsid:artportalen.se:site:2798180"
]
```

## In progress
- A property, `LocationType`, will be added that states whether the geometry is a point or a polygon.

## Data model
| Field 	| Type 	| Example 	| Description | Darwin Core 	|
|:---	|:---	|:---	|:---	|:---	|  
| decimalLatitude 	| double? 	| 57.4303 	| The geographic latitude of the geographic center of a Location (WGS84).    	| https://dwc.tdwg.org/terms/#dwc:decimalLatitude 	|
| decimalLongitude 	| double? 	| 16.66547 	| The geographic longitude of the geographic center of a Location (WGS84).    	| https://dwc.tdwg.org/terms/#dwc:decimalLongitude 	|
| coordinateUncertaintyInMeters 	| int32? 	| 100 	| The horizontal distance (in meters) from the given CoordinateX and   CoordinateY describing the smallest circle containing the whole of the Location.    	| https://dwc.tdwg.org/terms/#dwc:coordinateUncertaintyInMeters 	|
| locality 	| string 	| "Mosse 200 m SO om TÅNGEN, Vg" 	| The specific description of the place.    	| https://dwc.tdwg.org/terms/#dwc:locality 	|
| continent 	| VocabularyValue[\<continent\>](Vocabularies.md#continent)	| \{ "id":4, "value":"Europe" \} | The name of the continent in which the Location occurs. 	| https://dwc.tdwg.org/terms/#dwc:continent 	|
| country 	| VocabularyValue[\<country\>](Vocabularies.md#country) | \{ "id":0, "value":"Sweden" \} | The name of the country in which the Location occurs. 	| https://dwc.tdwg.org/terms/#dwc:country 	|
| countryCode 	| string 	| "SE" 	| The standard code for the country in which the Location occurs.    	| https://dwc.tdwg.org/terms/#dwc:countryCode 	|
| county 	| VocabularyValue[\<county\>](Areas.md#County-Län)	| \{ "id":"8", "value":"Kalmar" \}	| The county ('län' in swedish) in which the Location occurs. 	| https://dwc.tdwg.org/terms/#dwc:county 	|
| municipality 	| VocabularyValue[\<municipality\>](Areas.md#Municipality-Kommun) 	| \{ "id":"882", "value":"Oskarshamn" \}	| The municipality ('kommun' in swedish) in which the Location occurs. 	| https://dwc.tdwg.org/terms/#dwc:municipality 	|
| parish 	| VocabularyValue[\<parish\>](Areas.md#Parish-Socken) 	| \{ "id":"845", "value":"Misterhult" \}	| The parish ('socken' in swedish) in which the Location occurs. 	|  	|
| province 	| VocabularyValue[\<province\>](Areas.md#Province-Landskap) 	| \{ "id":"3", "value":"Småland" \} | The province ('landskap' in swedish) in which the Location occurs. 	| https://dwc.tdwg.org/terms/#dwc:stateProvince 	|
| countryRegion 	| VocabularyValue\<countryRegion\> 	| \{ "id":"4", "value":"Götaland" \}	| The parish ('socken' in swedish) in which the Location occurs. 	|  	|
| coordinatePrecision 	| double? 	|  	| A decimal representation of the precision of the coordinates given in the   DecimalLatitude and DecimalLongitude.    	| https://dwc.tdwg.org/terms/#dwc:coordinatePrecision 	|
| attributes.externalId 	| string 	|  	| External Id of an Artportalen site.    	|  	|
| locationAccordingTo 	| string 	|  	| Information about the source of this Location information.    	| https://dwc.tdwg.org/terms/#dwc:locationAccordingTo 	|
| locationId 	| string 	| "urn:lsid:artportalen.se:site:649345" 	| An identifier for the set of location information.    	| https://dwc.tdwg.org/terms/#dwc:locationId 	|
| locationRemarks 	| string 	| "Old-growth blueberry spruce forest." 	| Comments or notes about the Location.    	| https://dwc.tdwg.org/terms/#dwc:locationRemarks 	|
| attributes.projectId 	| string 	|  	| Artportalen project id.    	|  	|
| point 	| PointGeoShape 	| `{ "coordinates": [15.01, 58.05],"type": "point" }` 	| The geographic center of the Location (WGS84).    	|  	|
| pointWithBuffer 	| PolygonGeoShape 	| `{"coordinates": [[[15.02,58.05],[15.02,58.06],...]],"type": "polygon"}`  	| The site polygon if the site is a polygon in Artportalen. Otherwise it is a circle with `point` as center and `coordinateUncertaintyInMeters` as radius (WGS84).    	|  	|