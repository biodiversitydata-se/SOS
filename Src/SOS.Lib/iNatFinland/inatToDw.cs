using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.iNatFinland;
public class inatToDw
{
    
    // Skips observations that don't have enough information to be included
    public static bool SkipObservation(dynamic inat)
    {
        if (inat.taxon == null)
        {
            Console.WriteLine(" skipping " + inat.id + " without taxon.");
            return true;
        }
        else if (inat.observed_on_details == null)
        {
            Console.WriteLine(" skipping " + inat.id + " without date.");
            return true;
        }
        else
        {
            return false;
        }
    }

    // Adds a keyword to the keyword list if it exists and is truthy
    public static List<string> AppendKeyword(List<string> keywordList, dynamic inat, string keywordName)
    {
        if (inat.ContainsKey(keywordName) && inat[keywordName] != null)
        {
            keywordList.Add(keywordName);
        }
        return keywordList;
    }

    // Adds collection project facts and keywords
    public static (List<Dictionary<string, object>>, List<string>) AppendCollectionProjects(List<Dictionary<string, object>> factsList, List<string> keywords, dynamic projects)
    {
        foreach (var project in projects)
        {
            keywords.Add("project-" + project.project_id.ToString());
            factsList.Add(new Dictionary<string, object> { { "fact", "collectionProjectId" }, { "value", project.project_id } });
        }
        return (factsList, keywords);
    }

    // Adds traditional project facts and keywords
    public static (List<Dictionary<string, object>>, List<string>) AppendTraditionalProjects(List<Dictionary<string, object>> factsList, List<string> keywords, dynamic projects)
    {
        foreach (var project in projects)
        {
            keywords.Add("project-" + project.project.id.ToString());
            factsList.Add(new Dictionary<string, object> { { "fact", "traditionalProjectId" }, { "value", project.project.id } });
        }
        return (factsList, keywords);
    }

    // Adds tags to the keyword list
    public static List<string> AppendTags(List<string> keywordsList, dynamic tags)
    {
        foreach (var tag in tags)
        {
            keywordsList.Add(tag.ToString());
        }
        return keywordsList;
    }

    // Summarizes the quality metrics
    public static Dictionary<string, int> SummarizeQualityMetrics(dynamic qualityMetrics)
    {
        var summary = new Dictionary<string, int>();

        foreach (var vote in qualityMetrics)
        {
            if (vote.ContainsKey("user"))
            {
                if (vote.user.spam || vote.user.suspended)
                {
                    continue;
                }
            }

            int value = vote.agree == true ? 1 : vote.agree == false ? -1 : 0;

            if (!summary.ContainsKey(vote.metric))
            {
                summary[vote.metric] = 0;
            }

            summary[vote.metric] += value;
        }

        return summary;
    }

    // Gets the license URL based on the license code
    public static string GetLicenseUrl(string licenseCode)
    {
        var licenses = new Dictionary<string, string>
        {
            { "cc0", "http://tun.fi/MZ.intellectualRightsCC0-4.0" },
            { "cc-by", "http://tun.fi/MZ.intellectualRightsCC-BY-4.0" },
            { "cc-by-nc", "http://tun.fi/MZ.intellectualRightsCC-BY-NC-4.0" },
            { "cc-by-nd", "http://tun.fi/MZ.intellectualRightsCC-BY-ND-4.0" },
            { "cc-by-sa", "http://tun.fi/MZ.intellectualRightsCC-BY-SA-4.0" },
            { "cc-by-nc-nd", "http://tun.fi/MZ.intellectualRightsCC-BY-NC-ND-4.0" },
            { "cc-by-nc-sa", "http://tun.fi/MZ.intellectualRightsCC-BY-NC-SA-4.0" }
        };

        if (string.IsNullOrEmpty(licenseCode))
        {
            return "http://tun.fi/MZ.intellectualRightsARR";
        }
        else if (licenses.ContainsKey(licenseCode))
        {
            return licenses[licenseCode];
        }
        else
        {
            Console.WriteLine("Unknown license code " + licenseCode);
            return "http://tun.fi/MZ.intellectualRightsARR";
        }
    }

