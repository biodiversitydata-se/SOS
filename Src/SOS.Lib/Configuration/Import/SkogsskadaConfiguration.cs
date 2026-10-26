namespace SOS.Lib.Configuration.Import;

public class SkogsskadaConfiguration
{
    /// <summary>
    ///     Skogsskada connection settings.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    ///     The number of observations that will be fetched in each loop.
    /// </summary>
    public int ChunkSize { get; set; } = 10000;
}
