using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SOS.Lib.iNatFinland;
public class GetInat
{
    private static readonly HttpClient httpClient = new HttpClient();

    public static async Task<Dictionary<string, Dictionary<string, object>>> GetPageFromAPIAsync(string url)
    {
        Console.WriteLine("Getting " + url);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(url);
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting data from iNaturalist API", ex);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine("iNaturalist API responded " + (int)response.StatusCode);
        }
        else
        {
            Console.WriteLine("iNaturalist responded with error " + (int)response.StatusCode);
            return null;
        }

        try
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var inatResponseDict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(jsonResponse);
            return inatResponseDict;
        }
        catch (JsonException)
        {
            Console.WriteLine("iNaturalist responded with invalid JSON");
            return null;
        }
    }

    public static async IAsyncEnumerable<Dictionary<string, Dictionary<string, object>>> GetUpdatedGenerator(int latestObsId, string latestUpdateTime, int pageLimit, int perPage, int sleepSeconds, string urlSuffix = "")
    {
        int page = 1;

        while (page <= pageLimit)
        {
            Console.WriteLine($"Getting set number {page} of {pageLimit} latestObsId {latestObsId} latestUpdateTime {latestUpdateTime}");

            // place_id filter: Finland, Åland & Finland EEZ
            string url = $"https://api.inaturalist.org/v1/observations?place_id=7020%2C10282%2C165234&page={page}&per_page={perPage}&order=asc&order_by=id&updated_since={latestUpdateTime}&id_above={latestObsId}&include_new_projects=true{urlSuffix}";

            if (url.Contains(" "))
            {
                throw new Exception("iNat API url malformed, contains space(s)");
            }

            var inatResponseDict = await GetPageFromAPIAsync(url);

            if (inatResponseDict == null)
            {
                await Task.Delay(10000);
                continue;
            }

            var resultObservationCount = int.Parse(inatResponseDict["total_results"].ToString());
            Console.WriteLine("Search matched " + resultObservationCount + " observations.");

            if (resultObservationCount == 0)
            {
                Console.WriteLine("No more observations.");
                yield return null;
                break;
            }
            else
            {
                latestObsId = 1; // todo - int.Parse(((JObject)inatResponseDict["results"])[^1]["id"].ToString());
                page++;

                await Task.Delay(sleepSeconds * 1000);

                yield return inatResponseDict;
            }
        }
    }

    public static async Task<Dictionary<string, Dictionary<string, object>>> GetSingle(int observationId)
    {
        string url = $"https://api.inaturalist.org/v1/observations?id={observationId}&order=desc&order_by=created_at&include_new_projects=true";

        var inatResponseDict = await GetPageFromAPIAsync(url);

        if (inatResponseDict == null || int.Parse(inatResponseDict["total_results"].ToString()) == 0)
        {
            throw new Exception("Zero results from iNaturalist API");
        }

        return inatResponseDict;
    }
}

