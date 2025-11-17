namespace SOS.Lib.Configuration.Shared;

public class AreaConfiguration
{
    public int? SwedenExtentBufferKm { get; set; }

    /// <summary>
    /// If true, the area geometries will not be loaded.
    /// Use this to speed up load time when running integration tests.
    /// </summary>
    public bool ExcludeParishGeometries { get; set; } = false;
}