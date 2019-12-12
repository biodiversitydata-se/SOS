using System.IO;
using System.Xml;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Export.IO.DwcArchive
{
    /// <summary>
    /// Creates an DwC Archive meta.xml file.
    /// </summary>
    public static class DwcProcessInfoFileWriter
    {
        private static string _elementNamespace = "http://rs.tdwg.org/dwc/text/";

        /// <summary>
        /// Create a process info xml file
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="processInfo"></param>
        public static void CreateProcessInfoFile(Stream stream, ProcessInfo processInfo)
        {
            var doc = new XmlDocument();

            // Create header
            var header = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(header);

            // Create root node
            var processNode = doc.CreateElement("process", _elementNamespace);
            
            // Add root attributes
            var processStartAttribute = doc.CreateAttribute("start");
            processStartAttribute.Value = processInfo.Start.ToString("O");
            processNode.Attributes.Append(processStartAttribute);

            var processEndAttribute = doc.CreateAttribute("end");
            processEndAttribute.Value = processInfo.End.ToString("O");
            processNode.Attributes.Append(processEndAttribute);

            if (processInfo.VerbatimInfo != null)
            {
                // Add verbatim info
                foreach (var verbatimInfo in processInfo.VerbatimInfo)
                {
                    var verbatimInfoNode = CreateHarvestInfoNode(doc, "verbatim", verbatimInfo);

                    if (verbatimInfo.Metadata == null)
                    {
                        continue;
                    }

                    // Add verbatim info
                    foreach (var metadata in verbatimInfo.Metadata)
                    {
                        var metadataNode = CreateHarvestInfoNode(doc, "metadata", metadata);

                        verbatimInfoNode.AppendChild(metadataNode);
                    }

                    processNode.AppendChild(verbatimInfoNode);
                }
            }

            doc.AppendChild(processNode);
            doc.Save(stream);
        }

        /// <summary>
        /// Create a harvest information node
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="nodeName"></param>
        /// <param name="harvestInfo"></param>
        /// <returns></returns>
        private static XmlNode CreateHarvestInfoNode(XmlDocument doc, string nodeName, HarvestInfo harvestInfo)
        {
            // Create new node
            var harvestInfoNode = doc.CreateElement(nodeName, _elementNamespace);

            // Add verbatim info attributes
            var providerAttribute = doc.CreateAttribute("provider");
            providerAttribute.Value = harvestInfo.DataProvider.ToString();
            harvestInfoNode.Attributes.Append(providerAttribute);

            var nameAttribute = doc.CreateAttribute("name");
            nameAttribute.Value = harvestInfo.Id;
            harvestInfoNode.Attributes.Append(nameAttribute);

            var startAttribute = doc.CreateAttribute("start");
            startAttribute.Value = harvestInfo.Start.ToString("O");
            harvestInfoNode.Attributes.Append(startAttribute);

            var endAttribute = doc.CreateAttribute("end");
            endAttribute.Value = harvestInfo.End.ToString("O");
            harvestInfoNode.Attributes.Append(endAttribute);

            return harvestInfoNode;
        }
    }
}
