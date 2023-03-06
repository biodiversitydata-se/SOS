namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
    /// Custom class for geometries. Decide if we should use this or IGeoShape.
    /// </summary>
    public class GeometryObject
    {
        public string Type { get; set; }
        public dynamic Coordinates { get; set; }
    }
}
