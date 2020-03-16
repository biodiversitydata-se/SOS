using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DwC_A.Meta
{
    internal class MetaDataReader : IMetaDataReader
    {
        private const string MetaFileName = "meta.xml";

        /// <summary>
        /// Deserializes the meta.xml file into an Archive object
        /// </summary>
        /// <param name="path">Path to the meta.xml file (excluding filename)</param>
        /// <returns>Archive object</returns>
        public Archive ReadMetaData(string path)
        {
            string fileName = Path.Combine(path, MetaFileName);
            if (File.Exists(fileName))
            {
                return DeserializeMetaDataFile(fileName);
            }
            return DefaultMetaData(path);
        }

        private Archive DefaultMetaData(string path)
        {
            var fileNames = Directory.GetFiles(path);
            var coreFileName = Path.GetFileName(fileNames.Single());
            var archive = new Archive()
            {
                Core = new CoreFileType()
            };
            archive.Core.Files.Add(coreFileName);
            return archive;
        }

        private Archive DeserializeMetaDataFile(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Archive));
            using (Stream stream = new FileStream(fileName, FileMode.Open))
            {
                return serializer.Deserialize(stream) as Archive;
            }
        }
    }
}
