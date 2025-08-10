namespace SOS.Status.Web.Client.Models.GeoJson;

public class GeoJsonPointAppearance
{
    public object[] data { get; set; } = new object[0];
    public string name { get; set; } = "";
    public PointSymbol? symbology { get; set; }
    public Tooltip? tooltip { get; set; }

    public static GeoJsonPointAppearance CreateTestData()
    {
        // Usage in leafletForBlazor:
        // var pointsAppearance = GeoJsonPointAppearance.CreateTestData();
        // await args.sender.Geometric.DataFromGeoJSON.addObject(pointsAppearance);

        List<GeoJsonItem> inputPointsList =
        [
            new GeoJsonItem()
            {
                type = "Feature",
                geometry = new PointGeometry()
                {
                    type = "Point",
                    coordinates = [43.96898521116147, 25.337392340780355],
                    properties = new Properties()
                    {
                        name = "name 1",
                        quantity = 5
                    }
                }
            },
            new GeoJsonItem()
            {
                type = "Feature",
                geometry = new PointGeometry()
                {
                    type = "Point",
                    coordinates = [43.97596818245641, 25.33369159513244],
                    properties = new Properties()
                    {
                        name = "name 2"
                    }
                }

            }
        ];

        return new GeoJsonPointAppearance()
        {
            data = inputPointsList.ToArray(),
            name = "points",
            symbology = new PointSymbol()
            {
                color = "red",
                radius = 10
            },
            tooltip = new Tooltip()
            {
                content = "<i>${name}</i><br/><font size='4' face='verdana' color='blue' >${quantity}</font>",
                offset = [2, 2],
                permanent = true,
                opacity = 0.6,
                visibilityZoomLevels = new VisibilityZoomLevel()
                {
                    maxZoomLevel = 16,
                    minZoomLevel = 14
                }
            }
        };
    }
}


public class Tooltip
{
    public string? content { get; set; }//js string template
    public int[]? offset { get; set; }
    public bool permanent { get; set; }
    public double opacity { get; set; }
    public bool coordinateInversion { get; set; }
    public VisibilityZoomLevel? visibilityZoomLevels { get; set; }

}

public class VisibilityZoomLevel
{
    public double minZoomLevel { get; set; } = 0;
    public double maxZoomLevel { get; set; } = 0;
}

public class GeoJsonPolygonAppearance
{
    public object[] data { get; set; } = new object[0];
    public string name { get; set; } = "";
    public PolygonSymbol? symbology { get; set; }
    public Tooltip? tooltip { get; set; }

    public static GeoJsonPolygonAppearance CreateTestData()
    {
        // Usage in leafletForBlazor:
        // var pointsAppearance = GeoJsonPointAppearance.CreateTestData();
        // await args.sender.Geometric.DataFromGeoJSON.addObject(polygonsAppearance);

        List<GeoJsonLineString> inputPolygonList =
        [
            new GeoJsonLineString()
            {
                type = "Feature",
                geometry = new LineStringGeometry()
                {
                    type = "Polygon",
                    coordinates =
                        [
                        new double[] { 43.97209871008421, 25.328761772135064 },
                        new double[] { 43.972004589576606, 25.329119019038004},
                        new double[] {43.97191506364906, 25.32894358371036 },
                        new double[] {43.97174519194365, 25.328889356767974},
                        new double[] {43.97186226638604, 25.328803236079175 },
                        new double[] {43.97183242466567, 25.32865650950876 },
                     ],
                    properties = new Properties()
                    {
                        name = "name"
                    }
                }
            },
        ];

        return new GeoJsonPolygonAppearance()
        {
            data = inputPolygonList.ToArray(),
            name = "Polygon",
            symbology = new PolygonSymbol()
            {
                color = "blue",
                opacity = 0.6,
                weight = 8
            },
            tooltip = new Tooltip()
            {
                content = "Polygon",
                offset = new int[2] { 2, 6 },
                permanent = true,
                opacity = 0.6,
                visibilityZoomLevels = new VisibilityZoomLevel()
                {
                    maxZoomLevel = 14,
                    minZoomLevel = 12
                }
            }
        };
    }
}

public class PointSymbol
{
    public int radius { get; set; } = 4;
    public string? fillColor { get; set; }
    public string? color { get; set; }
    public int weight { get; set; }
    public double opacity { get; set; }
    public double fillOpacity { get; set; } = 1;

}
public class PolygonSymbol
{
    public string? color { get; set; }
    public double opacity { get; set; }
    public int weight { get; set; }
}

public class GeoJsonItem
{
    public string? type { get; set; }
    public PointGeometry? geometry { get; set; }
}

public class GeoJsonLineString
{
    public string? type { get; set; }
    public LineStringGeometry? geometry { get; set; }
}

public class PointGeometry
{
    public string? type { get; set; } = "Point";
    public double[]? coordinates { get; set; }
    public Properties? properties { get; set; }
}


public class LineStringGeometry
{
    public string? type { get; set; } = "LineString";
    public double[][]? coordinates { get; set; }
    public Properties? properties { get; set; }
}

public class Properties
{
    public string? name { get; set; }
    public int? quantity { get; set; }
}