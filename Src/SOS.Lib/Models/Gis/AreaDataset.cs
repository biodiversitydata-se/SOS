using System;

namespace SOS.Lib.Models.Gis
{
    public class AreaDataset
    {
        public int Id { get; set; }
        public int ArtportalenId { get; set; }
        public int LifeWatchId { get; set; }
        public string Name { get; set; }
        public AreaDatasetCategory AreaDatasetCategory { get; set; }
        public Boolean IsValidationArea { get; set; }
        public int MaxProtectionLevel { get; set; }
    }
}