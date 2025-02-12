using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SOS.Lib.iNatFinland;
public class postDw
{
    public static string InatStagingToken { get; set; }
    public static string InatProductionToken { get; set; }

  
    private static readonly HttpClient client = new HttpClient();

    public static async Task Main(string[] args)
    {
        // Example usage
        var dwObs = new { /* your data object here */ };
        await PostSingle(dwObs, "staging");
        await PostMulti(dwObs, "production");
    }

    public static async Task<bool> PostSingle(dynamic dwObs, string target)
    {
        string targetUrl = GetTargetUrl(target, true);
        Console.WriteLine("Pushing to " + targetUrl);
        return await PostData(dwObs, targetUrl);
    }

    public static async Task<bool> PostMulti(dynamic dwObs, string target)
    {
        string targetUrl = GetTargetUrl(target, false);
        Console.WriteLine("Pushing to " + targetUrl);
        return await PostData(dwObs, targetUrl);
    }

    private static string GetTargetUrl(string target, bool isSingle)
    {
        return null;
        // todo - implement
        //string baseUrl = target == "staging" ? "https://apitest.laji.fi/v0/warehouse/push?access_token=" : "https://api.laji.fi/v0/warehouse/push?access_token=";
        //string token = target == "staging" ? SecretData.InatStagingToken : SecretData.InatProductionToken;
        //return baseUrl + token;
    }

    private static async Task<bool> PostData(dynamic dwObs, string targetUrl)
    {
        string dwObsJson = JsonSerializer.Serialize(dwObs);
        var content = new StringContent(dwObsJson, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(targetUrl, content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("API responded " + (int)response.StatusCode);
            return true;
        }
        else
        {
            string errorCode = ((int)response.StatusCode).ToString();
            Console.WriteLine(await response.Content.ReadAsStringAsync()); // DEBUG
            throw new Exception($"API responded with error {errorCode}");
        }
    }
    
}
