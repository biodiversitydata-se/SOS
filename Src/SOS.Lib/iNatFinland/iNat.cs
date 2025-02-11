using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SOS.Lib.iNatFinland;
public class iNat
{
    public async Task Main(string[] args)
    {
        string target = args[0]; // staging | production
        string mode = args[1]; // auto | manual

        // Load private data
        Console.WriteLine("Loading private data");
        List<Dictionary<string, object>> privateObservationData = LoadPrivateData("./privatedata/latest.tsv");

        int rowCount = privateObservationData.Count;
        Console.WriteLine($"Loaded {rowCount} rows");

        var privateEmails = INatHelpers.LoadPrivateEmails();

        Console.WriteLine("------------------------------------------------");
        Console.WriteLine($"Starting inat.py, target {target}");

        DateTime now = DateTime.UtcNow;
        string thisUpdateTime = now.ToString("yyyy-MM-ddTHH:mm:ss+00:00");
        thisUpdateTime = thisUpdateTime.Replace(":", "%3A").Replace("+", "%2B");

        var variables = ReadVariables();

        string variableNameLatestObsId = "";
        string variableNameLatestUpdate = "";
        string urlSuffix = "";

        if (mode == "auto")
        {
            if (target == "staging")
            {
                variableNameLatestObsId = "inat_auto_staging_latest_obsId";
                variableNameLatestUpdate = "inat_auto_staging_latest_update";
            }
            else if (target == "production")
            {
                variableNameLatestObsId = "inat_auto_production_latest_obsId";
                variableNameLatestUpdate = "inat_auto_production_latest_update";
            }
        }
        else if (mode == "manual")
        {
            urlSuffix = variables["inat_MANUAL_urlSuffix"];
            if (target == "staging")
            {
                variableNameLatestObsId = "inat_MANUAL_staging_latest_obsId";
                variableNameLatestUpdate = "inat_MANUAL_staging_latest_update";
            }
            else if (target == "production")
            {
                variableNameLatestObsId = "inat_MANUAL_production_latest_obsId";
                variableNameLatestUpdate = "inat_MANUAL_production_latest_update";
            }
        }
        else
        {
            Console.WriteLine("Invalid mode");
            return;
        }

        string AirflowLatestObsId = variables[variableNameLatestObsId];
        string AirflowLatestUpdate = variables[variableNameLatestUpdate];

        AirflowLatestUpdate = ReduceMinutes(AirflowLatestUpdate, 3);

        int page = 1;
        
        int sleepSeconds = 10;
        int perPage = 100;
        int pageLimit = 10000;

        await foreach (Dictionary<string, Dictionary<string, object>> multiObservationDict in GetInat.GetUpdatedGenerator(int.Parse(AirflowLatestObsId), AirflowLatestUpdate, pageLimit, perPage, sleepSeconds, urlSuffix))
        {
            if (multiObservationDict == null)
            {
                Console.WriteLine($"Finishing, setting latest update to {thisUpdateTime}");
                SetVariable(variableNameLatestUpdate, thisUpdateTime);
                SetVariable(variableNameLatestObsId, "0");
                break;
            }

            // todo - implement
            Dictionary<string, object> res = inatToDw.ConvertObservations(null, null, privateEmails);
            //Dictionary<string, object> res = inatToDw.ConvertObservations((List<Dictionary<string, Dictionary<string, object>>>)multiObservationDict["results"], null, privateEmails);
            object dwObservations = res["observations"];
            string latestObsId = (string)res["lastUpdateKey"];

       

            bool postSuccess = await postDw.PostMulti(dwObservations, target);

            if (postSuccess)
            {
                SetVariable(variableNameLatestObsId, latestObsId);
            }

            if (page < pageLimit)
            {
                page++;
            }
            else
            {
                throw new Exception($"Page limit {pageLimit} reached, this means that either page limit is set for debugging, or value is too low for production.");
            }
        }

        Console.WriteLine("End");
        Console.WriteLine("------------------------------------------------");
    }

    static List<Dictionary<string, object>> LoadPrivateData(string path)
    {
        var dataTable = File.ReadLines(path)
                            .Skip(1)
                            .Select(line => line.Split('\t'))
                            .Select(parts => parts.ToDictionary(
                                key => key,
                                value => value as object))
                            .ToList();

        if (dataTable.Count > 0 && dataTable.Last().All(entry => entry.Value == null))
        {
            dataTable.RemoveAt(dataTable.Count - 1);
        }

        return dataTable;
    }

    static string ReduceMinutes(string datetimeStr, int minutesToReduce)
    {
        string formattedStr = datetimeStr.Replace("%3A", ":").Replace("%2B", "+").Replace("%2F", "/");
        DateTime datetimeObj = DateTime.Parse(formattedStr);
        DateTime newDatetimeObj = datetimeObj.AddMinutes(-minutesToReduce);
        return newDatetimeObj.ToString("yyyy-MM-ddTHH:mm:ss").Replace(":", "%3A").Replace("+", "%2B").Replace("/", "%2F");
    }

    static Dictionary<string, string> ReadVariables()
    {
        string filePath = "./store/data.json";

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        else
        {
            return new Dictionary<string, string>();
        }
    }

    static void SetVariable(string varName, string varValue)
    {
        string filePath = "./store/data.json";

        Dictionary<string, string> data;
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        else
        {
            data = new Dictionary<string, string>();
        }

        data[varName] = varValue;

        string updatedJson = JsonSerializer.Serialize(data);
        File.WriteAllText(filePath, updatedJson);
    }
}
