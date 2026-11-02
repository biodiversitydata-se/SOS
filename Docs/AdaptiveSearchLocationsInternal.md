# `AdaptiveSearchLocationsInternal`

Intern endpoint för kartor med panorering och zoom. Den returnerar GeoJSON med lokaler och kluster, eller geogridrutor när ett lokalresultat blir för stort. Enskilda observationer används som underlag men returneras inte som egna poster.

## Kom igång

Skicka sökfiltret i body och kartans utsnitt och upplösning som query-parametrar:

```http
POST /observations/internal/adaptivesearchlocations?bbox=14.80,57.90,15.20,58.15&metersPerPixel=5&createPolygons=WhenNeeded
Content-Type: application/json

{
  "taxon": { "ids": [100077] }
}
```

Body är ett `SearchFilterInternalDto`; se [sökfiltrets parametrar](SearchFilter.md). `bbox` anges som `minLongitude,minLatitude,maxLongitude,maxLatitude` i WGS 84 och ersätter eventuell bbox i body. Minvärdena måste vara lägre än maxvärdena. `filter.protectionFilter` har företräde framför query-parametern `sensitiveObservations`.

Vid panorering ändras `bbox`; vid zoom ändras även `metersPerPixel`. Läs alltid `X-Result-Type` innan svaret tolkas:

| Värde | Innehåll |
|---|---|
| `locations` | Lokaler och eventuella lokalkluster. |
| `geogrid` | Gridceller med aggregerade observationsantal. |

## Sökstrategier

`Auto` är standard. `X-AdaptiveSearch-Strategy` visar vilken strategi som användes, även vid cacheträffar, tomma resultat och geogrid. Strategin och resultattypen är alltså två olika uppgifter.

| `adaptiveSearchStrategy` | Beteende |
|---|---|
| `Auto` | Väljer `LocationsThenGrid` när filtret garanterar exakt kartstatus utan brytår; annars `ObservationsThenGrid`. |
| `ObservationsThenGrid` | Bygger lokaler från alla matchande observationer när antalet är högst `observationsLimit`; annars geogrid. |
| `ObservationsThenLocationsThenGrid` | Försöker först med alla observationer, sedan med representanter om lokalgränsen tillåter det; annars geogrid. |
| `LocationsThenGrid` | Använder en representativ observation per lokal om lokalgränsen tillåter det; annars geogrid. |

För representanter måste både uppskattat och faktiskt lokalantal vara **lägre än** `locationAggregationLimit`. Kontrollen görs före sammanslagning av sammanfallande lokaler.

### När Auto använder representanter

Utan `cutoffYear` kan följande filter ge säker kartstatus med representanter:

| Filter | Lokalens status |
|---|---|
| Enbart positiva fynd, `protectionFilter=Public` | `Present` |
| Enbart positiva fynd, `protectionFilter=Sensitive` | `PresentSensitive` |
| Enbart negativa fynd och en icke-tom `invasiveSpeciesTreatmentIds` med enbart id:n ≥ 0 | `Treated` |
| Enbart negativa fynd, `hasInvasiveSpeciesTreatment=false`, `notRecoveredFilter=OnlyNotRecovered` | `NotRecovered` |
| Enbart negativa fynd, `hasInvasiveSpeciesTreatment=false`, `notRecoveredFilter=DontIncludeNotRecovered` | `NotPresent` |

Bedömningen följer sökfiltrets standardregler: utelämnad fyndstatus kan innebära enbart positiva fynd. `hasInvasiveSpeciesTreatment=true` räcker inte ensamt som garanti för `Treated`.

Övriga fall använder observationsvägen. Det gäller blandade positiva och negativa fynd, positiva fynd med `BothPublicAndSensitive`, sökningar med brytår och `createSampleObservations=true`.

## Klustring

Lokalresultat grupperas i denna ordning:

1. Sammanfallande lokaler bildar ett kartobjekt, även när klustring är avstängd.
2. Om `clusterRadiusMeters` anges körs fast meterklustring, oberoende av zoom och `cluster`.
3. Om `cluster=true` kan **högst ett** pixelsteg köras enligt tabellen nedan.

| Pixelsteg | När det används | Sökradie |
|---|---|---|
| Vanlig pixelklustring | Zoomgränsen och antalsgränsen tillåter det. | Börjar vid `metersPerPixel × clusterRadiusPx` meter och kan växa mot målet 400 kartobjekt. |
| Liten pixelgruppering | Zoom- eller antalsgränsen stoppar det vanliga steget. | Exakt `metersPerPixel × proximityClusterRadiusPx` meter, utan radieökning. |

