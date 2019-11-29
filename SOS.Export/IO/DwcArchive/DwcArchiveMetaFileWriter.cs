using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SOS.Export.Enums;
using SOS.Export.Models;

namespace SOS.Export.IO.DwcArchive
{
    /// <summary>
    /// Creates an DwC Archive meta.xml file.
    /// </summary>
    public static class DwcArchiveMetaFileWriter
    {
        private static string elementNamespace = "http://rs.tdwg.org/dwc/text/";

        /// <summary>
        /// Creates the meta XML file.
        /// TODO async?
        /// </summary>
        /// <param name="stream">The stream to write the meta file to.</param>
        /// <param name="fieldDescriptions">The field descriptions.</param>
        public static void CreateMetaXmlFile(Stream stream, IList<FieldDescription> fieldDescriptions)
        {
            if (fieldDescriptions == null || fieldDescriptions.Count == 0)
            {
                throw new ArgumentException("No field descriptions were provided. You need at least provide the OccurrenceID.");
            }
            if (fieldDescriptions.First().Id != (int)FieldDescriptionId.OccurrenceID)
            {
                throw new ArgumentException("OccurrenceID must be first in fieldDescriptions list.");
            }

            XmlDocument doc = new XmlDocument();
            CreateHeaderNode(doc);
            var archiveNode = CreateArchiveNode(doc);
            CreateCoreNode(fieldDescriptions, doc, archiveNode);
            // Todo - Add extensions
            doc.Save(stream);
        }

        private static void CreateHeaderNode(XmlDocument doc)
        {
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);
        }

        private static XmlNode CreateArchiveNode(XmlDocument doc)
        {
            // Create: <archive xmlns = "http://rs.tdwg.org/dwc/text/" metadata="eml.xml">
            XmlNode archiveNode = doc.CreateElement("archive", elementNamespace);
            doc.AppendChild(archiveNode);
            XmlAttribute archiveMetadataAttribute = doc.CreateAttribute("metadata");
            archiveMetadataAttribute.Value = "eml.xml";
            archiveNode.Attributes.Append(archiveMetadataAttribute);
            return archiveNode;
        }

        private static void CreateCoreNode(IList<FieldDescription> fieldDescriptions, XmlDocument doc, XmlNode archiveNode)
        {
            // Create: <core encoding = "UTF-8" fieldsTerminatedBy = "\t" linesTerminatedBy = "\n" fieldsEnclosedBy = "" ignoreHeaderLines = "1" rowType = "http://rs.tdwg.org/dwc/terms/Occurrence" >
            XmlNode coreNode = doc.CreateElement("core", elementNamespace);
            archiveNode.AppendChild(coreNode);
            // encoding = "UTF-8"
            XmlAttribute coreNodeEncodingAttribute = doc.CreateAttribute("encoding");
            coreNodeEncodingAttribute.Value = "UTF-8";
            coreNode.Attributes.Append(coreNodeEncodingAttribute);
            // fieldsTerminatedBy = "\t"
            XmlAttribute coreNodeFieldsTerminatedByAttribute = doc.CreateAttribute("fieldsTerminatedBy");
            coreNodeFieldsTerminatedByAttribute.Value = "\\t";
            coreNode.Attributes.Append(coreNodeFieldsTerminatedByAttribute);
            // linesTerminatedBy = "\n"
            XmlAttribute coreNodeLinesTerminatedByAttribute = doc.CreateAttribute("linesTerminatedBy");
            coreNodeLinesTerminatedByAttribute.Value = "\\n";
            coreNode.Attributes.Append(coreNodeLinesTerminatedByAttribute);
            // fieldsEnclosedBy = ""
            XmlAttribute coreNodeFieldsEnclosedByAttribute = doc.CreateAttribute("fieldsEnclosedBy");
            coreNodeFieldsEnclosedByAttribute.Value = "";
            coreNode.Attributes.Append(coreNodeFieldsEnclosedByAttribute);
            // ignoreHeaderLines = "1"
            XmlAttribute coreNodeIgnoreHeaderLinesAttribute = doc.CreateAttribute("ignoreHeaderLines");
            coreNodeIgnoreHeaderLinesAttribute.Value = "1";
            coreNode.Attributes.Append(coreNodeIgnoreHeaderLinesAttribute);
            // rowType = "http://rs.tdwg.org/dwc/terms/Occurrence"
            XmlAttribute coreNodeRowTypeAttribute = doc.CreateAttribute("rowType");
            coreNodeRowTypeAttribute.Value = "http://rs.tdwg.org/dwc/terms/Occurrence";
            coreNode.Attributes.Append(coreNodeRowTypeAttribute);

            //Create:
            //< files >
            //  < location > taxon.csv </ location >
            //</ files >
            XmlNode filesNode = doc.CreateElement("files", elementNamespace);
            XmlNode locationNode = doc.CreateElement("location", elementNamespace);
            locationNode.AppendChild(doc.CreateTextNode("occurrence.csv"));
            filesNode.AppendChild(locationNode);
            coreNode.AppendChild(filesNode);

            // Create: < id index = "0" />
            XmlNode idNode = doc.CreateElement("id", elementNamespace);
            XmlAttribute idNodeIndexAttribute = doc.CreateAttribute("index");
            idNodeIndexAttribute.Value = "0";
            idNode.Attributes.Append(idNodeIndexAttribute);
            coreNode.AppendChild(idNode);

            for (int i = 0; i < fieldDescriptions.Count; i++)
            {
                // Create field rows:
                // < field index = "1" term = "http://rs.tdwg.org/dwc/terms/occurrenceID" />    
                // ...
                XmlNode fieldNode = doc.CreateElement("field", elementNamespace);
                XmlAttribute fieldNodeIndexAttribute = doc.CreateAttribute("index");
                fieldNodeIndexAttribute.Value = i.ToString();
                fieldNode.Attributes.Append(fieldNodeIndexAttribute);
                XmlAttribute fieldNodeTermAttribute = doc.CreateAttribute("term");
                fieldNodeTermAttribute.Value = fieldDescriptions[i].DwcIdentifier;
                fieldNode.Attributes.Append(fieldNodeTermAttribute);
                coreNode.AppendChild(fieldNode);
            }
        }
    }
}
