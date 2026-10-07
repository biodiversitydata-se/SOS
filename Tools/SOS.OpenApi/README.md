# OpenAPI export och jämförelse

Exportera de fyra Observations- och två Analysis-dokumenten utan att starta API:erna, databaser eller tester:

```powershell
dotnet run --project Tools/SOS.OpenApi -c Release -- (Join-Path $env:TEMP 'sos-openapi-current')
```

Verktyget använder API:ernas Swagger-konfiguration och kräver en explicit utdatakatalog. Lägg tillfälliga specifikationer i Temp, inte i Git. Det använder inga utvecklingshemligheter eller databasanslutningar och är inte ett publicerbart paket.

Jämför två exportkataloger semantiskt (object-property-ordning ignoreras; övriga skillnader redovisas):

```powershell
./Tools/SOS.OpenApi/CompareOpenApi.ps1 -BaselineDirectory (Join-Path $env:TEMP 'sos-openapi-before') -CurrentDirectory (Join-Path $env:TEMP 'sos-openapi-current')
```

Kvarvarande Swagger-tester verifierar kontrakten och gör alltid assertioner. Filgenerering är inte längre ett test som kan rapportera godkänt utan att göra något.