När `metersPerPixel` anges kräver vanlig pixelklustring `metersPerPixel > disablePixelClusteringAtMetersPerPixel`, om zoomgränsen inte är `0`. Antalet klustringsbara kartobjekt måste också vara **större än** `minLocationCountBeforeClustering`, räknat efter metersteget och utan separata polygoner.

Med standardvärdena börjar vanlig pixelklustring vid 20 px när upplösningen är 5 m/px, alltså 100 meter. Vid 1 m/px används i stället den fasta radien 10 px, alltså 10 meter. Zoomgränsen 2 m/px väljer pixelsteg; den anger ingen radie.

Utan `metersPerPixel` beräknas vanlig klustringsradie från bbox och resultatets utbredning. Zoomgränsen och liten pixelgruppering används då inte. `cluster=false` stänger av båda pixelstegen. `proximityClusterRadiusPx=0` stänger av enbart den lilla grupperingen.

`minLocationsPerCluster` gäller alla kluster och räknar geografiska grupper; sammanfallande lokal-id:n räknas som en grupp. Metergrupper delas inte av pixelsteget. Radierna är sökradier, inte maximal klusterutbredning: närliggande grupper kan slås ihop. Den fasta pixelradien kan därför lämna fler än 400 kartobjekt.

Lämna `clusterRadiusMeters` tom för enbart zoomstyrd klustring. Metersteget används när du också behöver en fast sökradie i meter.

## Polygoner

| `createPolygons` | Beteende |
|---|---|
| `No` | Polygonlokaler returneras som centroidpunkter och kan klustras. Standard i endpointen och API-klienten. |
| `Yes` | Oklustrade polygonlokaler behåller sin geometri. Med `metersPerPixel` kan små polygoner klustras som punkter; utan upplösning hålls alla tillgängliga lokalpolygoner utanför klustringen. |
| `WhenNeeded` | Stora polygoner returneras som ytor, små som centroidpunkter som kan klustras. Kräver `metersPerPixel`. Standard i Status-kartan. |

Med kartupplösning använder `Yes` och `WhenNeeded` samma storleksgräns: minst 10 px bredd och höjd samt en faktisk area som minst motsvarar en cirkel med 10 px diameter. Stora polygoner hålls utanför båda klustringsstegen och räknas inte mot klustringens antalsmål.

Sammanfallande polygonlokaler bedöms utifrån sin gemensamma yta. Saknas polygongeometri används centroiden. Generaliserade observationer kan även ge osäkerhetsytor.

## Svaret

Svaret är en GeoJSON `FeatureCollection`. Sökningar utan träffar, även efter en tom geografisk skärning, ger HTTP 200.

### Lokalfeatures

Lokaler kan ha punkt- eller polygongeometri. Kluster är punkter vid klustrets mittpunkt. Använd `groupingType` för att identifiera kluster; `cluster` innehåller metadata och `presentation.status` styr presentationen.

| Property | Betydelse |
|---|---|
| `groupingType` | `singleLocation`, `coLocatedLocations`, `meterCluster` eller `pixelCluster`. De två sista är kluster. |
| `hasMultipleLocations` | `true` när `locations.count > 1`, även för sammanfallande lokaler. |
| `observationCount` | Totalt antal matchande observationer, även när representanter används. |
| `presentation` | Kartstatus, statusräknare, brytårsstatus och känslighetsinformation. |
| `observationSummary` | Antal taxa och delräknare för observationer. |
| `locations` | Fullständiga `count` och `ids`; `items` innehåller högst fem lokaler, sorterade efter id. |
| `cluster.radiusMeters` | Använd sökradie i meter. |
| `cluster.radiusMetersMin` | Ungefärlig radie kring klusterpunkten som omsluter medlemspositionerna, utan osäkerhetsytor. |

Använd feature-id för kartobjektets identitet och `locations.ids` vid observationssökningar. Kluster-id bygger på alla medlems-id:n, oberoende av ordning och skiftläge. Begränsningen till fem `items` gäller bara lokalernas metadata.

Med `includeFilterAreasInResult=true` tillkommer områdesfeatures märkta med `properties.FeatureType`: `FilterArea.Area` eller `FilterArea.OwnPolygon`. De har andra properties än lokalfeatures.

### Hur exakta är värdena?

Fullständigt observationsunderlag ger exakta sammanfattningar. Representanter ger fortfarande det totala observationsantalet, men vissa sammanfattningsvärden kan vara approximativa.

