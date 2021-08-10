using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive;
using Xunit;

namespace SOS.Export.UnitTests.IO.DwcArchive
{
    public class DwcArchiveMetaFileWriterTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "DwcArchiveUnit")]
        public void Create_meta_xml_file_fails_when_occurrenceId_field_description_is_not_first()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var memoryStream = new MemoryStream();
            var fieldDescriptions = FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions().ToList();
            fieldDescriptions.RemoveAt(0);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => DwcArchiveMetaFileWriter.CreateMetaXmlFile(memoryStream, fieldDescriptions, new List<DwcaFilePart>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("OccurrenceID must be first in fieldDescriptions list.");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "DwcArchiveUnit")]
        public void OccurrenceId_field_description_must_be_first_when_creating_meta_xml_file()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var memoryStream = new MemoryStream();
            var fieldDescriptions = FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            DwcArchiveMetaFileWriter.CreateMetaXmlFile(memoryStream, fieldDescriptions.ToList(), new List<DwcaFilePart>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert - Read XML
            //-----------------------------------------------------------------------------------------------------------
            var readMemoryStream = new MemoryStream(memoryStream.ToArray());
            var xmlDocument = XDocument.Load(readMemoryStream);
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("x", "http://rs.tdwg.org/dwc/text/");
            var coreNode = xmlDocument.XPathSelectElement("/x:archive/x:core", namespaceManager);
            var locationNode = xmlDocument.XPathSelectElement("/x:archive/x:core/x:files/x:location", namespaceManager);
            var firstFieldNode = xmlDocument.XPathSelectElement("/x:archive/x:core/x:field", namespaceManager);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            coreNode.Attribute("rowType")?.Value.Should().Be("http://rs.tdwg.org/dwc/terms/Occurrence");
            locationNode.Value.Should().Be("occurrence.csv");
            firstFieldNode.Attribute("term")?.Value.Should().Be("http://rs.tdwg.org/dwc/terms/occurrenceID");
        }
    }
}