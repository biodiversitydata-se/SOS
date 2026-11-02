using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;

namespace SOS.Shared.Api.Extensions;

public static class ResponseCompressionExtensions
{
    /// <summary>Registers the shared SOS Brotli/gzip response compression settings, including HTTPS.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="compressionLevel">The compression level for both providers.</param>
    /// <param name="mimeTypes">
    /// The complete set of content types to compress. Null uses ASP.NET Core's default content types
    /// (including JSON, HTML, CSS and JavaScript), plus GeoJSON. An explicit list replaces that default set.
    /// </param>
    public static IServiceCollection AddSosResponseCompression(this IServiceCollection services,
        CompressionLevel compressionLevel = CompressionLevel.Fastest, IEnumerable<string>? mimeTypes = null)
    {
        var contentTypes = (mimeTypes ?? ResponseCompressionDefaults.MimeTypes.Append("application/geo+json")).ToArray();
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.MimeTypes = contentTypes;
        });
        services.Configure<BrotliCompressionProviderOptions>(options => options.Level = compressionLevel);
        services.Configure<GzipCompressionProviderOptions>(options => options.Level = compressionLevel);
        return services;
    }
}
