using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SOS.Export.Models;
using SOS.Lib.Enums;

namespace SOS.Lib.IO.DwcArchive
{
    /// <summary>
    ///     Creates an DwC Archive meta.xml file.
    /// </summary>
    public static class DwcArchiveMetaFileWriter
    {
        private static readonly string elementNamespace = "http://rs.tdwg.org/dwc/text/";

        /// <summary>
        ///     Creates the meta XML file.
        ///     TODO async?
        /// </summary>
        /// <param name="stream">The stream to write the meta file to.</param>
        /// <param name="fieldDescriptions">The field descriptions.</param>
        /// <param name="dwcExtensions">The DwC-A extensions that is used.</param>
        public static void CreateMetaXmlFile(Stream stream, IList<FieldDescription> fieldDescriptions, ICollection<DwcaFilePart> dwcExtensions)
        {
            if (fieldDescriptions == null || fieldDescriptions.Count == 0)
            {
                throw new ArgumentException(
                    "No field descriptions were provided. You need at least provide the OccurrenceID.");
            }

            if (fieldDescriptions.First().Id != (int)FieldDescriptionId.OccurrenceID)
            {
                throw new ArgumentException("OccurrenceID must be first in fieldDescriptions list.");
            }

            var doc = new XmlDocument();
            CreateHeaderNode(doc);
            var archiveNode = CreateArchiveNode(doc);
            CreateCoreNode(fieldDescriptions, doc, archiveNode);
            if (dwcExtensions.Contains(DwcaFilePart.Emof)) AppendExtension(doc, archiveNode, ExtensionMetadata.EmofFactory.Create());
            if (dwcExtensions.Contains(DwcaFilePart.Multimedia)) AppendExtension(doc, archiveNode, ExtensionMetadata.SimpleMultimediaFactory.Create());
            doc.Save(stream);
        }

        public static void CreateEventMetaXmlFile(
            Stream stream,
            IList<FieldDescription> fieldDescriptions,
            ICollection<DwcaEventFilePart> dwcExtensions,
            IList<FieldDescription> occurrenceFieldDescriptions)
        {
            if (fieldDescriptions == null || fieldDescriptions.Count == 0)
            {
                throw new ArgumentException(
                    "No field descriptions were provided. You need at least provide the EventID.");
            }


            var doc = new XmlDocument();
            CreateHeaderNode(doc);
            var archiveNode = CreateArchiveNode(doc);
            CreateEventCoreNode(fieldDescriptions, doc, archiveNode);
            int occurrenceEventIdIndex =
                occurrenceFieldDescriptions.IndexOf(occurrenceFieldDescriptions.Single(m =>
                    m.FieldDescriptionId == FieldDescriptionId.EventID));
            if (dwcExtensions.Contains(DwcaEventFilePart.Occurrence)) AppendExtension(doc, archiveNode, ExtensionMetadata.OccurrenceFactory.Create(occurrenceFieldDescriptions), occurrenceEventIdIndex);
            if (dwcExtensions.Contains(DwcaEventFilePart.Emof)) AppendExtension(doc, archiveNode, ExtensionMetadata.EmofFactory.Create(isEventCore: true));
            if (dwcExtensions.Contains(DwcaEventFilePart.Multimedia)) AppendExtension(doc, archiveNode, ExtensionMetadata.SimpleMultimediaFactory.Create());
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
            var archiveMetadataAttribute = doc.CreateAttribute("metadata");
            archiveMetadataAttribute.Value = "eml.xml";
            archiveNode.Attributes.Append(archiveMetadataAttribute);
            return archiveNode;
        }

