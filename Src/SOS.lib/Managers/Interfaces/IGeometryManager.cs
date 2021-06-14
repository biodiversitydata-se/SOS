using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    /// Handle geometries
    /// </summary>
    public interface IGeometryManager
    {
        Task<Geometry> GetCircleAsync(Point wgs84Point, int? accuracy);
    }
}
