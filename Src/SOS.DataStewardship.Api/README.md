# SOS Data Stewardship API

## Testning

### Testmiljö
https://sos-datastewardship-st.artdata.slu.se/swagger/index.html

### Testdata från Artportalen
- Observationer och event skördas in i testmiljön från 2016-01-01 och framåt.
- `identifier` kan användas för att söka fram dataset. Dataset innehåller `eventId:n`. `Event` innehåller `occurrenceId:n`. De `project` (id:n) som specificeras nedan kan användas i Artportalen för att leta reda på observationer som också har skördats till Data Stewardship API.
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

### Att testa

#### GET /datasets/{id}
1. Hämta dataset med datasetId. Använd `identifier` som specificeras ovan.
2. Hämta dataset som inte finns. Verifiera att lämplig returkod returneras.
3. Testa att sätta exportmode=CSV.

#### POST /datasets
1. Använd filtret `DatasetList`. Använd `identifier` som specificeras ovan.
2. Använd filtret `Datum`. Testa med olika kombinationer av `StartDate` och `EndDate` och `DatumFilterType`
3. Använd filtret `Taxon` och ange olika TaxonId.
4. Använd filtret `Area` med fördefinierade regioner (`County`, `Municipality`, `Parish`, `Province`).
5. Använd filtret `GeographicArea` och ange en polygon i `GeoJSON`-format. Testa också att ange en punkt och `MaxDistanceFromGeometries`.
6. Testa kombinationer av filter.
7. Testa felaktiga filter. Taxon som inte finns, regioner som inte finns, etc.

#### GET /events/{id}
1. Hämta event med eventId. Sök först fram ett dataset med något av de dataset som specificeras ovan. Där finns det eventId:n som går att använda.
2. Hämta observation med eventId som inte finns. Verifiera att lämplig returkod returneras.
3. Testa att sätta exportmode=CSV.
4. Verifiera att värdena verkar korrekta. Jämför med Artportalen.

#### POST /events
1. Använd filtret `DatasetList`. Använd `identifier` som specificeras ovan.
2. Använd filtret `Datum`. Testa med olika kombinationer av `StartDate` och `EndDate` och `DatumFilterType`
3. Använd filtret `Taxon` och ange olika TaxonId.
4. Använd filtret `Area` med fördefinierade regioner (`County`, `Municipality`, `Parish`, `Province`).
5. Använd filtret `GeographicArea` och ange en polygon i `GeoJSON`-format. Testa också att ange en punkt och `MaxDistanceFromGeometries`.
6. Testa med olika värden på `responseCoordinateSystem`.
7. Testa kombinationer av filter.
8. Testa felaktiga filter. Taxon som inte finns, regioner som inte finns, etc.

#### GET /occurrences/{id}
1. Hämta observation med occurrenceId. Ta en observation som finns i något av Artportalenprojekten som specificeras ovan.
2. Hämta observation med occurrenceId som inte finns. Verifiera att lämplig returkod returneras.
3. Testa att använda olika koordinatsystem.
4. Testa att sätta exportmode=CSV.
5. Verifiera att värdena verkar korrekta. Jämför med Artportalen.

#### POST /occurrences
1. Använd filtret `DatasetIds`. Använd `identifier` som specificeras ovan.
2. Använd filtret `Datum`. Testa med olika kombinationer av `StartDate` och `EndDate` och `DatumFilterType`
3. Använd filtret `Taxon` och ange olika TaxonId.
4. Använd filtret `Area` med fördefinierade regioner (`County`, `Municipality`, `Parish`, `Province`).
5. Använd filtret `GeographicArea` och ange en polygon i `GeoJSON`-format. Testa också att ange en punkt och `MaxDistanceFromGeometries`.
6. Använd filtret `EventIds`.
7. Testa med olika värden på `responseCoordinateSystem`.
8. Använd kombinationer av filter.
9. Testa felaktiga filter. Taxon som inte finns, regioner som inte finns, etc.

---

## Eventuellt ändra