        private static void CreateCoreNode(IList<FieldDescription> fieldDescriptions, XmlDocument doc,
            XmlNode archiveNode)
        {
            // Create: <core encoding = "UTF-8" fieldsTerminatedBy = "\t" linesTerminatedBy = "\n" fieldsEnclosedBy = "" ignoreHeaderLines = "1" rowType = "http://rs.tdwg.org/dwc/terms/Occurrence" >
            XmlNode coreNode = doc.CreateElement("core", elementNamespace);
            archiveNode.AppendChild(coreNode);
            // encoding = "UTF-8"
            var coreNodeEncodingAttribute = doc.CreateAttribute("encoding");
            coreNodeEncodingAttribute.Value = "UTF-8";
            coreNode.Attributes.Append(coreNodeEncodingAttribute);
            // fieldsTerminatedBy = "\t"
            var coreNodeFieldsTerminatedByAttribute = doc.CreateAttribute("fieldsTerminatedBy");
            coreNodeFieldsTerminatedByAttribute.Value = "\\t";
            coreNode.Attributes.Append(coreNodeFieldsTerminatedByAttribute);
            // linesTerminatedBy = "\n"
            var coreNodeLinesTerminatedByAttribute = doc.CreateAttribute("linesTerminatedBy");
            coreNodeLinesTerminatedByAttribute.Value = "\\n";
            coreNode.Attributes.Append(coreNodeLinesTerminatedByAttribute);
            // fieldsEnclosedBy = ""
            var coreNodeFieldsEnclosedByAttribute = doc.CreateAttribute("fieldsEnclosedBy");
            coreNodeFieldsEnclosedByAttribute.Value = "";
            coreNode.Attributes.Append(coreNodeFieldsEnclosedByAttribute);
            // ignoreHeaderLines = "1"
            var coreNodeIgnoreHeaderLinesAttribute = doc.CreateAttribute("ignoreHeaderLines");
            coreNodeIgnoreHeaderLinesAttribute.Value = "1";
            coreNode.Attributes.Append(coreNodeIgnoreHeaderLinesAttribute);
            // rowType = "http://rs.tdwg.org/dwc/terms/Occurrence"
            var coreNodeRowTypeAttribute = doc.CreateAttribute("rowType");
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
            var idNodeIndexAttribute = doc.CreateAttribute("index");
            idNodeIndexAttribute.Value = "0";
            idNode.Attributes.Append(idNodeIndexAttribute);
            coreNode.AppendChild(idNode);

            for (var i = 0; i < fieldDescriptions.Count; i++)
            {
                // Create field rows:
                // < field index = "1" term = "http://rs.tdwg.org/dwc/terms/occurrenceID" />    
                // ...
                XmlNode fieldNode = doc.CreateElement("field", elementNamespace);
                var fieldNodeIndexAttribute = doc.CreateAttribute("index");
                fieldNodeIndexAttribute.Value = i.ToString();
                fieldNode.Attributes.Append(fieldNodeIndexAttribute);
                var fieldNodeTermAttribute = doc.CreateAttribute("term");
                fieldNodeTermAttribute.Value = fieldDescriptions[i].DwcIdentifier;
                fieldNode.Attributes.Append(fieldNodeTermAttribute);
                coreNode.AppendChild(fieldNode);
            }
        }

