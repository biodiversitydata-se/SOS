# Signalsök – Dokumentation

## 1. Översikt

Signalsök är en funktion som möjliggör sökning efter **skyddade fynd** utan att avslöja känslig information. Resultatet av en signalsökning är ett binärt svar: **Ja** eller **Nej**, som anger om det finns minst ett skyddat fynd som matchar angivna sökkriterier.

Signalsök är utformad för att:

* Skydda arter och lokaler genom att inte exponera fynddetaljer.
* Ge myndigheter och andra behöriga aktörer möjlighet att fatta beslut baserat på förekomstinformation. Ett Ja-svar kan fungera som en första indikation för en handläggare, som därefter kan ta kontakt med en person med högre behörighet som har tillgång till mer detaljerad information om fyndet eller fynden och kan besluta om vidare åtgärder.
* Säkerställa att endast behöriga användare kan utföra sökningar, och endast inom tillåtna geografiska områden.

> **Viktigt:** Endast skyddade fynd ingår i signalsök. Publika fynd inkluderas aldrig i sökningen.

---

## 2. Vad innebär ett Ja/Nej-svar?

* **Ja (true)**: Det finns minst ett skyddat fynd som matchar sökkriterierna.
* **Nej (false)**: Det finns inga skyddade fynd som matchar sökkriterierna.

Användaren får **inte** veta:

* Vilken eller vilka arter som träffades
* Antal fynd
* Exakta eller approximativa positioner

---

## 3. Behörighet och åtkomst

### 3.1 Grundläggande behörighetskrav

För att kunna utföra signalsök krävs:

* En roll som ger tillgång till **Signalsök**. Rollen måste ha en rättighet med identifieraren `SightingIndication`.

Behörighet kan ges:

* För hela Sverige (ovanligt)
* För ett avgränsat område, t.ex. ett län

### 3.2 Begränsning till geografiska områden

Om en användare försöker utföra signalsök i ett område hen saknar behörighet för kan följande inträffa:

* **HTTP 403 (Forbidden)**: Användaren saknar behörighet för hela sökområdet
* **HTTP 409 (Conflict)**: Användaren har behörighet för delar av sökområdet

Detta beteende styrs av parametern `returnHttp4xxWhenNoPermissions`.

### 3.3 Signalsök och skyddsklasser

Signalsök söker bland alla skyddade observationer, dvs. de med skyddsklass 3,4 och 5. Den söker ej bland observationer med skyddsklass 1 (publika fynd).

---

## 4. Var finns Signalsök?

