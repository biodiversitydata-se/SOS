# Harvesting many observations with ObservationsByCursor

The `ObservationsByCursor` endpoint is designed for retrieving large numbers of observations programmatically, for example when harvesting observations or paginating through a large result set. It complements the [export endpoints](Exports.md), which produce files, by returning observations as JSON pages that your own code can process as they arrive.

- Endpoint: `POST /Observations/SearchByCursor`
- [Endpoint in the API portal](https://api-portal.artdatabanken.se/api-details#api=sos-api-v1&operation=Observations_ObservationsByCursor)
- [Integration tests with examples of how the endpoint is used](https://github.com/biodiversitydata-se/SOS/blob/master/Tests/Integration/SOS.Observations.Api.IntegrationTests/Tests/ApiEndpoints/ObservationsEndpoints/ObservationsByCursorEndpoint/ObservationsByCursorTests.cs)


## Which approach should I use?

| Need | Use |
|:---|:---|
| Up to 10 000 observations as JSON | `/Observations/Search` with `skip` and `take` |
| Up to 2 million observations as JSON | `/Observations/SearchByCursor` (this document) |
| Up to 25 000 observations as a file export | [`/Exports/Download/*`](Exports.md) |
| All observations (except protected) | Download the DwC-A dataset, see [below](#downloading-everything-as-dwc-a) |

## Request

The body is a search filter, the same as for the other search endpoints. See [search filter parameters](SearchFilter.md).

| Query parameter | Default | Description |
|:---|:---|:---|
| `take` | 1000 | Number of observations per page. Maximum is 2 500. |
| `cursor` | | Omit in the first request. In following requests, pass the `nextCursor` value from the previous response. |
| `sortBy` | `modified` | Field to sort by. Must be the same in all requests of a harvest. |
| `sortOrder` | `Asc` | `Asc` or `Desc`. Must be the same in all requests of a harvest. |

## Response

| Property | Description |
|:---|:---|
| `records` | The observations in this page. |
| `nextCursor` | Pass this as `cursor` to get the next page. Null or empty when there are no more pages. |
| `take` | The page size that was requested. |
| `totalCount` | Number of observations matching the filter. |

## How to paginate

1. Send the first request without `cursor`.
2. Process `records`.
3. If `nextCursor` is null or empty, you are done. Otherwise send the same request again, with the same filter, `sortBy` and `sortOrder`, and with `cursor` set to `nextCursor`.
4. Repeat from step 2.

```csharp
var url = "observations/searchbycursor?take=1000&sortBy=modified&sortOrder=asc";
var filter = new SearchFilterDto { OccurrenceStatus = OccurrenceStatusFilterValuesDto.Present };
string? cursor = null;

do
{
    var requestUrl = cursor == null ? url : $"{url}&cursor={Uri.EscapeDataString(cursor)}";
    var response = await client.PostAsJsonAsync(requestUrl, filter);
    response.EnsureSuccessStatusCode();
    var page = await response.Content.ReadFromJsonAsync<SearchByCursorResultDto<Observation>>();

    foreach (var observation in page.Records ?? [])
    {
        // Process the observation
    }

    cursor = page.NextCursor;
} while (!string.IsNullOrEmpty(cursor));
```

In a real harvest, send your subscription key in the `Ocp-Apim-Subscription-Key` header (see the [FAQ](FAQ.md)).

## Things to be aware of

- **Limit of 2 million observations.** Check `totalCount` in the first response. If it exceeds 2 000 000, requests that include a `cursor` fail with 400 Bad Request. Narrow the search filter, for example by date interval, and harvest it in several parts.
- **Live view.** Each request reads current data. `totalCount` can change between requests, and observations that are added, changed or removed during the harvest can cause duplicates or missing observations. De-duplicate on `occurrence.occurrenceId`. Sorting by `modified` ascending makes it easy to run a new harvest later with a filter on `modified` to pick up changes.
- **Keep the sort order.** Changing `sortBy` or `sortOrder` between pages gives wrong results.
- **Max page size.** `take` above 2 500 returns 400 Bad Request.

## Examples in the integration tests

[ObservationsByCursorTests.cs](https://github.com/biodiversitydata-se/SOS/blob/master/Tests/Integration/SOS.Observations.Api.IntegrationTests/Tests/ApiEndpoints/ObservationsEndpoints/ObservationsByCursorEndpoint/ObservationsByCursorTests.cs) shows how the endpoint is intended to be used. The tests are easy to convert to other languages, for example Python.


## Downloading everything as DwC-A

To work with all observations (except protected), download the complete dataset as a Darwin Core Archive and filter it yourself. Use the [Exports_GetDatasetsList](https://api-portal.artdatabanken.se/api-details#api=sos-api-v1&operation=Exports_GetDatasetsList) endpoint to list the available datasets and download the one with key `sos.dwca.zip`. The file is about 20 GB.
