# SOS Data Stewardship API

## Testdata från Artportalen
`identifier` kan användas för att söka fram dataset. Dataset innehåller `eventId:n`. `Event` innehåller `occurrenceId:n`. De `project` (id:n) som specificeras nedan kan användas i Artportalen för att leta reda på observationer som också har skördats till Data Stewardship API.
```json
[
  {
    "identifier": "Artportalen - Mossor",
    "project": [3986, 3764, 1391, 3946, 3945]
  },
  {
    "identifier": "Artportalen - Fladdermöss",
    "project": [3606]
  }
]
```

## Klart att testa
Följande är klart för test:

### GET /datasets/{id}
1. Hämta dataset med datasetId. Använd `identifier` som specificeras ovan.
2. Hämta dataset som inte finns. Verifiera att lämplig returkod returneras.
3. Testa att sätta exportmode=CSV.

### POST /datasets
- todo...

### GET /events/{id}
1. Hämta event med eventId. Sök först fram ett dataset med något av de dataset som specificeras ovan. Där finns det eventId:n som går att använda.
2. Hämta observation med eventId som inte finns. Verifiera att lämplig returkod returneras.
3. Testa att sätta exportmode=CSV.

### POST /events
- todo...

### GET /occurrences/{id}
1. Hämta observation med occurrenceId. Ta en observation som finns i något av Artportalenprojekten som specificeras ovan.
2. Hämta observation med occurrenceId som inte finns. Verifiera att lämplig returkod returneras.
3. Testa att använda olika koordinatsystem.
4. Testa att sätta exportmode=CSV.

### POST /occurrences
- todo...


---
## Förslag till förändringar i API-specifikationen


## Filter som återstår


## Funktionalitet som återstår i Datavärdskap API
- Optimeringar kring Elasticsearch projiceringar. Nu returneras alltid alla fält, men det skulle säkert kunna begränsas.
- Intern Health check
- Publik Health check
- Hantera skyddade observationer
- Hantera skyddade event

## Funktionalitet som återstår i SOS Hangfire skördning
- Lägg till alla dataset som ska in i Artportalens databas
- Bygg stöd för att skörda publika datavärdskapsdata från Lund
- Bygg stöd för att skörda skyddade datavärdskapsdata från Lund
- Testa att importera DwC-A från SLU Umeå - NILS
- Verifiera Event precis som vi verifierar observationer? Då måste Dataset.EventIds enbart ta med godkända events.

## Övrigt som återstår
- Publicera API på Azure API och koppla ihop med.
- Förbättra Open API dokumentationen.
- Skapa dokumentation i markdown.
- Skapa diagram som visar dataflöden.

## Referenser
- [Open API Specification (Lund)](https://github.com/Lund-University-Biodiversity-data/datahost-api/blob/main/api/openapi.yaml)
- [Open API Specification template (Lund)](https://github.com/Lund-University-Biodiversity-data/datahost-api/blob/main/api/templateOpenapi.yaml)