    // Gets image data for a photo
    public static Dictionary<string, string> GetImageData(dynamic photo, string observer)
    {
        var squareUrl = photo.photo.url;

        var thumbnailUrl = INatHelpers.GetProxyUrl(squareUrl, "small");
        var fullUrl = INatHelpers.GetProxyUrl(squareUrl, "original");

        return new Dictionary<string, string>
        {
            { "thumbnailURL", thumbnailUrl },
            { "fullURL", fullUrl },
            { "copyrightOwner", observer },
            { "author", observer },
            { "licenseId", GetLicenseUrl(photo.photo.license_code) },
            { "mediaType", "IMAGE" }
        };
    }

    // Checks if a value is not NaN
    public static bool HasValue(dynamic val)
    {
        return val == val;
    }    

    private static Dictionary<string, object> GetImageData(Dictionary<string, Dictionary<string, object>> photo, string observer)
    {
        var squareUrl = photo["photo"]["url"].ToString();

        var thumbnailUrl = INatHelpers.GetProxyUrl(squareUrl, "small");
        var fullUrl = INatHelpers.GetProxyUrl(squareUrl, "original");

        return new Dictionary<string, object>
        {
            { "thumbnailURL", thumbnailUrl },
            { "fullURL", fullUrl },
            { "copyrightOwner", observer },
            { "author", observer },
            { "licenseId", GetLicenseUrl(photo["photo"]["license_code"].ToString()) },
            { "mediaType", "IMAGE" }
        };
    }

    private static void AppendKeyword(List<string> keywordList, Dictionary<string, object> inat, string keywordName)
    {
        if (inat.ContainsKey(keywordName) && inat[keywordName] != null)
        {
            keywordList.Add(keywordName);
        }
    }

    private static void AppendCollectionProjects(List<Dictionary<string, object>> factsList, List<string> keywords, List<Dictionary<string, object>> projects)
    {
        foreach (var project in projects)
        {
            keywords.Add("project-" + project["project_id"]);
            factsList.Add(new Dictionary<string, object> { { "fact", "collectionProjectId" }, { "value", project["project_id"] } });
        }
    }

    private static void AppendTraditionalProjects(List<Dictionary<string, object>> factsList, List<string> keywords, List<Dictionary<string, Dictionary<string, object>>> projects)
    {
        foreach (var project in projects)
        {
            keywords.Add("project-" + project["project"]["id"]);
            factsList.Add(new Dictionary<string, object> { { "fact", "traditionalProjectId" }, { "value", project["project"]["id"] } });
        }
    }

    private static void AppendTags(List<string> keywordsList, List<string> tags)
    {
        keywordsList.AddRange(tags);
    }

