using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// XML extensions
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Convert XDocument to byte array
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static async Task<byte[]> ToBytesAsync(this XDocument xml)
        {
            var settings = new XmlWriterSettings { Async = true, OmitXmlDeclaration = false, CheckCharacters = true, Encoding = Encoding.UTF8 };
            await using var memoryStream = new MemoryStream();
            await using var xmlWriter = XmlWriter.Create(memoryStream, settings);
            await xml.WriteToAsync(xmlWriter, CancellationToken.None);
            await xmlWriter.FlushAsync();
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Cast byte xml array to xml document
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<XDocument> ToXmlAsync(this byte[] data)
        {
            if (!data?.Any() ?? true)
            {
                return null;
            }
            return await new MemoryStream(data).ToXmlAsync();
        }

        /// <summary>
        /// Cast xml stream to xml document
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<XDocument> ToXmlAsync(this Stream stream)
        {
            if (stream == null)
            {
                return null;
            }
            return await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
        }
    }
}
