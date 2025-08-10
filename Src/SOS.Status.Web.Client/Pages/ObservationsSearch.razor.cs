using LeafletForBlazor;
using SOS.Status.Web.Client.Dtos.SosObsApi;
using SOS.Status.Web.Client.JsonConverters;
using SOS.Status.Web.Client.Models;
using System.Text.Json.Serialization;

namespace SOS.Status.Web.Client.Pages;

public partial class ObservationsSearch
{
    private string searchFilter = "";
    private int activeTabIndex = 0;
    private RealTimeMap? realTimeMap; // reference to map control   
    private int mapMetersPerPixel = 0;
    private int mapZoomLevel = 0;
    private int mapPointHitDistance = 100; //meters
    private const int mapPointRadius = 8; //pixels
    private PagedResultDto<Observation>? observationSearchResult = null;
    private System.Text.Json.JsonSerializerOptions jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(),
            new GeoJsonConverter()
        }
    };
    private RealTimeMap.LoadParameters mapSettings = new RealTimeMap.LoadParameters()
    {
        location = new RealTimeMap.Location()
        {
            latitude = 58.4501715,
            longitude = 15.1107672,
        },
        zoomLevel = 6,
        basemap = new RealTimeMap.Basemap()
        {
            basemapLayers = new List<RealTimeMap.BasemapLayer>()
            {
                new RealTimeMap.BasemapLayer()
                {
                    url = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                    attribution = "©Open Street Map",
                    title = "Open Street Map",
                    detectRetina = true
                },
                new RealTimeMap.BasemapLayer()
                {
                    url = "https://tile.opentopomap.org/{z}/{x}/{y}.png",
                    attribution = "Open Topo",
                    title = "Open Topo",
                    detectRetina = true
                }
            }
        }
    };

    private async Task OnSearchObservationsAsync()
    {
        if (string.IsNullOrWhiteSpace(searchFilter))
        {
            return;
        }

        SearchFilterInternalDto? searchFilterInternal = null;
        try
        {
            searchFilterInternal = System.Text.Json.JsonSerializer.Deserialize<SearchFilterInternalDto>(searchFilter, jsonSerializerOptions);
            searchFilterInternal!.Output = new OutputFilterExtendedDto()
            {
                FieldSet = OutputFieldSet.All
            };
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Invalid search filter. {ex.Message}");
            return;
        }

        try
        {
            observationSearchResult = await searchService.SearchObservations(searchFilterInternal, 0, 10);
            await AddMapResultAsync();
            activeTabIndex = 1;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error when calling search service. {ex.Message}");
            return;
        }
    }

    private async Task AddMapResultAsync()
    {
        if (observationSearchResult == null || observationSearchResult.Records == null || observationSearchResult.Records.Count == 0 || realTimeMap == null)
        {
            return;
        }

        var points = new List<RealTimeMap.StreamPoint>();
        foreach (var obs in observationSearchResult.Records)
        {
            if (obs.Occurrence == null || obs.Location == null) continue;
            var point = new RealTimeMap.StreamPoint
            {
                //guid = obs.Occurrence.OccurrenceId, //the use of GUID identification is mandatory
                guid = Guid.NewGuid(),
                latitude = obs.Location.DecimalLatitude!.Value,      //the latitude of the map click point
                longitude = obs.Location.DecimalLongitude!.Value,    //the longitude of the map click point
                type = "observation",
                value = obs
            };

            points.Add(point);
        }

        //await realTimeMap.Geometric.Points.upload(points);
        await realTimeMap.Geometric.Points.add(points.ToArray());
    }

    private void OnZoomLevelEndChange(RealTimeMap.MapZoomEventArgs args)
    {
        UpdateMapPointHitDistance(args.zoomLevel, args.centerOfView);
    }

    private async Task OnAfterMapLoadedAsync(RealTimeMap.MapEventArgs args)
    {
        await Task.Delay(100); // Slight delay to ensure map is fully ready, fixes JavaScript error in some cases        

        UpdateMapPointHitDistance(args.zoomLevel, args.centerOfView);
        args.sender.Geometric.Points.clusteringAfterCollectionUpdate = true;
        args.sender.Geometric.Points.clusteringConfiguration = new LeafletForBlazor.RealTime.points.ClusteringConfiguration()
        {
            showCoverageOnHover = false,     // Show coverage on hover
            spiderfyOnMaxZoom = true,       // Spiderfy markers when zoomed in
            zoomToBoundsOnClick = true,    // Zoom to bounds when clicking on a cluster
            maxClusterRadius = 120,         // Maximum radius of a cluster when it is not zoomed in px
        };


        args.sender.Geometric.Points.OnClusterClick += OnClusterClick;
        args.sender.Geometric.Points.Appearance(item => item.type != "selected").pattern = new RealTimeMap.PointSymbol()
        {
            color = "black",
            opacity = 1.0,
            fillColor = "blue",
            fillOpacity = 0.5,
            radius = 8,
            weight = 2
        };

        args.sender.Geometric.Points.Appearance(m => m.type == "selected").pattern = new RealTimeMap.PointSymbol()
        {
            color = "black",
            opacity = 1.0,
            fillColor = "red",
            fillOpacity = 0.8,
            radius = 8,
            weight = 2
        };

        await AddMapResultAsync();
    }

    public void OnClusterClick(object? sender, LeafletForBlazor.RealTime.points.ClusteringEventArgs args)
    {
        // Handle the cluster click event
        //you receive bounds of the cluster, location of mouse click and guids
    }

    private void LoadExample(int? id)
    {
        searchFilter = TaxonSearchFilterSamples.Samples[id ?? 1].Json;
    }

    public void OnMapClick(RealTimeMap.ClicksMapArgs value)
    {
        var foundPoints = new List<RealTimeMap.StreamPoint>();
        foundPoints = value.sender.Geometric.Points.getItems(point => value.sender.Geometric.Computations.distance(
            point,
            new RealTimeMap.StreamPoint() { latitude = value.location.latitude, longitude = value.location.longitude },
            RealTimeMap.UnitOfMeasure.meters
            ) <= 100
        );

        // Remove selected state
        foreach (var item in value.sender.Geometric.Points.getItems())
        {
            item.type = "observation";
            //value.sender.Geometric.Points.update(item);
        }

        foreach (var item in foundPoints)
        {
            item.type = "selected";
            //value.sender.Geometric.Points.update(item);
        }

        foreach (var item in value.sender.Geometric.Points.getItems().ToList())
        {
            value.sender.Geometric.Points.update(item);
        }
    }

    private void UpdateMapPointHitDistance(int zoomLevel, RealTimeMap.Location location)
    {
        mapZoomLevel = zoomLevel;
        mapMetersPerPixel = (int)GetMetersPerPixel(location.latitude, zoomLevel);
        mapPointHitDistance = mapPointRadius * mapMetersPerPixel;
    }

    private double GetMetersPerPixel(double latitude, int zoom)
    {
        double earthCircumference = 40075016.686; // meter
        double metersPerPixel = earthCircumference / (256 * Math.Pow(2, zoom));
        return metersPerPixel * Math.Cos(latitude * Math.PI / 180);
    }
}