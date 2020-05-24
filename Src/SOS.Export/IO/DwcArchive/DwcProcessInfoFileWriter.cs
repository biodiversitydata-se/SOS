using System.IO;
using System.Xml;
using SOS.Lib.Models.Processed.ProcessInfo;

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

            var processSuccessAttribute = doc.CreateAttribute("status");
            processSuccessAttribute.Value = processInfo.Status.ToString();
            processNode.Attributes.Append(processSuccessAttribute);

            if (processInfo.ProvidersInfo != null)
            {
                // Add verbatim info
                foreach (var providerInfo in processInfo.ProvidersInfo)
                {
                    var providerInfoNode = CreateProviderInfoNode(doc, "verbatim", providerInfo);

                    if (providerInfo.MetadataInfo != null)
                    {
                        // Add verbatim info
                        foreach (var metadata in providerInfo.MetadataInfo)
                        {
                            var metadataNode = CreateProviderInfoNode(doc, "metadata", metadata);

                            providerInfoNode.AppendChild(metadataNode);
                        }
                    }

                    processNode.AppendChild(providerInfoNode);
                }
            }

            doc.AppendChild(processNode);
            doc.Save(stream);
        }

        /// <summary>
        /// Create a provider information node
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="nodeName"></param>
        /// <param name="providerInfo"></param>
        /// <returns></returns>
        private static XmlNode CreateProviderInfoNode(XmlDocument doc, string nodeName, ProviderInfo providerInfo)
        {
            // Create new node
            var providerInfoNode = doc.CreateElement(nodeName, _elementNamespace);

            // Add verbatim info attributes
            var providerAttribute = doc.CreateAttribute("provider");
            providerAttribute.Value = providerInfo.DataProviderType.ToString();
            providerInfoNode.Attributes.Append(providerAttribute);

            var harvestCountAttribute = doc.CreateAttribute("harvest-count");
            harvestCountAttribute.Value = providerInfo.HarvestCount.ToString();
            providerInfoNode.Attributes.Append(harvestCountAttribute);

            var harvestStartAttribute = doc.CreateAttribute("harvest-start");
            harvestStartAttribute.Value = providerInfo.HarvestStart?.ToString("O");
            providerInfoNode.Attributes.Append(harvestStartAttribute);

            var harvestEndAttribute = doc.CreateAttribute("harvest-end");
            harvestEndAttribute.Value = providerInfo.HarvestEnd?.ToString("O");
            providerInfoNode.Attributes.Append(harvestEndAttribute);

            var harvestStatusAttribute = doc.CreateAttribute("harvest-end");
            harvestStatusAttribute.Value = providerInfo.HarvestStatus.ToString();
            providerInfoNode.Attributes.Append(harvestStatusAttribute);

            var processCountAttribute = doc.CreateAttribute("process-count");
            processCountAttribute.Value = providerInfo.ProcessCount.ToString();
            providerInfoNode.Attributes.Append(processCountAttribute);

            var processStartAttribute = doc.CreateAttribute("process-start");
            processStartAttribute.Value = providerInfo.ProcessStart.ToString("O");
            providerInfoNode.Attributes.Append(processStartAttribute);

            var processEndAttribute = doc.CreateAttribute("process-end");
            processEndAttribute.Value = providerInfo.ProcessEnd?.ToString("O");
            providerInfoNode.Attributes.Append(processEndAttribute);

            var processStatusAttribute = doc.CreateAttribute("process-end");
            processStatusAttribute.Value = providerInfo.ProcessStatus.ToString();
            providerInfoNode.Attributes.Append(processStatusAttribute);

            return providerInfoNode;
        }
    }
}
