using DwC_A.Meta;

namespace DwC_A.Factories
{
    public interface IAbstractFactory
    {
        /// <summary>
        ///     Creates an archive reader
        /// </summary>
        /// <param name="fileName">Name of archive file or directory containing extracted files</param>
        /// <returns>ArchiveReader</returns>
        ArchiveReader CreateArchiveReader(string fileName);

        /// <summary>
        ///     This class is used internally to manage extraction of archive zip files
        /// </summary>
        /// <param name="fileName">Archive zip file name</param>
        /// <param name="outputPath">Path to extract archive to.  Leave null to use a temporary directory</param>
        /// <returns>IArchiveFolder</returns>
        IArchiveFolder CreateArchiveFolder(string fileName, string outputPath);

        /// <summary>
        ///     Creates a meta.xml metadata reader
        /// </summary>
        /// <returns>Meta data reader class</returns>
        IMetaDataReader CreateMetaDataReader();

        /// <summary>
        ///     Creates a line tokenizer
        /// </summary>
        /// <param name="fileAttributes">Metadata class containing file attributes for field separators and line terminators etc.</param>
        /// <returns>Tokenizer</returns>
        ITokenizer CreateTokenizer(IFileMetaData fileAttributes);

        /// <summary>
        ///     Creates a factory that returns IRow objects
        /// </summary>
        /// <returns>Row factory</returns>
        IRowFactory CreateRowFactory();

        /// <summary>
        ///     Creates a FileReader object.  Works for both core files and extensions.
        /// </summary>
        /// <param name="fileName">Name of file to read</param>
        /// <param name="fileMetaData">File meta data</param>
        /// <returns>File reader object</returns>
        IFileReaderAggregate CreateFileReader(string fileName,
            IFileMetaData fileMetaData);

        /// <summary>
        ///     Creates an object that is used to wrap CoreFileTypes and ExtensionFileTypes to a common interface
        /// </summary>
        /// <param name="coreFileType">CoreFileType raw meta data</param>
        /// <returns>Generic IFileMetaData object</returns>
        IFileMetaData CreateCoreMetaData(CoreFileType coreFileType);

        /// <summary>
        ///     Creates an object that is used to wrap CoreFileTypes and ExtensionFileTypes to a common interface
        /// </summary>
        /// <param name="extensionFileType">ExtensionFileType raw meta data</param>
        /// <returns>Generic IFileMetaData object</returns>
        IFileMetaData CreateExtensionMetaData(ExtensionFileType extensionFileType);
    }
}