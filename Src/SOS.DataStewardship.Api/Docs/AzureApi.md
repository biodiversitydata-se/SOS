# Azure API

## Hur uppdaterar man Azure API?
1. Spara Open API-specifikationen från Swagger UI i prod-miljön som `Docs/OpenApi/data-stewardship-api.json`.
2. Kopiera `info` och `servers` segmenten från `azure-data-stewardship-api.json` till `data-stewardship-api.json`.
3. Skapa en ny revision av API:et i Azure API. Ladda in filen `data-stewardship-api.json`.
4. Ta bort de endpoints som inte ska exponeras via Azure API.
5. Ändra Web Service Url under Settings till `https://api2.artdata.slu.se/data-stewardship-api/v1/`.
6. Sätt revisionen till current version.
