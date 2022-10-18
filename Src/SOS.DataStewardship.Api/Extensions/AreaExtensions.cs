using SOS.DataStewardship.Api.Models;
using SOS.Lib.Enums.VocabularyValues;

namespace SOS.DataStewardship.Api.Extensions
{
    public static class AreaExtensions
    {
        public static County? GetCounty(this string countyId)
        {
            if (string.IsNullOrEmpty(countyId)) return null;

            switch(countyId)
            {
                case CountyId.Blekinge:
                    return County.BlekingeLän;
                case CountyId.Dalarna:
                    return County.DalarnasLän;
                case CountyId.Gotland:
                    return County.GotlandsLän;
                case CountyId.Gävleborg:
                    return County.GävleborgsLän;
                case CountyId.Halland:
                    return County.HallandsLän;
                case CountyId.Jämtland:
                    return County.JämtlandsLän;
                case CountyId.Jönköping:
                    return County.JönköpingsLän;
                case CountyId.Kalmar:
                    return County.KalmarLän;
                case CountyId.Kronoberg:
                    return County.KronobergsLän;
                case CountyId.Norrbotten:
                    return County.NorrbottensLän;
                case CountyId.Skåne:
                    return County.SkåneLän;
                case CountyId.Stockholm:
                    return County.StockholmsLän;
                case CountyId.Södermanland:
                    return County.SödermanlandsLän;
                case CountyId.Uppsala:
                    return County.UppsalaLän;
                case CountyId.Värmland:
                    return County.VärmlandsLän;
                case CountyId.Västerbotten:
                    return County.VästerbottensLän;
                case CountyId.Västernorrland:
                    return County.VästernorrlandsLän;
                case CountyId.Västmanland:
                    return County.VästmanlandsLän;
                case CountyId.VästraGötaland:
                    return County.VästraGötalandsLän;
                case CountyId.Örebro:
                    return County.ÖrebroLän;
                case CountyId.Östergötland:
                    return County.ÖstergötlandsLän;
                default:
                    return null;
            }
        }
    }
}