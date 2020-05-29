using System.Collections.Generic;
using System.IO;
using System.Linq;
using DwC_A.Factories;
using DwC_A.Meta;

namespace DwC_A
{
    internal class FileReader : IFileReaderAggregate
    {
        private const int BufferSize = 65536; //TODO: Make this configurable to allow tuning
        private readonly StreamReader streamReader;

        public FileReader(string fileName,
            IRowFactory rowFactory,
            ITokenizer tokenizer,
            IFileMetaData fileMetaData)
        {
            FileName = fileName;
            FileMetaData = fileMetaData;
            FileReaderUtils.ValidateLineEnds(fileMetaData.LinesTerminatedBy);
            streamReader = new StreamReader(rowFactory, tokenizer, fileMetaData);
        }

        public IEnumerable<IRow> Rows
        {
            get
            {
                using (var stream = new FileStream(FileName,
                    FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, false))
                {
                    foreach (var row in streamReader.ReadRows(stream))
                    {
                        yield return row;
                    }
                }
            }
        }

        public IEnumerable<IRow> HeaderRows => Rows.Take(FileMetaData.HeaderRowCount);

        public IEnumerable<IRow> DataRows => Rows.Skip(FileMetaData.HeaderRowCount);

        public async IAsyncEnumerable<IRow> GetRowsAsync()
        {
            using (var stream = new FileStream(FileName,
                FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true))
            {
                await foreach (var row in streamReader.ReadRowsAsync(stream))
                {
                    yield return row;
                }
            }
        }

        public FieldType TryGetFieldMetaData(string term)
        {
            return FileMetaData.Fields.FirstOrDefault(m => m.Term == term);
        }

        public int GetIdIndex()
        {
            return FileMetaData.Id?.Index ?? 0;
        }

        public async IAsyncEnumerable<IRow> GetHeaderRowsAsync()
        {
            var count = 0;
            await foreach (var row in GetRowsAsync())
            {
                if (count < FileMetaData.HeaderRowCount)
                {
                    yield return row;
                }
                else
                {
                    break;
                }

                count++;
            }
        }

        public async IAsyncEnumerable<IRow> GetDataRowsAsync()
        {
            var count = 0;
            await foreach (var row in GetRowsAsync())
            {
                if (count >= FileMetaData.HeaderRowCount)
                {
                    yield return row;
                }
                else
                {
                    count++;
                }
            }
        }

        public string FileName { get; }

        public IFileMetaData FileMetaData { get; }
    }
}