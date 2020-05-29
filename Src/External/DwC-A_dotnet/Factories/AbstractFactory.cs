using DwC_A.Meta;

namespace DwC_A.Factories
{
    public abstract class AbstractFactory : IAbstractFactory
    {
        public ArchiveReader CreateArchiveReader(string fileName)
        {
            return new ArchiveReader(fileName, this);
        }

        public virtual IArchiveFolder CreateArchiveFolder(string fileName, string outputPath)
        {
            return new ArchiveFolder(fileName, outputPath);
        }

        public virtual IMetaDataReader CreateMetaDataReader()
        {
            return new MetaDataReader();
        }

        public virtual ITokenizer CreateTokenizer(IFileMetaData fileMetaData)
        {
            return new Tokenizer(fileMetaData);
        }

        public virtual IRowFactory CreateRowFactory()
        {
            return new RowFactory();
        }

        public virtual IFileReaderAggregate CreateFileReader(string fileName, IFileMetaData fileMetaData)
        {
            return new FileReader(fileName,
                CreateRowFactory(),
                CreateTokenizer(fileMetaData),
                fileMetaData);
        }

        public virtual IFileMetaData CreateCoreMetaData(CoreFileType coreFileType)
        {
            return new CoreFileMetaData(coreFileType);
        }

        public virtual IFileMetaData CreateExtensionMetaData(ExtensionFileType extensionFileType)
        {
            return new ExtensionFileMetaData(extensionFileType);
        }
    }
}