using NReco.Csv;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SOS.Lib.iNatFinland;
public class INatHelpers
{
    public static void AppendFact(List<Dictionary<string, object>> factsList, string factLabel, object factValue = null)
    {
        if (factValue != null && (factValue is bool && (bool)factValue || !(factValue is bool)))
        {
            factsList.Add(new Dictionary<string, object> { { "fact", factLabel }, { "value", factValue } });
        }
    }

    // Emailvalideringsfunktion
    public static bool IsValidEmail(string email)
    {
        string pattern = @"^[\w\.-]+@[\w\.-]+\.\w+$";
        return Regex.IsMatch(email, pattern);
    }

    // Kontrollera om en sträng är tom eller börjar med ett mellanslag
    public static bool IsValidString(string s)
    {
        return !string.IsNullOrEmpty(s) && !s.StartsWith(" ");
    }

    public static Dictionary<string, string> LoadPrivateEmails()
    {
        return null;
        //Console.WriteLine("Loading private emails");
        //var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        //{
        //    Delimiter = ",",
        //    Encoding = System.Text.Encoding.UTF8
        //};

        //Dictionary<string, string> privateUserEmails = new Dictionary<string, string>();

        //using (var reader = new StreamReader("./privatedata/inaturalist-suomi-20-users.csv"))
        //using (var csv = new CsvReader(reader, config))
        //{
        //    var records = csv.GetRecords<dynamic>().ToList();
        //    foreach (var record in records)
        //    {
        //        string login = Convert.ToString(record.login);
        //        string email = Convert.ToString(record.email);

        //        if (IsValidEmail(email) && IsValidString(login))
        //        {
        //            privateUserEmails[login] = email;
        //        }
        //    }
        //}

        //Console.WriteLine($"Loaded {privateUserEmails.Count} email addresses");
        //return privateUserEmails;
    }

    public static string ExtractAtlasCode(string text)
    {
        if (string.IsNullOrEmpty(text)) return null;

        string[] numbers = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        text = text.ToLower();

        int index = text.IndexOf("atl:");
        if (index == -1) return null;

        text = text.Replace("atl: ", "atl:");
        string atlasCode = text.Substring(index + 4, Math.Min(2, text.Length - (index + 4)));

        if (atlasCode.Length == 2 && !numbers.Contains(atlasCode[1].ToString()))
        {
            atlasCode = atlasCode.Substring(0, 1);
        }

        atlasCode = atlasCode.Trim('0');

        string[] allowedAtlasCodes = { "1", "2", "3", "4", "5", "6", "7", "8", "61", "62", "63", "64", "65", "66", "71", "72", "73", "74", "75", "81", "82" };

        Console.WriteLine("ATLASCODE: " + atlasCode);
        if (allowedAtlasCodes.Contains(atlasCode))
        {
            return atlasCode;
        }
        else
        {
            Console.WriteLine("Disallowed atlascode skipped: " + atlasCode);
            return null;
        }
    }

    public static void AppendRootFact(List<Dictionary<string, object>> factsList, Dictionary<string, object> inat, string factName)
    {
        if (inat.ContainsKey(factName) && inat[factName] != null && !string.IsNullOrEmpty(inat[factName].ToString()))
        {
            factsList.Add(new Dictionary<string, object> { { "fact", factName }, { "value", inat[factName] } });
        }
    }

    public static double DecimalFloor(double n, int decimals = 1)
    {
        double multiplier = Math.Pow(10, decimals);
        return Math.Floor(n * multiplier) / multiplier;
    }

