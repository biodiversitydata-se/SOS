namespace SOS.Status.Web.Client.Models;

public static class TaxonSearchFilterSamples
{
    public static readonly Dictionary<int, SearchFilterExample> Samples;

    static TaxonSearchFilterSamples()
    {
        Samples = new Dictionary<int, SearchFilterExample>
        {
            { 1, new SearchFilterExample() { Name = "Taxon", Json = _taxonSearchFilterSample } },
            { 2, new SearchFilterExample() { Name = "Polygon", Json = _polygonSearchFilterSample } },
            { 3, new SearchFilterExample() { Name = "Redlisted taxa", Json = _fieldsetRedlistedSearchFilterSample} },
            { 4, new SearchFilterExample() { Name = "Municipality", Json = _municipalitySearchFilterSample} },
            { 5, new SearchFilterExample() { Name = "Municipality - Amiral taxon", Json = _municipalityAmiralSample} },
            { 6, new SearchFilterExample() { Name = "Close locations", Json = _closeLocationsFilterSample} },
            { 7, new SearchFilterExample() { Name = "Locations far apart", Json = _locationsFarApartFilterSample} }
        };
    }

    private static string _taxonSearchFilterSample = """
{
    "date": {
        "startDate":"1990-01-01",
	    "endDate":"2025-12-31"
    },
    "taxon" : {
        "ids":[100077],
        "includeUnderlyingTaxa" : false
    },
    "occurrenceStatus":"present",
    "includeRealCount":true
}
""";

    private static string _closeLocationsFilterSample = """
{
    "geographics": {   
      "locationIds": [
        "urn:lsid:artportalen.se:site:2075304",
        "urn:lsid:artportalen.se:site:3978105",
        "urn:lsid:artportalen.se:site:10268619"
      ]
    },
    "includeRealCount":true
}
""";

    private static string _locationsFarApartFilterSample = """
{
    "geographics": {   
      "locationIds": [
        "urn:lsid:artportalen.se:site:2075304",
        "urn:lsid:artportalen.se:site:3978105",
        "urn:lsid:artportalen.se:site:10268619",
        "urn:lsid:mvm.se:site:3787"
      ]
    },
    "includeRealCount":true
}
""";


    private static string _municipalitySearchFilterSample = """
{    
    "geographics" : {     
       "areas" : [
           {
               "areaType": "Municipality",
               "featureId": "687"
           }
       ]
   },
   "includeRealCount":true
}
""";

    private static string _municipalityAmiralSample = """
{   
    "taxon" : {
        "ids":[201060],
        "includeUnderlyingTaxa" : true
    },
    "geographics" : {     
       "areas" : [
           {
               "areaType": "Municipality",
               "featureId": "687"
           }
       ]
   },
   "includeRealCount":true
}
""";    

    private static string _polygonSearchFilterSample = """
{
    "taxon": {
        "ids": [
            100077
        ]
    },
    "geographics": {
        "geometries": [
            {
                "type": "polygon",
                "coordinates": [
                    [
                        [15.07063, 57.92573],
                        [15.0051, 58.16108],
                        [14.58003, 58.10148],
                        [14.64143, 57.93294],
                        [15.07063, 57.92573]
                    ]
                ]
            }
        ],
        "considerObservationAccuracy": true
    },
    "includeRealCount":true
}
""";


    private static string _fieldsetRedlistedSearchFilterSample = """
{
    "date": {
        "startDate":"1990-01-01",
	    "endDate":"2025-12-31"
    },
    "taxon" : {
        "redListCategories": [ "CR", "EN", "VU" ]
    },
    "occurrenceStatus":"present",
    "includeRealCount":true
}
""";

    public class SearchFilterExample
    {
        public required string Name { get; set; }
        public required string Json { get; set; }
    }
}