### Dataset
1. När man söker dataset `POST Datasets` med filtret `DatasetList` så görs sökningen först mot observationer som har den `DatasetIdentifiern`. Sökningen borde göras direkt mot `Dataset-indexet` i Elasticsearch?
2. När man söker dataset `POST Datasets` med filtret `Datum` så görs sökningen först mot observationer. Oklart om det ska vara så eller om sökningen först ska gå direkt mot `Dataset-indexet` i Elasticsearch?

### Event
1. När man söker events `POST Events` med filtret `DatasetList` så görs sökningen först mot observationer som har den `DatasetIdentifiern`. Sökningen borde göras direkt mot `Events-indexet` i Elasticsearch?
2. När man söker events `POST Events` med filtret `Datum` så görs sökningen först mot observationer. Oklart om det ska vara så eller om sökningen först ska gå direkt mot `Events-indexet` i Elasticsearch?
3. När man söker events `POST Events` med filtret `Area` så görs sökningen först mot observationer. Oklart om det ska vara så eller om sökningen först ska gå direkt mot `Events-indexet` i Elasticsearch?

---
## Förslag till förändringar i API-specifikationen

### DatasetFilter, EventsFilter, OccurrenceFilter
- `DatasetList` borde hellre heta `DatasetIds`? `OccurrenceFilter` använder namnet `DatasetIds` så det är inte konsekvent just nu.
- `Datum` borde hellre heta `DateFilter` eller `Date`? `Date` är i vissa programmeringsspråk ett reserverat ord vilket gör att `DateFilter` kanske är ett bättre förslag?
- `DatumFilterType` borde hellre heta `DateFilterType`?
- `Area.Area` bord hellre heta `Area.Geometry`?
- Lägg till `EventIds` property till `EventsFilter`?

### OccurrenceModel, EventModel
- Det finns en property `OccurrenceModel.DatasetIdentifier` av sträng-typ, sedan finns det `EventModel.Dataset`. `OccurrenceModel.DatasetIdentifier` borde kanske bytas till `OccurrenceModel.Dataset` och ha samma typ som `EventModel.Dataset`?
- `decimal`-datatypen används för en del värden. Det borde väl räcka med `double`?

## Funktionalitet som återstår i Datavärdskap API
- Optimeringar kring Elasticsearch projiceringar. Nu returneras alltid alla fält, men det skulle säkert kunna begränsas.
- Intern Health check
- Publik Health check
- Hantera skyddade observationer
- Hantera skyddade event. Hantera `Event.LocationProtected`.
- Går det att ta bort `[DataMember]` attribut? 
- Kolla upp: Mappas `Sex`, `LifeStage`, `Activity` korrekt?
- Kolla upp: Mappas `Unit` korrekt?
- Kolla upp: Mappas `QuantityVariable` korrekt?
- Rensa bort properties som troligtvis inte ska finnas med, t.ex. `ObservationPointTest`.


## Funktionalitet som återstår i SOS Hangfire skördning
- Lägg till alla dataset som ska in i Artportalens databas
- Bygg stöd för att skörda publika datavärdskapsdata från Lund
- Bygg stöd för att skörda skyddade datavärdskapsdata från Lund
- Testa att importera DwC-A från SLU Umeå - NILS
- Verifiera Event precis som vi verifierar observationer? Då måste Dataset.EventIds enbart ta med godkända events.
- Bygg stöd för propertyn `Event.Weather`.
- Kolla upp: Mappas `Sex`, `LifeStage`, `Activity` korrekt?
- Kolla upp: Mappas `Unit` korrekt?
- Kolla upp: Mappas `QuantityVariable` korrekt?
- Byt namn på klass och tillhörande ES-index `ObservationEvent`?
- Byt namn på klass och tillhörande ES-index `ObservationDataset`?

## Övrigt som återstår
- Publicera API på Azure API och koppla ihop med interna API:et.
- Förbättra Open API dokumentationen.
- Skapa dokumentation i markdown.
- Skapa diagram som visar dataflöden.
- Deploya på Kubernetes-kluster?

## Referenser
- [Open API Specification (Lund)](https://github.com/Lund-University-Biodiversity-data/datahost-api/blob/main/api/openapi.yaml)
- [Open API Specification template (Lund)](https://github.com/Lund-University-Biodiversity-data/datahost-api/blob/main/api/templateOpenapi.yaml)