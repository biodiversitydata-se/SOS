using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Extensions
{
    public static class ObservationExtensions
    {
        public static bool ShallBeProtected(this Observation observation)
        {
            return observation.Occurrence.SensitivityCategory > 2 || observation.AccessRights?.Id == (int)AccessRightsId.NotForPublicUsage;
        }
    }
}