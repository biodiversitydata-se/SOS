using SOS.Lib.Enums;

namespace SOS.Lib.Models.UserService
{
    public class UserArea
    {
        public int? Buffer { get; set; }
        public AreaType AreaType { get; set; }
        public string FeatureId { get; set; }
        public string Name { get; set; }
    }
}