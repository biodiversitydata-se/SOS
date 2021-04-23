using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using FluentAssertions;
using SOS.Lib.Factories;
using SOS.Lib.Models.Shared;
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

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var xmlDocument = await DwCArchiveEmlFileFactory.CreateEmlXmlFileAsync(DataProvider.FilterSubsetDataProvider);

            ////-----------------------------------------------------------------------------------------------------------
            //// Assert - Read XML
            ////-----------------------------------------------------------------------------------------------------------
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