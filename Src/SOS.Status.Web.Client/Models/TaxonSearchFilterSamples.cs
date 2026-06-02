namespace SOS.Status.Web.Client.Models;

public static class TaxonSearchFilterSamples
{
    public static readonly Dictionary<int, SearchFilterExample> Samples;

    static TaxonSearchFilterSamples()
    {
        Samples = new Dictionary<int, SearchFilterExample>
        {
            { 1, new SearchFilterExample() { Name = "Search with taxon", Json = _taxonSearchFilterSample } },
            { 2, new SearchFilterExample() { Name = "Search with polygon", Json = _polygonSearchFilterSample } },
            { 3, new SearchFilterExample() { Name = "Search with redlisted taxa", Json = _fieldsetRedlistedSearchFilterSample} },
            { 4, new SearchFilterExample() { Name = "Search in municipality", Json = _municipalitySearchFilterSample} },
            { 5, new SearchFilterExample() { Name = "Search in municipality Amiral", Json = _municipalityAmiralSample} }
        };
    }

    private static string _taxonSearchFilterSample = """"
{
    "date": {
        "startDate":"1990-01-01",
	    "endDate":"2025-12-31"
    },
    "taxon" : {
        "ids":[100077],
        "includeUnderlyingTaxa" : false
    },
    "occurrenceStatus":"present"
}
"""";

    private static string _municipalitySearchFilterSample = """"
{    
    "geographics" : {     
       "areas" : [
           {
               "areaType": "Municipality",
               "featureId": "687"
           }
       ]
   }
}
"""";

    private static string _municipalityAmiralSample = """"
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
   }
}
"""";    

    private static string _polygonSearchFilterSample = """"
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
    }
}
"""";


    private static string _fieldsetRedlistedSearchFilterSample = """"
{
    "date": {
        "startDate":"1990-01-01",
	    "endDate":"2025-12-31"
    },
    "taxon" : {
        "redListCategories": [ "CR", "EN", "VU" ]
    },
    "occurrenceStatus":"present"    
}
"""";

    public class SearchFilterExample
    {
        public required string Name { get; set; }
        public required string Json { get; set; }
    }
}