using Nest;
using NReco.Csv;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SOS.Lib.iNatFinland;
public class Simplify
{
    static void Main(string[] args)
    {
        Console.WriteLine("Loading datafile");

        string filePath = "../privatedata/inaturalist-suomi-20-observations.csv";
        var records = LoadCsvFile(filePath);
        Console.WriteLine("Datafile loaded");

        var filteredRecords = records
            .Where(r => r.CoordinatesObscured == true &&
                        (r.PlaceCountryName == "Finland" || r.PlaceCountryName == "Åland") &&
                        r.PrivateLatitude != null)
            .ToList();
        Console.WriteLine("Filtered");

        // Projecting to a new anonymous type with selected columns
        var selectedRecords = filteredRecords
            .Select(r => new
            {
                r.Id,
                r.ObservedOn,
                r.PositionalAccuracy,
                r.PrivatePlaceGuess,
                r.PrivateLatitude,
                r.PrivateLongitude
            })
            .ToList();
        Console.WriteLine("Columns selected");

        string outputFilePath = "../privatedata/latest.tsv";
        SaveAsTsv(selectedRecords, outputFilePath);
        Console.WriteLine($"All done, file saved as {outputFilePath}");
    }

    static List<Observation> LoadCsvFile(string path)
    {
        return null; 
        // todo - implement
        //using var reader = new StreamReader(path);
        //using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        //return csv.GetRecords<Observation>().ToList();
    }

    static void SaveAsTsv(IEnumerable<dynamic> records, string path)
    {
        // todo - implement
        //using var writer = new StreamWriter(path);
        //using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "\t" });
        //csv.WriteRecords(records);
    }
}

public class Observation
{
    public int Id { get; set; }
    public DateTime? ObservedOn { get; set; }
    public string PositionalAccuracy { get; set; }
    public string PrivatePlaceGuess { get; set; }
    [JsonPropertyName("private_latitude")]
    public double? PrivateLatitude { get; set; }
    [JsonPropertyName("private_longitude")]
    public double? PrivateLongitude { get; set; }
    [JsonPropertyName("coordinates_obscured")]
    public bool CoordinatesObscured { get; set; }
    [JsonPropertyName("place_country_name")]
    public string PlaceCountryName { get; set; }
}