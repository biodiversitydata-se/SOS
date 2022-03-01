using SOS.Lib.Enums;
using SOS.Lib.Extensions;

namespace SOS.Harvest.Managers
{
    public class CoordinateDiffusionManager
    {
        private readonly Random _random = new Random();

        public Dictionary<int, DiffusedProtectionLevelStats> CalculateCoordinateDiffusionStats(CoordinateSys diffusionCoordinateSys = CoordinateSys.WebMercator)
        {
            const int nrCoordinates = 100000;
            var diffusedCoordinateInfoByProtectionLevel = new Dictionary<int, List<DiffusedCoordinateInfo>>();
            var protectionLevelStatsByProtectionLevel = new Dictionary<int, DiffusedProtectionLevelStats>();

            for (int protectionLevel = 2; protectionLevel <= 5; protectionLevel++)
            {
                diffusedCoordinateInfoByProtectionLevel.Add(protectionLevel, new List<DiffusedCoordinateInfo>());
                for (int i = 0; i < nrCoordinates; i++)
                {
                    var coordinate = CreateCoordinateInSweden();
                    var diffusedCoordinateInfo = CreateDiffusedCoordinateInfo(protectionLevel, coordinate, diffusionCoordinateSys);
                    diffusedCoordinateInfoByProtectionLevel[protectionLevel].Add(diffusedCoordinateInfo);
                }

                var protectionLevelStats = new DiffusedProtectionLevelStats();
                protectionLevelStats.ProtectionLevel = protectionLevel;
                protectionLevelStats.MinDistance = Convert.ToInt32(diffusedCoordinateInfoByProtectionLevel[protectionLevel].Min(diffusedCoordinateInfo => diffusedCoordinateInfo.DistanceBetweenOriginalAndDiffusedSweref99Tm));
                protectionLevelStats.MaxDistance = Convert.ToInt32(diffusedCoordinateInfoByProtectionLevel[protectionLevel].Max(diffusedCoordinateInfo => diffusedCoordinateInfo.DistanceBetweenOriginalAndDiffusedSweref99Tm));
                protectionLevelStats.AvgDistance = Convert.ToInt32(diffusedCoordinateInfoByProtectionLevel[protectionLevel].Average(diffusedCoordinateInfo => diffusedCoordinateInfo.DistanceBetweenOriginalAndDiffusedSweref99Tm));
                protectionLevelStats.NrOriginalDistinctCoordinates = diffusedCoordinateInfoByProtectionLevel[protectionLevel]
                    .Select(m => m.OriginalPointSweref99TmString).Distinct().Count();
                protectionLevelStats.NrDiffusedDistinctCoordinates = diffusedCoordinateInfoByProtectionLevel[protectionLevel]
                    .Select(m => m.DiffusedPointSweref99TmString).Distinct().Count();
                protectionLevelStatsByProtectionLevel.Add(protectionLevel, protectionLevelStats);
            }

            return protectionLevelStatsByProtectionLevel;
        }

        private DiffusedCoordinateInfo CreateDiffusedCoordinateInfo(
            int protectionLevel, 
            (double lon, double lat) wgs84Coordinate, 
            CoordinateSys diffusionCoordinateSys = CoordinateSys.WebMercator)
        {
            var (mod, add) = GetDiffusionValues(protectionLevel);

            //transform the point into the same format as Artportalen so that we can use the same diffusion as them
            NetTopologySuite.Geometries.Point originalPointWgs84 = new NetTopologySuite.Geometries.Point(wgs84Coordinate.lon, wgs84Coordinate.lat);
            NetTopologySuite.Geometries.Point originalPointWebMercator = (NetTopologySuite.Geometries.Point)originalPointWgs84.Transform(CoordinateSys.WGS84, CoordinateSys.WebMercator);
            NetTopologySuite.Geometries.Point originalPointSweref99Tm = (NetTopologySuite.Geometries.Point)originalPointWgs84.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM);
            NetTopologySuite.Geometries.Point diffusedPointWebMercator = null;
            NetTopologySuite.Geometries.Point diffusedPointWgs84 = null;
            NetTopologySuite.Geometries.Point diffusedPointSweref99Tm = null;

