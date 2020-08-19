using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DwC_A.Factories;
using DwC_A.Meta;
using DwC_A.Terms;

namespace DwC_A
{
    /// <summary>
    ///     Reads a Darwin Core archive zip file or directory contents
    /// </summary>
    public class ArchiveReader : IDisposable
    {
        private readonly IAbstractFactory abstractFactory;
        private readonly IArchiveFolder archiveFolder;
        private readonly IFileReaderAggregate coreFile;
        private readonly IList<IFileReaderAggregate> extensionFiles = new List<IFileReaderAggregate>();
        private readonly IMetaDataReader metaDataReader;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="archivePath">
        ///     Relative or absolute file name for archive file or
        ///     the name of a directory containing the extracted archive files
        /// </param>
        public ArchiveReader(string archivePath) :
            this(archivePath, new DefaultFactory())
        {
        }

        public ArchiveReader(string archivePath, string outputPath) :
            this(archivePath, new DefaultFactory(), Path.Combine(outputPath, $"dwca-{Guid.NewGuid()}"))
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="archivePath">Fully qualified file name for archive file</param>
        /// <param name="abstractFactory">Factory to create tokenizers, readers etc.</param>
        /// <param name="outputPath">Output path where the zip file should be extracted. If null then the OS temp path is used.</param>
        public ArchiveReader(string archivePath, IAbstractFactory abstractFactory, string outputPath = null)
        {
            this.abstractFactory = abstractFactory ??
                                   throw new ArgumentNullException(nameof(abstractFactory));
            var fileAttributes = File.GetAttributes(archivePath);
            if ((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                //File is a directory.  Set the outputPath and continue
                OutputPath = string.IsNullOrEmpty(archivePath)
                    ? throw new ArgumentNullException(nameof(archivePath))
                    : archivePath;
            }
            else
            {
                //File is an archive file.  Extract to temp directory
                archiveFolder = abstractFactory.CreateArchiveFolder(archivePath, outputPath);
                OutputPath = archiveFolder.Extract();
            }

            FileName = archivePath;
            metaDataReader = abstractFactory.CreateMetaDataReader();
            MetaData = metaDataReader.ReadMetaData(OutputPath);
            //Create a core file reader
            var coreFileMetaData = abstractFactory.CreateCoreMetaData(MetaData.Core);
            coreFile = CreateFileReader(coreFileMetaData);
            //Create file readers for extensions
            foreach (var extension in MetaData.Extension)
            {
                var extensionFileName = extension.Files.FirstOrDefault();
                var extensionFileMetaData = abstractFactory.CreateExtensionMetaData(extension);
                extensionFiles.Add(CreateFileReader(extensionFileMetaData));
            }

            Extensions = new FileReaderCollection(extensionFiles);
        }

        /// <summary>
        ///     Relative or absolute path name for the archive file if one was specified in the constructor or null
        /// </summary>
        public string FileName { get; }

        /// <summary>
        ///     Path where archive is extracted to or the path specified in the constructor
        /// </summary>
        public string OutputPath { get; }

        /// <summary>
        ///     Raw meta data for archive
        /// </summary>
        public Archive MetaData { get; }

        /// <summary>
        ///     File reader for Core file
        /// </summary>
        public IFileReader CoreFile => coreFile;

        /// <summary>
        ///     Collection of file readers for extension files
        /// </summary>
        public FileReaderCollection Extensions { get; }

        public bool IsSamplingEventCore => CoreFile.FileMetaData.RowType == RowTypes.Event;

        /// <summary>
        ///     Async File reader for Core file
        /// </summary>
        /// <returns>Async File reader</returns>
        public IAsyncFileReader GetAsyncCoreFile()
        {
            return coreFile;
        }

        /// <summary>
        ///     Get async file reader for a specific row type.
        /// </summary>
        /// <param name="rowType"></param>
        /// <returns></returns>
        public IAsyncFileReader GetAsyncFileReader(string rowType)
        {
            if (MetaData.Core.RowType == rowType)
            {
                return GetAsyncCoreFile();
            }

            return Extensions.GetAsyncFileReadersByRowType(rowType).FirstOrDefault();
        }

        private IFileReaderAggregate CreateFileReader(IFileMetaData fileMetaData)
        {
            var fullFileName = Path.Combine(OutputPath, fileMetaData.FileName);
            return abstractFactory.CreateFileReader(fullFileName, fileMetaData);
        }

        /// <summary>
        ///     Used to cleanup extracted files.
        /// </summary>
        public void Delete()
        {
            archiveFolder.DeleteFolder();
        }

        #region IDisposable

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (archiveFolder != null && archiveFolder.ShouldCleanup)
                    {
                        Delete();
                    }
                }
            }

            disposed = true;
        }

        ~ArchiveReader()
        {
            Dispose(false);
        }

        #endregion

        public int GetNumberOfRowsInOccurrenceFile()
        {

            var fileReader = GetAsyncFileReader(RowTypes.Occurrence);
            if (fileReader == null) return 0;
            var numberOfRows = fileReader.GetNumberOfRows();
            var headerRowCount = fileReader.FileMetaData.HeaderRowCount;
            return numberOfRows - headerRowCount;
        }
    }
}