    public static Dictionary<string, object> GetCoordinates(Dictionary<string, object> inat)
    {
        Dictionary<string, object> coord = new Dictionary<string, object>
        {
            { "type", "WGS84" }
        };

        bool obscured = inat.ContainsKey("obscured") && (bool)inat["obscured"];
        if (obscured)
        {
            double lonRaw = Convert.ToDouble(((JsonElement)inat["geojson"]).GetProperty("coordinates")[0]);
            int lonFirstDigit = int.Parse(lonRaw.ToString().Split('.')[1][0].ToString());

            if (lonFirstDigit % 2 == 0)
            {
                coord["lonMin"] = DecimalFloor(lonRaw);
                coord["lonMax"] = DecimalFloor(lonRaw + 0.2);
            }
            else
            {
                coord["lonMin"] = DecimalFloor(lonRaw - 0.1);
                coord["lonMax"] = DecimalFloor(lonRaw + 0.1);
            }

            double latRaw = Convert.ToDouble(((JsonElement)inat["geojson"]).GetProperty("coordinates")[1]);
            int latFirstDigit = int.Parse(latRaw.ToString().Split('.')[1][0].ToString());

            if (latFirstDigit % 2 == 0)
            {
                coord["latMin"] = DecimalFloor(latRaw);
                coord["latMax"] = DecimalFloor(latRaw + 0.2);
            }
            else
            {
                coord["latMin"] = DecimalFloor(latRaw - 0.1);
                coord["latMax"] = DecimalFloor(latRaw + 0.1);
            }
        }
        else
        {
            double lon = Math.Round(Convert.ToDouble(((JsonElement)inat["geojson"]).GetProperty("coordinates")[0]), 5);
            double lat = Math.Round(Convert.ToDouble(((JsonElement)inat["geojson"]).GetProperty("coordinates")[1]), 5);
            double accuracy = 100;

            if (inat.ContainsKey("positional_accuracy"))
            {
                double positionalAccuracy = Convert.ToDouble(inat["positional_accuracy"]);
                accuracy = positionalAccuracy < 10 ? 10 : Math.Round(positionalAccuracy, 0);
            }

            coord["accuracyInMeters"] = accuracy;
            coord["lonMin"] = lon;
            coord["lonMax"] = lon;
            coord["latMin"] = lat;
            coord["latMax"] = lat;
        }

        return coord;
    }

    public static string ConvertTaxon(string taxon)
    {
        Dictionary<string, string> convert = new Dictionary<string, string>
        {
            { "Life", "Biota" },
            { "unknown", "Biota" },
            { "Elämä", "Biota" },
            { "tuntematon", "Biota" },
            { "Taraxacum officinale", "Taraxacum" },
            { "Alchemilla vulgaris", "Alchemilla" },
            { "Pteridium aquilinum", "Pteridium pinetorum" },
            { "Ranunculus cassubicus", "Ranunculus cassubicus -ryhmä" },
            { "Ranunculus auricomus", "Ranunculus auricomus -ryhmä s. lat." },
            { "Bombus lucorum-complex", "Bombus lucorum coll." },
            { "Chrysoperla carnea-group", "Chrysoperla" },
            { "Potentilla argentea", "Potentilla argentea -ryhmä" },
            { "Chenopodium album", "Chenopodium album -ryhmä" },
            { "Imparidentia", "Heterodonta" },
            { "Canis familiaris", "Canis lupus familiaris" },
            { "Anguis", "Anguis colchica" },
            { "Monotropa", "Hypopitys" },
            { "Monotropa hypopitys", "Hypopitys monotropa" },
            { "Monotropa hypopitys ssp. hypophegea", "Hypopitys hypophegea" }
        };

        if (string.IsNullOrEmpty(taxon)) return "";
        return convert.ContainsKey(taxon) ? convert[taxon] : taxon;
    }

    public static (string, string) SummarizeAnnotation(Dictionary<string, int> annotation)
    {
        int key = annotation["controlled_attribute_id"];
        int value = annotation["controlled_value_id"];
        int voteScore = annotation["vote_score"];

        if (voteScore < 0) return ("against", "annotation_against");

        string annotationKey = "";
        string annotationValue = "";

        switch (value)
        {
            case 2:
                annotationKey = "lifeStage";
                annotationValue = "ADULT";
                break;
            case 4:
                annotationKey = "lifeStage";
                annotationValue = "PUPA";
                break;
            case 5:
                annotationKey = "lifeStage";
                annotationValue = "NYMPH";
                break;
            case 6:
                annotationKey = "lifeStage";
                annotationValue = "LARVA";
                break;
            case 7:
                annotationKey = "lifeStage";
                annotationValue = "EGG";
                break;
            case 8:
                annotationKey = "lifeStage";
                annotationValue = "JUVENILE";
                break;
            case 16:
                annotationKey = "lifeStage";
                annotationValue = "SUBIMAGO";
                break;
            case 10:
                annotationKey = "sex";
                annotationValue = "FEMALE";
                break;
            case 11:
                annotationKey = "sex";
                annotationValue = "MALE";
                break;
            case 18:
                annotationKey = "dead";
                annotationValue = "False";
                break;
            case 19:
                annotationKey = "dead";
                annotationValue = "True";
                break;
            case 13:
                annotationKey = "lifeStage";
                annotationValue = "FLOWER";
                break;
            case 14:
                annotationKey = "lifeStage";
                annotationValue = "RIPENING_FRUIT";
                break;
            case 15:
                annotationKey = "lifeStage";
                annotationValue = "BUD";
                break;
            default:
                // Return an empty tuple if no matching value was found
                return ("", "");
        }

        return (annotationKey, annotationValue);
    }

    public static string GetProxyUrl(string squareUrl, string imageSize)
    {
        return squareUrl.Replace("square", imageSize);
    }
}