            if (diffusionCoordinateSys == CoordinateSys.WebMercator)
            {
                diffusedPointWebMercator = new NetTopologySuite.Geometries.Point(
                    originalPointWebMercator.Coordinates[0].X - originalPointWebMercator.Coordinates[0].X % mod +
                    add,
                    originalPointWebMercator.Coordinates[0].Y - originalPointWebMercator.Coordinates[0].Y % mod +
                    add);

                diffusedPointWgs84 =
                    (NetTopologySuite.Geometries.Point) diffusedPointWebMercator.Transform(CoordinateSys.WebMercator,
                        CoordinateSys.WGS84);

                diffusedPointSweref99Tm =
                    (NetTopologySuite.Geometries.Point) diffusedPointWebMercator.Transform(CoordinateSys.WebMercator,
                        CoordinateSys.SWEREF99_TM);
            }
            else if (diffusionCoordinateSys == CoordinateSys.SWEREF99_TM)
            {
                diffusedPointSweref99Tm = new NetTopologySuite.Geometries.Point(
                    originalPointSweref99Tm.Coordinates[0].X - originalPointSweref99Tm.Coordinates[0].X % mod + (mod/2.0),
                    originalPointSweref99Tm.Coordinates[0].Y - originalPointSweref99Tm.Coordinates[0].Y % mod + (mod/2.0));

                diffusedPointWgs84 =
                    (NetTopologySuite.Geometries.Point)diffusedPointSweref99Tm.Transform(CoordinateSys.SWEREF99_TM,
                        CoordinateSys.WGS84);

                diffusedPointWebMercator =
                    (NetTopologySuite.Geometries.Point)diffusedPointSweref99Tm.Transform(CoordinateSys.SWEREF99_TM,
                        CoordinateSys.WebMercator);
            }

            var diffusedCoordinateInfo = new DiffusedCoordinateInfo
            {
                OriginalPointWgs84 = originalPointWgs84,
                OriginalPointWebMercator = originalPointWebMercator,
                OriginalPointSweref99Tm = originalPointSweref99Tm,
                DiffusedPointWgs84 = diffusedPointWgs84,
                DiffusedPointWebMercator = diffusedPointWebMercator,
                DiffusedPointSweref99Tm = diffusedPointSweref99Tm
            };

            return diffusedCoordinateInfo;
        }

        private (double lon, double lat) CreateCoordinateInSweden()
        {
            double lon;
            double lat = GetRandomNumber(56, 57);
            if (lat < 63)
            {
                lon = GetRandomNumber(12, 19);
            }
            else
            {
                lon = GetRandomNumber(15, 20);
            }

            return (lon, lat);
        }

        private double GetRandomNumber(double minimum, double maximum)
        {
            return _random.NextDouble() * (maximum - minimum) + minimum;
        }

        /// <summary>
        /// Get level of diffusion based on protection level
        /// </summary>
        /// <param name="protectionLevel"></param>
        /// <returns></returns>
        private (int mod, int add) GetDiffusionValues(int protectionLevel)
        {
            return protectionLevel switch
            {
                2 => (1000, 555),
                3 => (5000, 2505),
                4 => (25000, 12505),
                5 => (50000, 25005),
                _ => (1, 0)
            };
        }

        public class DiffusedCoordinateInfo
        {
            public NetTopologySuite.Geometries.Point OriginalPointWgs84 { get; set; }
            public NetTopologySuite.Geometries.Point OriginalPointSweref99Tm { get; set; }
            public NetTopologySuite.Geometries.Point OriginalPointWebMercator { get; set; }
            public NetTopologySuite.Geometries.Point DiffusedPointWgs84 { get; set; }
            public NetTopologySuite.Geometries.Point DiffusedPointSweref99Tm { get; set; }
            public NetTopologySuite.Geometries.Point DiffusedPointWebMercator { get; set; }
            public string OriginalPointSweref99TmString =>
                $"X={Convert.ToInt32(OriginalPointSweref99Tm.X)},Y={Convert.ToInt32(OriginalPointSweref99Tm.Y)}";
            public string DiffusedPointSweref99TmString =>
                $"X={Convert.ToInt32(DiffusedPointSweref99Tm.X)},Y={Convert.ToInt32(DiffusedPointSweref99Tm.Y)}";

            public double DistanceBetweenOriginalAndDiffusedSweref99Tm => OriginalPointSweref99Tm.Distance(DiffusedPointSweref99Tm);
            public double DistanceBetweenOriginalAndDiffusedWebMercator => OriginalPointWebMercator.Distance(DiffusedPointWebMercator);
        }
        public class DiffusedProtectionLevelStats
        {
            public int ProtectionLevel { get; set; }
            public int MinDistance { get; set; }
            public int MaxDistance { get; set; }
            public int AvgDistance { get; set; }
            public int NrOriginalDistinctCoordinates { get; set; }
            public int NrDiffusedDistinctCoordinates { get; set; }
        }
    }
}