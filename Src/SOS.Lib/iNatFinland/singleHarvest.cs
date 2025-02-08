using Newtonsoft.Json.Linq;
using NReco.Csv;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.iNatFinland;
public class singleHarvest
{
 
    static async Task Main(string[] args)
    {
        // Input
        string id = args.Length > 0 ? args[0] : throw new ArgumentException("ID is required");
        string target = args.Length > 1 ? args[1] : throw new ArgumentException("Target is required (dry | dry-verbose | production)");

        // Load private data
        var privateObservationData = LoadPrivateData("./privatedata/latest.tsv");

        var privateEmails = INatHelpers.LoadPrivateEmails();

        // Get and transform data
        var singleObservationDict = await GetInat.GetSingle(int.Parse(id));

        Dictionary<string, object> res = inatToDw.ConvertObservations(new List<Dictionary<string, Dictionary<string, object>>>() { singleObservationDict }, null, privateEmails);
        object dwObservations = res["observations"];
        string latestObsId = (string)res["lastUpdateKey"];


        // Output
        if (target == "staging" || target == "production")
        {
            await postDw.PostSingle((dynamic)dwObservations, target);
        }

        //if (target == "dry-verbose")
        //{
        //    Console.WriteLine("INAT:");
        //    Console.WriteLine(JsonConvert.SerializeObject(singleObservationDict.Results, Formatting.Indented));
        //}

        //if (target == "dry-verbose" || target == "dry")
        //{
        //    Console.WriteLine("--------------------------------------------------------------");
        //    Console.WriteLine("dwObservation:");
        //    Console.WriteLine(JsonConvert.SerializeObject(dwObservation, Formatting.Indented));
        //}
    }

    static List<dynamic> LoadPrivateData(string filePath)
    {
        // todo
        //using var reader = new StreamReader(filePath);
        //using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        //var records = csv.GetRecords<dynamic>().ToList();
        //if (records.Last() == null)
        //{
        //    records.RemoveAt(records.Count - 1);
        //}
        //return records;
        return null;
    }
    
}