Signalsök-endpointen är tillgänglig för interna applikationer på SLU och externa system via API:et *Species Observation System API (Internal)* på [Artdatabankens utvecklarportal](https://api-portal.artdatabanken.se).

Signalsök används i flera applikationer, till exempel:

* **Artsök** (Länsstyrelserna)
* **Navet** (Skogsstyrelsen)

Det finns två huvudtyper av användare:

* **Personanvändare** (inloggning via webbgränssnitt)
* **Applikationsanvändare** (system-till-system)


---

## 5. Sökkriterier (Search Filter)

För att en signalsökning ska vara giltig måste följande krav vara uppfyllda:

### 5.1 Geografiskt område (obligatoriskt)

Sökningen måste avse ett geografiskt område:

* Region
* Polygon(er)
* Bounding box

### 5.2 Taxonfilter (obligatoriskt)

* Minst **en taxonlista** måste anges
* Minst en av taxonlistorna måste vara en **obligatorisk signalsök-taxonomilista**

Signalsök-taxalistor:

* Fridlysta arter (Id 1)
* Rödlistade arter (Id 7)
* Habitatdirektivet (Id 8)
* Åtgärdsprogram (Id 17)
* Skogsstyrelsens naturvårdsarter (Id 18)

Om ingen obligatorisk lista anges returneras **HTTP 400 (Bad Request)**.

### 5.3 Datum (obligatoriskt)

* `StartDate` måste vara **minst ett år tillbaka i tiden**
* Nyare datum leder till valideringsfel

### 5.4 Övriga filter (valfria)

* Fågelhäckningkriterie (`BirdNestActivityLimit`)
* Datakällor (`DataProvider`)
* Artportalen-typfilter (`ArtportalenTypeFilter`)

---

## 6. Geografiskt filter

Med det geografiska filtret kan användaren specificera hur geografisk information ska hanteras vid signalsökning för att ta hänsyn till osäkerhet i observationer och arters känslighet för störning.
Observationer i databasen kan ha geografisk representation på flera nivåer, vilket påverkar hur sökningen utförs.

### 6.1 Hur geografisk information lagras

Varje observation kan representeras geografiskt på följande sätt:

1. **Mittpunkt (location.point)**.
   En punkt som representerar observationens angivna position, tillsammans med en koordinatosäkerhet (`coordinateUncertaintyInMeters`).

2. **Polygonyta (location.pointWithBuffer)**.
   En cirkulär polygon där mittpunkten är observationens punkt och radien motsvarar koordinatosäkerheten (`coordinateUncertaintyInMeters`).
   För **polygonlokaler** lagras här istället den **exakta polygonen** som beskriver observationens utbredning.

3. **Störningsyta (location.pointWithDisturbanceBuffer)**.
   En cirkulär polygon där mittpunkten är observationens punkt och radien motsvarar taxonets definierade störningsradie. Denna yta används för arter som är känsliga för störning.

### 6.2 Hur geometri används vid sökning

Vilken geografisk representation som används i signalsökningen styrs av vilka parametrar som är aktiverade. Dessa avgör om sökningen ska ta hänsyn till osäkerhet i observationens position och/eller artens störningskänslighet.

* **considerObservationAccuracy = true**.
  Sökningen görs mot polygonytan `location.pointWithBuffer`.
  Observationer vars mittpunkt ligger utanför sökgeometrin kan ändå inkluderas, förutsatt att någon del av observationens polygonyta skär eller överlappar sökområdet.

* **considerDisturbanceRadius = true**.
  Sökningen görs mot störningsytan `location.pointWithDisturbanceBuffer`.
  Observationer vars mittpunkt ligger utanför sökgeometrin kan ändå inkluderas, förutsatt att någon del av observationens störningsyta skär eller överlappar sökområdet.

* **considerObservationAccuracy = false** och **considerDisturbanceRadius = false**.
  Sökningen görs enbart mot observationens mittpunkt (`location.point`). Endast observationer vars punkt ligger inom sökgeometrin kan då ge träff.

### 6.3 Begränsning baserat på noggrannhet

* Om parametern **maxAccuracy** är satt inkluderas endast observationer vars koordinatosäkerhet (`coordinateUncertaintyInMeters`) är **mindre än eller lika med** angivet värde.

Denna begränsning tillämpas oavsett vilken geografisk representation som i övrigt används vid sökningen.


---

## 7. Loggning och spårbarhet

Alla signalsökningar loggas för att säkerställa:

* Spårbarhet
* Uppföljning av användning
* Säkerhetsgranskning

---

## 8. API-endpoint för Signalsök

### 8.1 Endpoint

```
Observations_SignalSearchInternal
```

### 8.2 Headers

| Header                                 | Beskrivning                                |
| -------------------------------------- | ------------------------------------------ |
| X-Authorization-Role-Id                | Begränsar behörighet till en specifik roll |
| X-Authorization-Application-Identifier | Applikation som används vid auktorisering  |

### 8.3 Query-parametrar

| Parameter                      | Beskrivning                                | Standard |
| ------------------------------ | ------------------------------------------ | -------- |
| areaBuffer                     | Buffert runt område (0–100 m)              | 0        |
| onlyAboveMyClearance           | Signal endast över användarens behörighet? | true     |
| returnHttp4xxWhenNoPermissions | Returnera 403/409 vid bristande behörighet | false    |

### 8.4 Response

| HTTP-status             | Betydelse                         |
| ----------------------- | --------------------------------- |
| 200 OK                  | Returnerar `true` eller `false`   |
| 400 Bad Request         | Ogiltiga sökparametrar            |
| 401 Unauthorized        | Saknar autentisering              |
| 403 Forbidden           | Saknar behörighet till region     |
| 409 Conflict            | Delvis behörighet till region l   |