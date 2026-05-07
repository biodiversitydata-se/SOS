using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Import;

public class VirtualHerbariumServiceConfiguration : RestServiceConfiguration
{
    /// <summary>
    ///     If true, uses test file (sweden-occurrences-test.ndjson) instead of production file
    /// </summary>
    public bool UseTestFile { get; set; } = false;
}