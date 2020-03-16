using DwC_A.Factories;
using DwC_A.Meta;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DwC_A
{
    internal class FileReader : IFileReaderAggregate
    {
        const int BufferSize = 65536;   //TODO: Make this configurable to allow tuning
        private readonly StreamReader streamReader;

        public FileReader(string fileName,
            IRowFactory rowFactory,
            ITokenizer tokenizer,
            IFileMetaData fileMetaData)
        {
            this.FileName = fileName;
            this.FileMetaData = fileMetaData;
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
                    foreach(var row in streamReader.ReadRows(stream))
                    {
                        yield return row;
                    }
                }
            }
        }

        public IEnumerable<IRow> HeaderRows
        {
            get
            {
                return Rows.Take(FileMetaData.HeaderRowCount);
            }
        }

        public IEnumerable<IRow> DataRows
        {
            get
            {
                return Rows.Skip(FileMetaData.HeaderRowCount);
            }
        }

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

        public async IAsyncEnumerable<IRow> GetHeaderRowsAsync()
        {
            int count = 0;
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
            int count = 0;
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
