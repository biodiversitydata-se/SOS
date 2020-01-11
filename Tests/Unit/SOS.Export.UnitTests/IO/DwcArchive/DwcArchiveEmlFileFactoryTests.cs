using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using FluentAssertions;
using SOS.Export.IO.DwcArchive;
using Xunit;

namespace SOS.Export.UnitTests.IO.DwcArchive
{
    public class DwcArchiveEmlFileFactoryTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "DwcArchiveUnit")]
        public async Task CreateEmlXmlFile_Success()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var memoryStream = new MemoryStream();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            await DwCArchiveEmlFileFactory.CreateEmlXmlFileAsync(memoryStream);

            ////-----------------------------------------------------------------------------------------------------------
            //// Assert - Read XML
            ////-----------------------------------------------------------------------------------------------------------
            var readMemoryStream = new MemoryStream(memoryStream.ToArray());
            var xmlDocument = XDocument.Load(readMemoryStream);
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("eml", "eml://ecoinformatics.org/eml-2.1.1");
            var pubDate = xmlDocument.XPathSelectElement("/eml:eml/dataset/pubDate", namespaceManager);
            
            ////-----------------------------------------------------------------------------------------------------------
            //// Assert
            ////-----------------------------------------------------------------------------------------------------------
            pubDate.Value.Should().Be(DateTime.Now.ToString("yyyy-MM-dd"));
        }
    }
}