    private static Dictionary<string, int> SummarizeQualityMetrics(List<Dictionary<string, object>> quality_metrics)
    {
        var summary = new Dictionary<string, int>();

        foreach (var vote in quality_metrics)
        {
            // Skip spam and suspended user votes
            if (vote.ContainsKey("user") && vote["user"] != null)
            {
                var user = vote["user"] as Dictionary<string, object>;
                if (user["spam"].ToString().Equals("True", StringComparison.OrdinalIgnoreCase) || user["suspended"].ToString().Equals("True", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            int value = vote["agree"].ToString().Equals("True", StringComparison.OrdinalIgnoreCase) ? 1 : -1;

            if (!summary.ContainsKey(vote["metric"].ToString()))
            {
                summary[vote["metric"].ToString()] = 0;
            }

            summary[vote["metric"].ToString()] += value;
        }

        return summary;
    }

    public static Dictionary<string, object> ConvertObservations(
            List<Dictionary<string, Dictionary<string, object>>> inatObservations,
            JObject privateObservationData,
            Dictionary<string, string> privateEmails)
    {
        var dwObservations = new List<Dictionary<string, object>>();
        int lastUpdateKey = 0;

        foreach (var inat in inatObservations)
        {
            //var privateData = privateObservationData.SelectToken($"$.observations[?(@.id == {inat["id"]})]")?.ToObject<Dictionary<string, object>>();
            var logSuffix = "";
           // bool hasPrivateData = false;
            //if (privateData != null)
            //{
            //    hasPrivateData = true;
            //    logSuffix += " has private data";
            //}

            bool hasPrivateEmail = privateEmails.ContainsKey(inat["user"]["login"].ToString());
            if (hasPrivateEmail)
            {
                var privateEmail = privateEmails[inat["user"]["login"].ToString()];
                logSuffix += " has private email";
            }

            Console.WriteLine($"Converting obs {inat["id"]}{logSuffix}");

            if (SkipObservation(inat))
            {
                continue;
            }

            var dw = new Dictionary<string, object>();
            var publicDocument = new Dictionary<string, object>();
            var gathering = new Dictionary<string, object>();
            var unit = new Dictionary<string, object>();

            var desc = new Dictionary<string, object>();
            var keywords = new List<string>();
            var documentFacts = new List<Dictionary<string, object>>();
            var gatheringFacts = new List<Dictionary<string, object>>();
            var unitFacts = new List<Dictionary<string, object>>();

            gathering["notes"] = "";
            unit["sourceTags"] = new List<string>();
            unit["quality"] = new Dictionary<string, object> { { "issue", new Dictionary<string, object>() } };

            string collectionId = "http://tun.fi/HR.3211";
            dw["collectionId"] = collectionId;
            publicDocument["collectionId"] = collectionId;

            dw["sourceId"] = "http://tun.fi/KE.901";
            dw["schema"] = "laji-etl";
            publicDocument["secureLevel"] = "NONE";
            publicDocument["concealment"] = "PUBLIC";
            publicDocument["referenceURL"] = inat["uri"];

            publicDocument["secureReasons"] = new List<string>();
            if (!("open".Equals(inat["taxon_geoprivacy"]) || inat["taxon_geoprivacy"] == null))
            {
                ((List<string>)publicDocument["secureReasons"]).Add("DEFAULT_TAXON_CONSERVATION");
            }
            if (!("open".Equals(inat["geoprivacy"]) || inat["geoprivacy"] == null))
            {
                ((List<string>)publicDocument["secureReasons"]).Add("USER_HIDDEN_LOCATION");
            }

            unit["taxonVerbatim"] = INatHelpers.ConvertTaxon(inat["taxon"]["name"].ToString());
            unitFacts.Add(new Dictionary<string, object> { { "fact", "species_guess" }, { "value", inat["species_guess"] } });
            unitFacts.Add(new Dictionary<string, object> { { "fact", "taxonInterpretationByiNaturalist" }, { "value", inat["taxon"]["name"] } });

            string documentId = $"{dw["collectionId"]}/{inat["id"]}";
            dw["documentId"] = documentId;
            publicDocument["documentId"] = documentId;
            gathering["gatheringId"] = $"{documentId}-G";
            unit["unitId"] = $"{documentId}-U";

            documentFacts.Add(new Dictionary<string, object> { { "fact", "catalogueNumber" }, { "value", inat["id"] } });
            keywords.Add(inat["id"].ToString());

            var agreeingIdentifiers = new List<string>();
            // todo - implement.
            //foreach (var identification in (JArray)inat["identifications"])
            //{
            //    if (identification["taxon"]["name"].ToString() == unit["taxonVerbatim"].ToString())
            //    {
            //        var detname = identification["user"]["login"].ToString();
            //        if (identification["user"]["name"] != null)
            //        {
            //            detname = identification["user"]["name"].ToString();
            //        }
            //        agreeingIdentifiers.Add(detname);
            //    }
            //}
            unit["det"] = string.Join(", ", agreeingIdentifiers);

            string observer = inat["user"]["name"] != null ? inat["user"]["name"].ToString() : inat["user"]["login"].ToString();
            gathering["team"] = new List<string> { observer };

            string userId = $"inaturalist:{inat["user"]["id"]}";
            publicDocument["editorUserIds"] = new List<string> { userId };
            gathering["observerUserIds"] = new List<string> { userId };

            if (inat["user"]["orcid"] != null)
            {
                documentFacts.Add(new Dictionary<string, object> { { "fact", "observerOrcid" }, { "value", inat["user"]["orcid"] } });
            }

            if (inat["description"] != null)
            {
                gathering["notes"] = gathering["notes"].ToString() + inat["description"];
            }

            if (Convert.ToInt32(inat["captive"]) == 1)
            {
                unit["wild"] = false;
            }

            // Projects
            // todo - implement.
            //if (inat.ContainsKey("non_traditional_projects"))
            //{
            //    (documentFacts, keywords) = AppendCollectionProjects(documentFacts, keywords, (JArray)inat["non_traditional_projects"]);
            //}

            //if (inat.ContainsKey("project_observations"))
            //{
            //    (documentFacts, keywords) = AppendTraditionalProjects(documentFacts, keywords, (JArray)inat["project_observations"]);
            //}

            publicDocument["createdDate"] = inat["created_at_details"]["date"];
            publicDocument["modifiedDate"] = inat["updated_at"].ToString().Split("T")[0];

            gathering["eventDate"] = new Dictionary<string, object>
            {
                { "begin", inat["observed_on_details"]["date"] },
                { "end", inat["observed_on_details"]["date"] }
            };

            documentFacts.Add(new Dictionary<string, object> { { "fact", "observedOrCreatedAt" }, { "value", inat["time_observed_at"] } });
            gathering["locality"] = inat["place_guess"];
            gathering["country"] = "";

            // todo - implement.
            //if (inat.ContainsKey("tags"))
            //{
            //    keywords = AppendTags(keywords, (JArray)inat["tags"]);
            //}

            // todo - implement.
            //int photoCount = ((JArray)inat["observation_photos"]).Count;
            //if (photoCount >= 1)
            //{
            //    unit["media"] = new List<object>();
            //    keywords.Add("has_images");
            //    unitFacts.Add(new Dictionary<string, object> { { "fact", "imageCount" }, { "value", photoCount } });
            //    unit["externalMediaCount"] = photoCount;

            //    bool arrImagesIncluded = false;

            //    if (photoCount > 4)
            //    {
            //        keywords.Add("over_4_photos");
            //    }


            //    foreach (var photo in (JArray)inat["observation_photos"])
            //    {
            //        string photoId = photo["photo"]["id"].ToString();
            //        unitFacts.Add(new Dictionary<string, object> { { "fact", "imageId" }, { "value", photoId } });
            //        unitFacts.Add(new Dictionary<string, object> { { "fact", "imageUrl" }, { "value", $"https://www.inaturalist.org/photos/{photoId}" } });

            //        if (photo["photo"]["license_code"] != null)
            //        {
            //            ((List<object>)unit["media"]).Add(GetImageData(photo, observer));
            //        }
            //        else
            //        {
            //            arrImagesIncluded = true;
            //        }
            //    }

            //    if (arrImagesIncluded)
            //    {
            //        keywords.Add("image_arr");
            //    }
            //}

            // todo - implement.
            //int soundCount = ((JArray)inat["sounds"]).Count;
            //if (soundCount >= 1)
            //{
            //    keywords.Add("has_audio");
            //    unitFacts.Add(new Dictionary<string, object> { { "fact", "soundCount" }, { "value", soundCount } });

            //    if (soundCount > 1)
            //    {
            //        keywords.Add("over_1_sounds");
            //    }

            //    foreach (var sound in (JArray)inat["sounds"])
            //    {
            //        unitFacts.Add(new Dictionary<string, object> { { "fact", "audioId" }, { "value", sound["id"] } });
            //        unitFacts.Add(new Dictionary<string, object> { { "fact", "audioUrl" }, { "value", sound["file_url"] } });
            //    }
            //}

            //if (soundCount == 0 && photoCount == 0)
            //{
            //    keywords.Add("no_media");
            //}

            bool hasSpecimen = false;
            string abundanceString = "";
            string atlasCode = null;

            // todo - implement.
            //foreach (var val in (JArray)inat["ofvs"])
            //{
            //    unitFacts.Add(new Dictionary<string, object> { { "fact", val["name_ci"] }, { "value", val["value_ci"] } });

            //    switch (val["name_ci"].ToString())
            //    {
            //        case "Specimen":
            //            hasSpecimen = true;
            //            break;
            //        case "Yksilömäärä":
            //            abundanceString = val["value_ci"].ToString();
            //            break;
            //        case "Atlas Code":
            //            atlasCode = val["value_ci"].ToString();
            //            break;
            //        default:
            //            break;
            //    }
            //}

            unit["abundanceString"] = abundanceString;
            unitFacts.Add(new Dictionary<string, object> { { "fact", "hasSpecimen" }, { "value", hasSpecimen.ToString().ToLower() } });

            if (atlasCode != null)
            {
                unit["atlasCode"] = atlasCode;
            }

            publicDocument["keywords"] = keywords;
            publicDocument["facts"] = documentFacts;
            gathering["facts"] = gatheringFacts;
            publicDocument["gatherings"] = new List<Dictionary<string, object>> { gathering };
            unit["facts"] = unitFacts;
            gathering["units"] = new List<Dictionary<string, object>> { unit };
            dwObservations.Add(dw);
            lastUpdateKey = Convert.ToInt32(inat["id"]);
        }

        return new Dictionary<string, object>
        {
            { "lastUpdateKey", lastUpdateKey },
            { "observations", dwObservations }
        };
    }
}