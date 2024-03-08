# Diffusering av Artportalen observationer

## Exempel - Artportalen observation

| Property       | Värde | Kommentar                                                                             |
| -------------- | ----- | ------------------------------------------------------------------------------------- |
| Id             | 1     |                                                                                       |
| DiffusionId    | 1000  | Antal meter som diffuseringen har skett med. Eventuellt är det längden på en kvadrat? |
| Accuracy       | 20    | Koordinatnoggrannhet på den verkliga koordinaten                                      |
| XCoord         | 10    | Den verkliga koordinaten                                                              |
| YCoord         | 10    | Den verkliga koordinaten                                                              |
| DiffusedXCoord | 11    | Diffuserad koordinat                                                                  |
| DiffusedYCoord | 9     | Diffuserad koordinat                                                                  |

## SOS - processning till publikt index
| Property                               | Värde | Kommentar                                                           |
| -------------------------------------- | ----- | ------------------------------------------------------------------- |
| Id                                     | 1     |                                                                     |
| Location.CoordinateUncertaintyInMeters | 1000  | Ska värdet egentligen vara `Sqrt(1000^2 + 1000^2)`?                   |
| Location.XCoord                        | 11    | Diffuserad koordinat                                                |
| Location.YCoord                        | 9     | Diffuserad koordinat                                                |
| IsGeneralized                          | true  |                                                                     |
| HasGeneralizedObsInOtherIndex          | false | Visas inte utåt, men behövs för att kunna få till filtrena korrekt. |

## SOS - processning till skyddat index

| Property                               | Värde | Kommentar                                                           |
| -------------------------------------- | ----- | ------------------------------------------------------------------- |
| Id                                     | 1     |                                                                     |
| Location.CoordinateUncertaintyInMeters | 20    |                                                                     |
| Location.XCoord                        | 10    | Den verkliga koordinaten                                            |
| Location.YCoord                        | 10    | Den verkliga koordinaten                                            |
| IsGeneralized                          | false |                                                                     |
| HasGeneralizedObsInOtherIndex          | true  | Visas inte utåt, men behövs för att kunna få till filtrena korrekt. |

# Sökningar
## Sök skyddade fynd utan filter
Ger inga svar. Om det hade gett svar hade kontraktet brutits mot befintliga användare som ställer frågor mot både publika och skyddade index och som sätter ihop resultatet i sin klient, dvs det skulle kunna bli dubbletter i sådana fall.

**Request**
```json
{
	"ProtectionFilter": "Sensitive"
}
```

**Response**
```json
[]
```

## Sök skyddade fynd med filter
Om användaren har rätt att se fyndet, så returneras observationen med rätt koordinater

**Request**
```json
{
	"ProtectionFilter": "Sensitive",
	"GeneralizationFilter" {
	    "SensitiveGeneralizationFilter": "IncludeGeneralizedObservations"
	}
}
```

**Response**
```json
[{
   "Id": 1,
   "IsGeneralized": false,
   "Location": {
		"XCoord": 10,
		"YCoord": 10,
		"CoordinateUncertaintyInMeters": 20
   }
}]
```


## Sök publika fynd
Returnerar observation med diffuserade koordinater. Användaren får en indikation, `IsGeneralized`, om att observation är diffuserad och kan ställa ytterligare en fråga till det skyddade indexet och får tillbaka rätt koordinat om hen har rättigheter.

**Request**
```json
{
	"ProtectionFilter": "Public"
}
```

**Response**
```json
[{
   "Id": 1,
   "IsGeneralized": true,
   "Location": {
		"XCoord": 11,
		"YCoord": 9,
		"CoordinateUncertaintyInMeters": 1000
   }
}]
```

## Sök publika och skyddade fynd
Returnerar observation med diffuserade koordinater. Användaren får en indikation, `IsGeneralized`, om att observation är diffuserad och kan ställa ytterligare en fråga till det skyddade indexet och får tillbaka rätt koordinat om hen har rättigheter.

**Request**
```json
{
	"ProtectionFilter": "BothPublicAndSensitive"
}
```

**Response**
```json
[{
   "Id": 1,
   "IsGeneralized": true,
   "Location": {
		"XCoord": 11,
		"YCoord": 9,
		"CoordinateUncertaintyInMeters": 1000
   }
}]
```