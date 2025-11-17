using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SOS.Lib.Extensions;

/// <summary>
/// XML extensions
/// </summary>
public static class XmlExtensions
{
    extension(XDocument xml)
    {
        /// <summary>
        /// Convert XDocument to byte array
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> ToBytesAsync()
        {
            var settings = new XmlWriterSettings { Async = true, OmitXmlDeclaration = false, CheckCharacters = true, Encoding = Encoding.UTF8 };
            await using var memoryStream = new MemoryStream();
            await using var xmlWriter = XmlWriter.Create(memoryStream, settings);
            await xml.WriteToAsync(xmlWriter, CancellationToken.None);
            await xmlWriter.FlushAsync();
            return memoryStream.ToArray();
        }
    }

    extension(byte[] data)
    {
        /// <summary>
        /// Cast byte xml array to xml document
        /// </summary>
        /// <returns></returns>
        public async Task<XDocument> ToXmlAsync()
        {
            if (!data?.Any() ?? true)
            {
                return null;
            }
            return await new MemoryStream(data).ToXmlAsync();
        }
    }

    extension(Stream stream)
    {
        /// <summary>
        /// Cast xml stream to xml document
        /// </summary>
        /// <returns></returns>
        public async Task<XDocument> ToXmlAsync()
        {
            if (stream == null)
            {
                return null;
            }
            return await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
        }
    }
}
