using SOS.DataStewardship.Api.Models;
using SOS.Lib.Enums.VocabularyValues;

namespace SOS.DataStewardship.Api.Extensions
{
    public static class AreaExtensions
    {
        private static Dictionary<string, County> countyByFeatureId;
        private static Dictionary<County, string> featureIdByCounty;

        // todo - read mapping from JSON file?
        private static List<(County County, string FeatureId)> countyMappings = new List<(County county, string FeatureId)>()
        {
            new (County.BlekingeLän, CountyId.Blekinge),
            new (County.DalarnasLän, CountyId.Dalarna),
            new (County.GotlandsLän, CountyId.Gotland),
            new (County.GävleborgsLän, CountyId.Gävleborg),
            new (County.HallandsLän, CountyId.Halland),
            new (County.JämtlandsLän, CountyId.Jämtland),
            new (County.JönköpingsLän, CountyId.Jönköping),
            new (County.KalmarLän, CountyId.Kalmar),
            new (County.KronobergsLän, CountyId.Kronoberg),
            new (County.NorrbottensLän, CountyId.Norrbotten),
            new (County.SkåneLän, CountyId.Skåne),
            new (County.StockholmsLän, CountyId.Stockholm),
            new (County.SödermanlandsLän, CountyId.Södermanland),
            new (County.UppsalaLän, CountyId.Uppsala),
            new (County.VärmlandsLän, CountyId.Värmland),
            new (County.VästerbottensLän, CountyId.Västerbotten),
            new (County.VästernorrlandsLän, CountyId.Västernorrland),
            new (County.VästmanlandsLän, CountyId.Västmanland),
            new (County.VästraGötalandsLän, CountyId.VästraGötaland),
            new (County.ÖrebroLän, CountyId.Örebro),
            new (County.ÖstergötlandsLän, CountyId.Östergötland)
        };

        static AreaExtensions()
        {
            countyByFeatureId = countyMappings.ToDictionary(m => m.FeatureId, m => m.County);
            featureIdByCounty = countyMappings.ToDictionary(m => m.County, m => m.FeatureId);
        }

        public static County? GetCounty(this string countyId)
        {
            if (string.IsNullOrEmpty(countyId)) return null;
            if (countyByFeatureId.TryGetValue(countyId, out var county))
            {
                return county;
            }

            return null;

            //switch(countyId)
            //{
            //    case CountyId.Blekinge:
            //        return County.BlekingeLän;
            //    case CountyId.Dalarna:
            //        return County.DalarnasLän;
            //    case CountyId.Gotland:
            //        return County.GotlandsLän;
            //    case CountyId.Gävleborg:
            //        return County.GävleborgsLän;
            //    case CountyId.Halland:
            //        return County.HallandsLän;
            //    case CountyId.Jämtland:
            //        return County.JämtlandsLän;
            //    case CountyId.Jönköping:
            //        return County.JönköpingsLän;
            //    case CountyId.Kalmar:
            //        return County.KalmarLän;
            //    case CountyId.Kronoberg:
            //        return County.KronobergsLän;
            //    case CountyId.Norrbotten:
            //        return County.NorrbottensLän;
            //    case CountyId.Skåne:
            //        return County.SkåneLän;
            //    case CountyId.Stockholm:
            //        return County.StockholmsLän;
            //    case CountyId.Södermanland:
            //        return County.SödermanlandsLän;
            //    case CountyId.Uppsala:
            //        return County.UppsalaLän;
            //    case CountyId.Värmland:
            //        return County.VärmlandsLän;
            //    case CountyId.Västerbotten:
            //        return County.VästerbottensLän;
            //    case CountyId.Västernorrland:
            //        return County.VästernorrlandsLän;
            //    case CountyId.Västmanland:
            //        return County.VästmanlandsLän;
            //    case CountyId.VästraGötaland:
            //        return County.VästraGötalandsLän;
            //    case CountyId.Örebro:
            //        return County.ÖrebroLän;
            //    case CountyId.Östergötland:
            //        return County.ÖstergötlandsLän;
            //    default:
            //        return null;
            //}
        }

        // todo - should we create two dictionaries instead?
        // Dictionary<string, County>
        // Dictionary<County, string>
        public static string GetCountyFeatureId(this County county)
        {            
            if (featureIdByCounty.TryGetValue(county, out var featureId))
            {
                return featureId;
            }

            return null;

            //switch(county)
            //{
            //    case County.BlekingeLän:
            //        return CountyId.Blekinge;
            //    case County.DalarnasLän:
            //        return CountyId.Dalarna;
            //    case County.GotlandsLän:
            //        return CountyId.Gotland;
            //    case County.GävleborgsLän:
            //        return CountyId.Gävleborg;
            //    case County.HallandsLän:
            //        return CountyId.Halland;
            //    case County.JämtlandsLän:
            //        return CountyId.Jämtland;
            //    case County.JönköpingsLän:
            //        return CountyId.Jönköping;
            //    case County.KalmarLän:
            //        return CountyId.Kalmar;
            //    case County.KronobergsLän:
            //        return CountyId.Kronoberg;
            //    case County.NorrbottensLän:
            //        return CountyId.Norrbotten;
            //    case County.SkåneLän:
            //        return CountyId.Skåne;
            //    case County.StockholmsLän:
            //        return CountyId.Stockholm;
            //    case County.SödermanlandsLän:
            //        return CountyId.Södermanland;
            //    case County.UppsalaLän:
            //        return CountyId.Uppsala;
            //    case County.VärmlandsLän:
            //        return CountyId.Värmland;
            //    case County.VästerbottensLän:
            //        return CountyId.Västerbotten;
            //    case County.VästernorrlandsLän:
            //        return CountyId.Västernorrland;
            //    case County.VästmanlandsLän:
            //        return CountyId.Västmanland;
            //    case County.VästraGötalandsLän:
            //        return CountyId.VästraGötaland;
            //    case County.ÖrebroLän:
            //        return CountyId.Örebro;
            //    case County.ÖstergötlandsLän:
            //        return CountyId.Östergötland;
            //    default:
            //        return null;
            //}
        }
    }
}