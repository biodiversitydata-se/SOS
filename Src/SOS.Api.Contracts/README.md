# SOS API Contracts

`SOS.Api.Contracts` contains the public request, response, GeoJSON and enum types used by the supported SOS Observations API endpoints.

Install this package when an application only needs the typed wire contracts. Applications that call the API should normally install `SOS.Api.Client`, which references this package automatically.

The contracts follow the JSON API and do not expose the server's `SOS.Lib` assembly.