        private static void CreateEventCoreNode(IList<FieldDescription> fieldDescriptions, XmlDocument doc,
            XmlNode archiveNode)
        {
            // Create: <core encoding = "UTF-8" fieldsTerminatedBy = "\t" linesTerminatedBy = "\n" fieldsEnclosedBy = "" ignoreHeaderLines = "1" rowType = "http://rs.tdwg.org/dwc/terms/Event" >
            XmlNode coreNode = doc.CreateElement("core", elementNamespace);
            archiveNode.AppendChild(coreNode);
            // encoding = "UTF-8"
            var coreNodeEncodingAttribute = doc.CreateAttribute("encoding");
            coreNodeEncodingAttribute.Value = "UTF-8";
            coreNode.Attributes.Append(coreNodeEncodingAttribute);
            // fieldsTerminatedBy = "\t"
            var coreNodeFieldsTerminatedByAttribute = doc.CreateAttribute("fieldsTerminatedBy");
            coreNodeFieldsTerminatedByAttribute.Value = "\\t";
            coreNode.Attributes.Append(coreNodeFieldsTerminatedByAttribute);
            // linesTerminatedBy = "\n"
            var coreNodeLinesTerminatedByAttribute = doc.CreateAttribute("linesTerminatedBy");
            coreNodeLinesTerminatedByAttribute.Value = "\\n";
            coreNode.Attributes.Append(coreNodeLinesTerminatedByAttribute);
            // fieldsEnclosedBy = ""
            var coreNodeFieldsEnclosedByAttribute = doc.CreateAttribute("fieldsEnclosedBy");
            coreNodeFieldsEnclosedByAttribute.Value = "";
            coreNode.Attributes.Append(coreNodeFieldsEnclosedByAttribute);
            // ignoreHeaderLines = "1"
            var coreNodeIgnoreHeaderLinesAttribute = doc.CreateAttribute("ignoreHeaderLines");
            coreNodeIgnoreHeaderLinesAttribute.Value = "1";
            coreNode.Attributes.Append(coreNodeIgnoreHeaderLinesAttribute);
            // rowType = "http://rs.tdwg.org/dwc/terms/Event"
            var coreNodeRowTypeAttribute = doc.CreateAttribute("rowType");
            coreNodeRowTypeAttribute.Value = "http://rs.tdwg.org/dwc/terms/Event";
            coreNode.Attributes.Append(coreNodeRowTypeAttribute);

            //Create:
            //< files >
            //  < location > taxon.csv </ location >
            //</ files >
            XmlNode filesNode = doc.CreateElement("files", elementNamespace);
            XmlNode locationNode = doc.CreateElement("location", elementNamespace);
            locationNode.AppendChild(doc.CreateTextNode("event.csv"));
            filesNode.AppendChild(locationNode);
            coreNode.AppendChild(filesNode);

            // Create: < id index = "0" />
            XmlNode idNode = doc.CreateElement("id", elementNamespace);
            var idNodeIndexAttribute = doc.CreateAttribute("index");
            idNodeIndexAttribute.Value = "0";
            idNode.Attributes.Append(idNodeIndexAttribute);
            coreNode.AppendChild(idNode);

            for (var i = 0; i < fieldDescriptions.Count; i++)
            {
                // Create field rows:
                // < field index = "1" term = "http://rs.tdwg.org/dwc/terms/occurrenceID" />    
                // ...
                XmlNode fieldNode = doc.CreateElement("field", elementNamespace);
                var fieldNodeIndexAttribute = doc.CreateAttribute("index");
                fieldNodeIndexAttribute.Value = i.ToString();
                fieldNode.Attributes.Append(fieldNodeIndexAttribute);
                var fieldNodeTermAttribute = doc.CreateAttribute("term");
                fieldNodeTermAttribute.Value = fieldDescriptions[i].DwcIdentifier;
                fieldNode.Attributes.Append(fieldNodeTermAttribute);
                coreNode.AppendChild(fieldNode);
            }
        }


        private static void AppendExtension(XmlDocument doc, XmlNode coreNode, ExtensionMetadata metaData, int coreIndex = 0)
        {
            var extension = doc.CreateElement("extension", elementNamespace);

            var attr = doc.CreateAttribute("encoding");
            attr.Value = "UTF8";
            extension.Attributes.Append(attr);

            attr = doc.CreateAttribute("fieldsTerminatedBy");
            attr.Value = @"\t";
            extension.Attributes.Append(attr);

            attr = doc.CreateAttribute("linesTerminatedBy");
            attr.Value = @"\n";
            extension.Attributes.Append(attr);

            attr = doc.CreateAttribute("fieldsEnclosedBy");
            attr.Value = "";
            extension.Attributes.Append(attr);

            attr = doc.CreateAttribute("ignoreHeaderLines");
            attr.Value = "1";
            extension.Attributes.Append(attr);

            attr = doc.CreateAttribute("rowType");
            attr.Value = metaData.RowType;
            extension.Attributes.Append(attr);

            var fileLocation = doc.CreateElement("location", elementNamespace);
            fileLocation.AppendChild(doc.CreateTextNode(metaData.FileLocation));


            //    <files>
            //    <location>VernacularName.tsv</location>
            //    </files>

            var fileElement = doc.CreateElement("files", elementNamespace);
            fileElement.AppendChild(fileLocation);
            extension.AppendChild(fileElement);

            attr = doc.CreateAttribute("index");
            attr.Value = coreIndex.ToString();

            // <coreid index="0" />

            var coreElement = doc.CreateElement("coreid", elementNamespace);
            coreElement.Attributes.Append(attr);
            extension.AppendChild(coreElement);

            foreach (var f in metaData.Fields.OrderBy(o => o.Index))
            {
                var field = doc.CreateElement("field", elementNamespace);


                attr = doc.CreateAttribute("index");
                attr.Value = f.Index.ToString();

                field.Attributes.Append(attr);

                attr = doc.CreateAttribute("term");
                attr.Value = f.Term;
                field.Attributes.Append(attr);

                extension.AppendChild(field);
            }

            coreNode.AppendChild(extension);
        }
    }
}