| Flagga | Gäller |
|---|---|
| `isPresentationStatusApproximated` | Endast `presentation.status` och `presentation.cutoffYearStatus`. Alltid `false` för `Auto`. |
| `isSummaryApproximated` | `observationSummary`, `presentation.statusCounts` och potentiellt `presentation.isAllObservationsSensitive`. Enskilda värden kan fortfarande vara exakta. |

Flaggorna följer dessa fält, inte JSON-objektens indelning. Följande förkortade lokalresultat är därför korrekt när `Auto` använder representanter för publika, positiva fynd:

```json
{
  "type": "FeatureCollection",
  "features": [{
    "type": "Feature",
    "id": "Point_5800647_1500492",
    "geometry": { "type": "Point", "coordinates": [15.00492, 58.00647] },
    "properties": {
      "groupingType": "singleLocation",
      "observationCount": 356,
      "isSummaryApproximated": true,
      "isPresentationStatusApproximated": false,
      "presentation": {
        "status": "Present",
        "statusCounts": { "Present": 1 },
        "isAllObservationsSensitive": false
      },
      "locations": { "count": 1, "ids": ["urn:lsid:artportalen.se:site:123456"] },
      "observationSummary": { "taxaCount": 1 }
    }
  }]
}
```

Kartstatusen `Present` är säker och antalet matchande observationer är 356. Statusräknaren och artantalet bygger däremot på representanten. `statusCounts` behöver därför inte summera till `observationCount`.

En publik representant ger ett säkert `isAllObservationsSensitive=false`. Enbart skyddade representanter kan ge ett osäkert `true` om filtret även tillåter publika fynd. Ett filter med enbart skyddade fynd ger ett säkert `true`.

Flaggorna beräknas även för uttryckligen valda strategier. Kluster och svarshuvuden anger `true` om någon medlem har motsvarande flagga. Tomma resultat och geogrid ger `false` i båda svarshuvudena; filterområden får inga observationsflaggor.

### Svarshuvuden och cache

| Header | Betydelse |
|---|---|
| `X-Result-Type` | `locations` eller `geogrid`. |
| `X-AdaptiveSearch-Strategy` | Effektiv strategi efter utvärdering av `Auto`. |
| `X-Observations-TotalCount` | Totalt antal matchande observationer. |
| `X-Estimated-Location-Count` | Uppskattat antal distinkta lokaler, när uppskattning finns. |
| `X-Feature-Count` | Antal primära lokal-, kluster- eller gridfeatures, utan filterområden. |
| `X-Locations-Before-Clustering` | Lokalantal före klustring, för lokalresultat. |
| `X-Zoom` | Använd geogridnivå, för gridresultat. |
| `X-Cache` | `HIT`, `MISS` eller `BYPASS`. |
| `X-AdaptiveSearch-Timings` | Servertider; överföring och kartvisning ingår inte. |
| `X-Location-Aggregation-Mode` | `representative` när representanter används. |
| `X-Location-Summary-Approximated` | Om någon features sammanfattning kan vara approximativ enligt flaggan ovan. |
| `X-Location-Presentation-Status-Approximated` | Om någon features kartstatus kan vara approximativ enligt flaggan ovan. |

Lokalcachen används för fullständiga lokalresultat; representanter och geogrid kringgår den. Klustringen anpassas till aktuell upplösning även vid cacheträffar. När svarskomprimering är aktiverad förhandlas gzip eller Brotli med `Accept-Encoding`.

## Parameterreferens

Ogiltiga parametervärden ger HTTP 400.

### Sökning och svar

| Parameter | Standard | Tillåtna värden | Funktion |
|---|---:|---|---|
| `adaptiveSearchStrategy` | `Auto` | Strategierna ovan | Väljer observations-, representant- eller gridvägen. |
| `observationsLimit` | `10000` | `0`–`10000` | Högsta observationsantal för att hämta hela underlaget. |
| `locationAggregationLimit` | `1000` | `1`–`10000` | Representanter tillåts strikt under gränsen. |
| `maxZoom` | `21` | `1`–`21` | Högsta geogridnivå, inte kartans zoom. |
| `maxGridCells` | `10000` | `1`–`10000` | Önskad övre nivå på gridantalet; används för att välja gridzoom. |
| `bbox` | `null` | Fyra WGS 84-tal | Ersätter bbox i body. |
| `returnInputBBox` | `false` | `true`/`false` | Returnerar inmatad bbox i stället för resultatets utbredning. |
| `sensitiveObservations` | `false` | `true`/`false` | Väljer skyddade/publika fynd när `filter.protectionFilter` saknas. |
| `cutoffYear` | `null` | Heltal | Delar presentationen före och från och med året; ändrar inte sökfiltret. |
| `includeFilterAreasInResult` | `false` | `true`/`false` | Lägger till filterområden som separata features. |
| `useCache` | `true` | `true`/`false` | Tillåter läsning och skrivning i lokalcachen. |
| `createSampleObservations` | `false` | `true`/`false` | Felsökningsdata för kartsymboler; `Auto` väljer observationsvägen. |

