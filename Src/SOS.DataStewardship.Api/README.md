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
2. Använd filtret `Datum`. Testa med olika kombinationer av `StartDate` och `EndDate` och `DateFilterType`
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
2. Använd filtret `Datum`. Testa med olika kombinationer av `StartDate` och `EndDate` och `DateFilterType`
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
2. Använd filtret `Datum`. Testa med olika kombinationer av `StartDate` och `EndDate` och `DateFilterType`
3. Använd filtret `Taxon` och ange olika TaxonId.
4. Använd filtret `Area` med fördefinierade regioner (`County`, `Municipality`, `Parish`, `Province`).
5. Använd filtret `GeographicArea` och ange en polygon i `GeoJSON`-format. Testa också att ange en punkt och `MaxDistanceFromGeometries`.
6. Använd filtret `EventIds`.
7. Testa med olika värden på `responseCoordinateSystem`.
8. Använd kombinationer av filter.
9. Testa felaktiga filter. Taxon som inte finns, regioner som inte finns, etc.

---

## API diff (DatasetFilter, EventsFilter, OccurrenceFilter)
- `DatasetList` heter `DatasetIds`.
- `Datum` heter `DateFilter`
- `Area.Area` heter `Area.Geometry`
- `EventDataset` heter `DatasetInfo`

## Funktionalitet som återstår i Datavärdskap API
- Intern Health check
- Publik Health check
- Hantera skyddade observationer
- Hantera skyddade event. Hantera `Event.LocationProtected`.
- Kolla upp: Mappas `LifeStage`, `Activity` korrekt?
- Kolla upp: Mappas `Unit` korrekt?
- Kolla upp: Mappas `QuantityVariable` korrekt?

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

## Övrigt som återstår
- Publicera API på Azure API och koppla ihop med interna API:et.
- Förbättra Open API dokumentationen.
- Skapa dokumentation i markdown.
- Skapa diagram som visar dataflöden.
- Deploya på Kubernetes-kluster?

## Referenser
- [Open API Specification (Lund)](https://github.com/Lund-University-Biodiversity-data/datahost-api/blob/main/api/openapi.yaml)
- [Open API Specification template (Lund)](https://github.com/Lund-University-Biodiversity-data/datahost-api/blob/main/api/templateOpenapi.yaml)

# Länkar
- https://github.com/domaindrivendev/Swashbuckle.AspNetCore#enrich-operation-metadata