### Klustring och geometri

| Parameter | Standard | Tillåtna värden | Funktion |
|---|---:|---|---|
| `cluster` | `true` | `true`/`false` | Aktiverar de två alternativa pixelstegen. |
| `clusterRadiusMeters` | `null` | Positivt ändligt tal | Fast metersteg före pixelsteget, även vid `cluster=false`. |
| `metersPerPixel` | `null` | Positivt ändligt tal | Kartupplösning i markmeter per CSS-pixel. |
| `clusterRadiusPx` | `20` | `1`–`500` | Startradie för vanlig pixelklustring, i CSS-pixlar. |
| `disablePixelClusteringAtMetersPerPixel` | `2.0` | Icke-negativt ändligt tal | Stoppar vanligt pixelsteg vid upplösning ≤ gränsen. `0` tar bort zoomgränsen. |
| `proximityClusterRadiusPx` | `10.0` | Ändligt tal `0`–`500` | Fast pixelradie när vanligt steg stoppas. `0` stänger av den lilla grupperingen. |
| `minLocationCountBeforeClustering` | `0` | `0`–`10000` | Vanligt pixelsteg tillåts över detta antal klustringsbara kartobjekt. |
| `minLocationsPerCluster` | `2` | `2`–`100` | Minsta antal geografiska grupper per kluster. |
| `createPolygons` | `No` | `No`, `Yes`, `WhenNeeded` | Polygonläge enligt tabellen ovan. |

## Exempel med API-klienten

Med en injicerad `ISosObservationsClient`:

```csharp
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Observations.Requests;

var response = await observationsClient.AdaptiveSearchLocationsInternalAsync(
    new SearchFilterInternalDto { Taxon = new() { Ids = [100077] } },
    new AdaptiveSearchLocationsOptions
    {
        BoundingBoxQuery = "14.80,57.90,15.20,58.15",
        MetersPerPixel = 5,
        CreatePolygons = CreatePolygonsMode.WhenNeeded
    },
    cancellationToken);
```

Läs `response.ResultType` innan `response.GeoJson` tolkas. För typade lokalfeatures finns även `ISosObservationSearchService.AdaptiveSearchLocationsInternalAsFeatureCollectionAsync`; geogrid och filterområden har andra properties än lokalernas DTO.

## Status-kartan

Första sökningen startas med **Sök**. Därefter kan panorering och zoom söka automatiskt. **Kartinfo → Använd strategi** visar strategin som servern valde.

Vid pan/zoom med samma filter behålls markeringen och observationslistan. Tabellen byter innehåll först när nya observationer har hämtats. Ett ändrat filter återställer markeringen. Klick på en punkt prioriteras framför en underliggande polygon.

Under **Visning → Punktstorlek** används samma antalsbaserade skala för punkter och kluster:

| Läge | Storlek |
|---|---|
| Fast storlek | Punkter 10 px, kluster 12 px. |
| Efter antal – automatiska gränser | 10/14/18 px; gränser vid 75:e och 90:e percentilen i aktuellt punktresultat. |
| Efter antal – fasta gränser (standard) | ≤5 fynd: 10 px; 6–50: 14 px; ≥51: 18 px. |

Antalslägena använder `observationCount`. Ytor och filterområden ingår inte i automatgränserna. Visningsvalen ändrar kartan direkt utan ny sökning och påverkar inte serverns klustring. Gul klustermarkering använder den omslutande radien `cluster.radiusMetersMin`; noggrannhetscirklar och klusterradier kan visas som hjälplager.

![Lokaler och kluster](Images/adaptivesearchlocations-cluster.png)

![Geogrid](Images/adaptivesearchlocations-grid.png)

## För befintliga anrop

- Utelämnade parametrar använder standardvärdena ovan, även vid cacheträffar och för äldre anrop. Kartupplösning medför alltså zoomgränsen 2 m/px och den lilla pixelradien 10 px om de inte anges.
- `locationAggregationLimit` är ett icke-nullbart heltal med standardvärdet 1000, även för äldre strategier med representantsteget. Klientkod som tilldelar `null` måste ändras. Välj `ObservationsThenGrid` för att hoppa över representantsteget.
- `clusteringAlgorithm` och klientegenskapen `ClusteringAlgorithm` har tagits bort. Klustringen använder FastGreedy i SWEREF99 TM.
